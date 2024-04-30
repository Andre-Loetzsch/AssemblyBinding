using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;
using Oleander.Assembly.Binding.Tool.Html;
using Oleander.Assembly.Binding.Tool.Xml;
using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger<AssemblyBindingTool> logger)
{
    internal int Execute(DirectoryInfo baseDirInfo, FileInfo? appConfigFileInfo, bool recursive, bool noReport, string branch)
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

            if (!string.IsNullOrEmpty(branch))
            {
                logger.CreateMSBuildWarning("ABT2", "The --branch option is ignored because it is only valid with the --recursive true option", "assembly-binding");
            }
        }

        var toDoList = this.CreateToDoList(baseDirInfo, appConfigFileInfo, branch, recursive);
        var result = 0;

        for (var i = 0; i < toDoList.Count; i++)
        {
            var innerResult = this.InnerExecute(toDoList[i], i, toDoList.Count, noReport);
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

    private int InnerExecute(ToDo toDo, int index, int maxIndex, bool noReport)
    {
        logger.CreateMSBuildMessage("ABT0", $"Load assemblies: {toDo.AppConfigFileInfo?.Directory?.Name} {index + 1}/{maxIndex}", "assembly-binding");
        var cache = AssemblyBindingsBuilder.Create(toDo.BaseDirInfo);

        if (cache.Count == 0)
        {
            logger.CreateMSBuildWarning("ABT2", $"Directory '{toDo.BaseDirInfo}' does not contain any assemblies!", "assembly-binding");
            return 0;
        }

        if (!noReport)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", "Create reports", "assembly-binding");
                toDo.HtmlIndexPage = cache.CreateReports(toDo.AppConfigFileInfo);
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT1", ex.Message, "assembly-binding");
                return 4;
            }
        }

        if (toDo.AppConfigFileInfo != null)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", $"Update app config file: {toDo.AppConfigFileInfo?.Directory?.Name}{Path.DirectorySeparatorChar}{toDo.AppConfigFileInfo?.Name}", "assembly-binding");
                toDo.AppConfigFileInfo!.IsReadOnly = false;
                cache.CreateOrUpdateApplicationConfigFile(toDo.AppConfigFileInfo.FullName);
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT1", ex.Message, "assembly-binding");
                return 5;
            }
        }

        return 0;
    }

    private List<ToDo> CreateToDoList(DirectoryInfo baseDirInfo, FileInfo? appConfigFileInfo, string branch, bool recursive)
    {
        var toDoList = new List<ToDo>();
        var enumerationOptions = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = true
        };

        if (recursive)
        {
            logger.CreateMSBuildMessage("ABT0", "Processing data. This could take a while...", "assembly-binding");

            toDoList = baseDirInfo.GetFiles("*.config", enumerationOptions)
                .Where(x => x.Directory != null &&
                            !x.Directory.FullName.Contains($"{Path.DirectorySeparatorChar}obj", StringComparison.InvariantCultureIgnoreCase) &&
                            ApplicationConfiguration.IsAppConfigFile(x.FullName))
                .Select(x => new ToDo(x.Directory!, x))
                .ToList();

            if (!string.IsNullOrEmpty(branch))
            {
                branch = string.Concat(Path.DirectorySeparatorChar, branch, Path.DirectorySeparatorChar);
                toDoList = toDoList.Where(x => x.AppConfigFileInfo != null &&
                x.AppConfigFileInfo.FullName.Contains(branch, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }

            foreach (var appConfigToDo in toDoList
                .Where(x => x.AppConfigFileInfo != null &&
                            string.Equals(x.AppConfigFileInfo.Name, "app.config", StringComparison.InvariantCultureIgnoreCase)).ToList())
            {
                var projectFileInfo = appConfigToDo.AppConfigFileInfo!.Directory!.GetFiles("*.csproj").FirstOrDefault();
                if (projectFileInfo == null) continue;

                var exeFileName = string.Concat(projectFileInfo.Name.Substring(0, projectFileInfo.Name.Length - 6), "exe");
                var exeConfigFileName = string.Concat(exeFileName, ".config");
                var binPath = Path.Combine(appConfigToDo.AppConfigFileInfo.Directory.FullName, "bin", "release");
                var exeFilePath = Path.Combine(binPath, exeFileName);
                var exeConfigFilePath = Path.Combine(binPath, exeConfigFileName);

                if (File.Exists(exeFilePath) && File.Exists(exeConfigFilePath))
                {
                    appConfigToDo.BaseDirInfo = new DirectoryInfo(binPath);
                    continue;
                }

                if (projectFileInfo.Name.EndsWith(".test.csproj", StringComparison.InvariantCultureIgnoreCase))
                {
                    exeFilePath = exeFilePath.Replace(".Test.exe", ".Test.dll");
                    exeConfigFilePath = exeConfigFilePath.Replace(".Test.exe.config", ".Test.dll.config");

                    if (File.Exists(exeFilePath) && File.Exists(exeConfigFilePath))
                    {
                        appConfigToDo.BaseDirInfo = new DirectoryInfo(binPath);
                        continue;
                    }
                }

                binPath = Path.Combine(appConfigToDo.AppConfigFileInfo.Directory.FullName, "bin", "debug");
                exeFilePath = Path.Combine(binPath, exeFileName);

                exeConfigFilePath = Path.Combine(binPath, exeConfigFileName);

                if (File.Exists(exeFilePath) && File.Exists(exeConfigFilePath))
                {
                    appConfigToDo.BaseDirInfo = new DirectoryInfo(binPath);
                    continue;
                }

                if (projectFileInfo.Name.EndsWith(".test.csproj", StringComparison.InvariantCultureIgnoreCase))
                {
                    exeFilePath = exeFilePath.Replace(".Test.exe", ".Test.dll");
                    exeConfigFilePath = exeConfigFilePath.Replace(".Test.exe.config", ".Test.dll.config");

                    if (File.Exists(exeFilePath) && File.Exists(exeConfigFilePath))
                    {
                        appConfigToDo.BaseDirInfo = new DirectoryInfo(binPath);
                        continue;
                    }
                }

                toDoList.Remove(appConfigToDo);
            }

            return toDoList;
        }

        toDoList.Add(new ToDo(baseDirInfo, appConfigFileInfo));
        return toDoList;
    }
}