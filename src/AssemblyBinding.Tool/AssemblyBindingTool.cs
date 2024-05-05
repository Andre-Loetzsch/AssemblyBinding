using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;
using Oleander.Assembly.Binding.Tool.Html;
using Oleander.Assembly.Binding.Tool.Xml;
using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger<AssemblyBindingTool> logger)
{
    internal async Task<int> Execute(DirectoryInfo baseDirInfo, FileInfo? appConfigFileInfo, bool recursive, bool noReport, string branch, string configurationName)
    {
        if (recursive)
        {
            if (appConfigFileInfo != null)
            {
                logger.CreateMSBuildError("ABT1", "Option --app-config is not supported when --recursive is true!", "assembly-binding");
                return 1;
            }
        }
        else
        {
            if (appConfigFileInfo == null && noReport)
            {
                logger.CreateMSBuildError("ABT2", "No actions were taken! No configuration file was specified and the --no-report option was set to true.", "assembly-binding");
                return 2;
            }

            if (!string.IsNullOrEmpty(branch))
            {
                logger.CreateMSBuildError("ABT3", "The --branch option is only valid with the --recursive true option", "assembly-binding");
                return 3;
            }

            if (!string.IsNullOrEmpty(configurationName))
            {
                logger.CreateMSBuildError("ABT4", "The --configuration-name option is only valid with the --recursive true option", "assembly-binding");
                return 4;
            }
        }

        var result = 0;
        var links = new Dictionary<string, string>();
        var now = DateTime.Now;

        foreach (var toDo in this.CreateToDoItems(baseDirInfo, appConfigFileInfo, recursive, branch, configurationName))
        {
            var innerResult = await this.InnerExecuteAsync(toDo, noReport);

            if (innerResult > 0) result = innerResult;
            if (toDo.HtmlIndexPage == null || toDo.AppConfigFileInfo == null) continue;

            links[toDo.HtmlIndexPage] = toDo.AppConfigFileInfo.FullName;
        }

        if (links.Count == 0) return result;

        var htmlIndexFileName = links.Keys.First();

        if (links.Count > 1)
        {
            htmlIndexFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out", "index.html");
            await File.WriteAllTextAsync(htmlIndexFileName, HtmlIndex.Create(links, "Assembly binding reports", "Assembly binding index", "Select a link to get more information"));
        }

        if (string.IsNullOrEmpty(htmlIndexFileName)) return result;

        var psi = new ProcessStartInfo
        {
            FileName = htmlIndexFileName,
            UseShellExecute = true // Wichtig für .NET Core oder .NET 5+
        };

        var diff = (DateTime.Now - now); //1100

        return Process.Start(psi) == null ? 4 : result;
    }

    private async Task<int> InnerExecuteAsync(ToDo toDo, bool noReport)
    {
        //logger.CreateMSBuildMessage("ABT3", $"Load assemblies: {toDo.BaseDirInfo.GetShortPathInfo(20)}", "assembly-binding");
        var cache = await AssemblyBindingsBuilder.CreateAsync(toDo.BaseDirInfo);

        if (cache.Count == 0)
        {
            logger.CreateMSBuildWarning("ABT1", $"Directory '{toDo.BaseDirInfo.GetShortPathInfo(20)}' does not contain any assemblies!", "assembly-binding");
            return 0;
        }

        if (!noReport)
        {
            try
            {
                //logger.CreateMSBuildMessage("ABT4", "Create reports: ...", "assembly-binding");
                toDo.HtmlIndexPage = await cache.CreateReportsAsync(toDo.AppConfigFileInfo);
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT5", ex.Message, "assembly-binding");
                return 5;
            }
        }

        if (toDo.AppConfigFileInfo != null)
        {
            try
            {
                //logger.CreateMSBuildMessage("ABT5", $"Update config file: {toDo.AppConfigFileInfo.GetShortPathInfo(20)}", "assembly-binding");
                if (toDo.AppConfigFileInfo.Exists) toDo.AppConfigFileInfo.IsReadOnly = false;
                await cache.CreateOrUpdateApplicationConfigFileAsync(toDo.AppConfigFileInfo.FullName);
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT6", ex.Message, "assembly-binding");
                return 6;
            }
        }

        return 0;
    }

    private IEnumerable<ToDo> CreateToDoItems(DirectoryInfo baseDirInfo, FileInfo? appConfigFileInfo, bool recursive, string branch, string configurationName)
    {
        if (recursive)
        {
            var enumerationOptions = new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = true
            };

            logger.CreateMSBuildMessage("ABT1", "Processing data...", "assembly-binding");
            branch = string.IsNullOrEmpty(branch) ?
                Path.DirectorySeparatorChar.ToString() :
                string.Concat(Path.DirectorySeparatorChar, branch, Path.DirectorySeparatorChar);

            var configurationNames = string.IsNullOrEmpty(configurationName) ?
                new[] { "debug", "release" } :
                new[] { configurationName };


            var toDoList = baseDirInfo.GetFiles("*.config", enumerationOptions)
                .Where(x => x.Directory != null &&
                            !x.Directory.FullName.Contains($"{Path.DirectorySeparatorChar}obj",
                                StringComparison.InvariantCultureIgnoreCase) &&
                            !x.Directory.FullName.Contains($"{Path.DirectorySeparatorChar}TestResults",
                                StringComparison.InvariantCultureIgnoreCase) &&
                            x.Directory.FullName.Contains(branch, StringComparison.CurrentCultureIgnoreCase) &&
                            ApplicationConfiguration.IsAppConfigFile(x.FullName))
                .OrderBy(x => x.FullName)
                .Select(x => new ToDo(x.Directory!, x)).ToList();

            var count = toDoList.Count;
            
            foreach (var toDo in toDoList.ToList())
            {
                toDoList.Remove(toDo);
                logger.CreateMSBuildMessage("ABT2", $"=== Processing {count - toDoList.Count}/{count} ===", "assembly-binding");
                logger.CreateMSBuildMessage("ABT3", $"{toDo.BaseDirInfo.GetShortPathInfo(20)}", "assembly-binding");
                logger.CreateMSBuildMessage("ABT4", $"{toDo.AppConfigFileInfo.GetShortPathInfo(20)}", "assembly-binding");

                if (string.Equals(toDo.AppConfigFileInfo?.Name, "app.config", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (toDo.AppConfigFileInfo?.Directory == null) continue;

                    var binDirInfo = (from configName in configurationNames
                                      select Path.Combine(toDo.AppConfigFileInfo.Directory.FullName, "bin", configName)
                                      into binPath
                                      where Directory.Exists(binPath) && Directory.GetFiles(binPath, "*.dll").Length > 0
                                      select new DirectoryInfo(binPath)).FirstOrDefault();

                    if (binDirInfo == null)
                    {
                        continue;
                    }

                    toDo.BaseDirInfo = binDirInfo;
                }
                
                yield return toDo;
            }
        }
        else
        {
            yield return new ToDo(baseDirInfo, appConfigFileInfo);
        }
    }
}