﻿using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;
using Oleander.Assembly.Binding.Tool.Html;
using Oleander.Assembly.Binding.Tool.Xml;
using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger<AssemblyBindingTool> logger)
{
    internal int Execute(DirectoryInfo baseDirInfo, FileInfo? appConfigFileInfo, bool recursive, bool noReport, string branch, string configurationName)
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
        var index = 0;
        var links = new Dictionary<string, string>();

        foreach (var toDo in this.CreateToDoItems(baseDirInfo, appConfigFileInfo, recursive, branch, configurationName))
        {
            var innerResult = this.InnerExecute(toDo, noReport);

            index++;

            if (innerResult > 0) result = innerResult;

            if (toDo.HtmlIndexPage == null || toDo.AppConfigFileInfo == null) continue;
            links[toDo.HtmlIndexPage] = toDo.AppConfigFileInfo.FullName;

            logger.CreateMSBuildMessage("ABT1", $"Completed tasks: {index}", "assembly-binding");
        }

        if (links.Count == 0) return result;

        var htmlIndexFileName = links.Keys.First();

        if (links.Count > 1)
        {
            htmlIndexFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out", "index.html");
            File.WriteAllText(htmlIndexFileName, HtmlIndex.Create(links, "Assembly binding reports", "Assembly binding index", "Select a link to get more information"));
        }

        if (string.IsNullOrEmpty(htmlIndexFileName)) return result;

        var psi = new ProcessStartInfo
        {
            FileName = htmlIndexFileName,
            UseShellExecute = true // Wichtig für .NET Core oder .NET 5+
        };

        return Process.Start(psi) == null ? 4 : result;
    }

    private int InnerExecute(ToDo toDo, bool noReport)
    {
        var logDir = toDo.AppConfigFileInfo?.Directory?.FullName;

        if (logDir is { Length: > 20 })
        {
            var tempList = new List<string>();
            var splitPath = logDir.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var pathLen = 0;

            for (var i = splitPath.Length -1; i >= 0 ; i--)
            {
                var path = splitPath[i];
                pathLen += path.Length;

                tempList.Add(path);

                if (pathLen > 20) break;
            }

            logDir = string.Concat("...", Path.DirectorySeparatorChar, string.Join(Path.DirectorySeparatorChar, tempList));
        }


        logger.CreateMSBuildMessage("ABT2", $"Load assemblies: {logDir}", "assembly-binding");
        var cache = AssemblyBindingsBuilder.Create(toDo.BaseDirInfo);

        if (cache.Count == 0)
        {
            logger.CreateMSBuildWarning("ABT1", $"Directory '{toDo.BaseDirInfo}' does not contain any assemblies!", "assembly-binding");
            return 0;
        }

        if (!noReport)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT3", "Create reports", "assembly-binding");
                toDo.HtmlIndexPage = cache.CreateReports(toDo.AppConfigFileInfo);
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
                logger.CreateMSBuildMessage("ABT4", $"Update app config file: {logDir}{Path.DirectorySeparatorChar}{toDo.AppConfigFileInfo.Name}", "assembly-binding");
                if (toDo.AppConfigFileInfo.Exists) toDo.AppConfigFileInfo.IsReadOnly = false;
                cache.CreateOrUpdateApplicationConfigFile(toDo.AppConfigFileInfo.FullName);
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

            logger.CreateMSBuildMessage("ABT5", "Processing data...", "assembly-binding");
            branch = string.IsNullOrEmpty(branch) ? 
                Path.DirectorySeparatorChar.ToString() : 
                string.Concat(Path.DirectorySeparatorChar, branch, Path.DirectorySeparatorChar);

            var configurationNames = string.IsNullOrEmpty(configurationName) ?
                new[] { "debug", "release" } :
                new[] { configurationName };


            foreach (var toDo in baseDirInfo.EnumerateFiles("*.config", enumerationOptions)
                         .Where(x => x.Directory != null &&
                                     !x.Directory.FullName.Contains($"{Path.DirectorySeparatorChar}obj", StringComparison.InvariantCultureIgnoreCase) &&
                                     x.Directory.FullName.Contains(branch, StringComparison.CurrentCultureIgnoreCase) &&
                                     ApplicationConfiguration.IsAppConfigFile(x.FullName))
                         .Select(x => new ToDo(x.Directory!, x)))
            {

                if (string.Equals(toDo.AppConfigFileInfo?.Name, "app.config", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (toDo.AppConfigFileInfo?.Directory == null) continue;

                    var binDirInfo = (from configName in configurationNames
                                      select Path.Combine(toDo.AppConfigFileInfo.Directory.FullName, "bin", configName)
                                      into binPath
                                      where Directory.Exists(binPath) && Directory.GetFiles(binPath, "*.dll").Length > 0
                                      select new DirectoryInfo(binPath)).FirstOrDefault();

                    if (binDirInfo == null) continue;
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