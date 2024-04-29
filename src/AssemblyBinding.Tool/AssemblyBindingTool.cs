using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;
using Oleander.Assembly.Binding.Tool.Html;
using Oleander.Assembly.Binding.Tool.Xml;
using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger<AssemblyBindingTool> logger)
{
    internal int Execute(DirectoryInfo baseDirInfo, FileInfo? appConfigFileInfo, bool recursive, bool noReport)
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
                logger.CreateMSBuildWarning("ABT2", "No actions were taken! No configuration file was specified and the --no-report option was set to true.", "assembly-binding");
                return 2;
            }
        }

        var toDoList = this.CreateToDoList(baseDirInfo, appConfigFileInfo, recursive);
        var result = 0;

        foreach (var toDo in toDoList)
        {
            var innerResult = this.InnerExecute(toDo, toDoList.Count, noReport);
            if (innerResult > 0) result = innerResult;
        }

        if (toDoList.Count == 0) return 0;

        string? htmlIndexFileName;

        if (toDoList.Count == 1)
        {
            htmlIndexFileName = toDoList[0].HtmlIndexPage;
        }
        else
        {
            var links = new Dictionary<string, string>();

            foreach (var toDo in toDoList)
            {
                if (toDo.HtmlIndexPage == null || toDo.AppConfigFileInfo == null) continue;
                links[toDo.HtmlIndexPage] = toDo.AppConfigFileInfo.FullName;
            }

            htmlIndexFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out", "index.html");
            File.WriteAllText(htmlIndexFileName, HtmlIndex.Create(links, "Assembly binding reports", "Assembly binding index", "Select a link to get more information"));
        }

        if (string.IsNullOrEmpty(htmlIndexFileName)) return 0;

        var psi = new ProcessStartInfo
        {
            FileName = htmlIndexFileName,
            UseShellExecute = true // Wichtig für .NET Core oder .NET 5+
        };

        return Process.Start(psi) == null ? 4 : result;
    }

    private int InnerExecute(ToDo toDo, int maxIndex, bool noReport)
    {
        logger.CreateMSBuildMessage("ABT0", $"Load assemblies {toDo.Index + 1}/{maxIndex}", "assembly-binding");
        var cache = AssemblyBindingsBuilder.Create(toDo.BaseDirInfo);

        if (cache.Count == 0)
        {
            logger.CreateMSBuildWarning("ABT3", $"Directory '{toDo.BaseDirInfo}' does not contain any assemblies!", "assembly-binding");
            return 0;
        }

        if (!noReport)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", "Create reports", "assembly-binding");
                toDo.HtmlIndexPage = cache.CreateReports(toDo.AppConfigFileInfo, toDo.Index);
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT4", ex.Message, "assembly-binding");
                return 4;
            }
        }

        if (toDo.AppConfigFileInfo != null)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", $"Update app config file: {toDo.AppConfigFileInfo.Name}", "assembly-binding");
                toDo.AppConfigFileInfo.IsReadOnly = false;
                cache.CreateOrUpdateApplicationConfigFile(toDo.AppConfigFileInfo.FullName);
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT5", ex.Message, "assembly-binding");
                return 5;
            }
        }

        return 0;
    }

    private List<ToDo> CreateToDoList(DirectoryInfo baseDirInfo, FileInfo? appConfigFileInfo, bool recursive)
    {
        var toDoList = new List<ToDo>();

        if (recursive)
        {
            var index = 0;

            toDoList = baseDirInfo.GetFiles("*.config",
                new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    RecurseSubdirectories = true
                })
                .Where(x => x.Directory != null &&
                            !x.Directory.FullName.Contains($"{Path.DirectorySeparatorChar}obj", StringComparison.InvariantCultureIgnoreCase) &&
                            ApplicationConfiguration.IsAppConfigFile(x.FullName))
                .Select(x => new ToDo(x.Directory!, x, index++))
                .ToList();

            foreach (var appConfigToDo in toDoList
                .Where(x => x.AppConfigFileInfo != null && 
                            string.Equals(x.AppConfigFileInfo.Name, "app.config", StringComparison.InvariantCultureIgnoreCase)).ToList())
            {
                DirectoryInfo? baseDir = null;

                var projectFileInfo = appConfigToDo.AppConfigFileInfo!.Directory!.GetFiles("*.csproj").FirstOrDefault();
                if (projectFileInfo == null) continue;

                var exeFileName = string.Concat(projectFileInfo.Name.Substring(0, projectFileInfo.Name.Length - 7), "exe");

                foreach (var toDo in toDoList)
                {
                    if (toDo.BaseDirInfo.FullName.EndsWith(Path.Combine("bin", "release"), StringComparison.InvariantCultureIgnoreCase))
                    {
                        baseDir = toDo.BaseDirInfo;
                        break;
                    }

                    if (toDo.BaseDirInfo.FullName.EndsWith(Path.Combine("bin", "debug"), StringComparison.InvariantCultureIgnoreCase))
                    {
                        baseDir = toDo.BaseDirInfo;
                        break;
                    }
                }

                if (baseDir == null)
                {
                    toDoList.Remove(appConfigToDo);
                    logger.CreateMSBuildWarning("ABT2", $"Configuration file '{appConfigToDo.AppConfigFileInfo?.FullName}' skipped because no bin directory was found!", "assembly-binding");
                    continue;
                }

                if (!File.Exists(Path.Combine(baseDir.FullName, exeFileName)))
                {
                    toDoList.Remove(appConfigToDo);
                    logger.CreateMSBuildWarning("ABT2", $"Configuration file '{appConfigToDo.AppConfigFileInfo?.FullName}' skipped because no bin directory was found!", "assembly-binding")??;

                    continue;
                }

                appConfigToDo.BaseDirInfo = baseDir;

            }
            return toDoList;
        }

        toDoList.Add(new ToDo(baseDirInfo, appConfigFileInfo, 0));
        return toDoList;
    }
}