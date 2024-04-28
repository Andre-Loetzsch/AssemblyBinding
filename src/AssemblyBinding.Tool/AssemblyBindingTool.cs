using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Extensions;
using Oleander.Assembly.Binding.Tool.Xml;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger<AssemblyBindingTool> logger)
{
    internal int Execute(DirectoryInfo directoryInfo, FileInfo? appConfigFile, bool recursive, bool noReport)
    {
        var toDoList = new List<Tuple<DirectoryInfo, FileInfo?, int>>();

        if (recursive)
        {
            if (appConfigFile != null)
            {
                logger.CreateMSBuildError("ABT2", "Option --app-config is not supported when --recursive is true!", "assembly-binding");
                return 1;
            }

            var index = 0;
            foreach(var configFile in directoryInfo.GetFiles("*.config", new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = true
            }).Where(x => x.Directory != null &&      
                          ApplicationConfiguration.IsAppConfiFile(x.FullName)))
            {
               


                toDoList.Add(new(configFile.Directory!, configFile, index++));
            }
        }
        else if (appConfigFile == null && noReport)
        {
            logger.CreateMSBuildWarning("ABT3", "No actions were taken! No configuration file was specified and the --no-report option was set to true.", "assembly-binding");
            return 2;
        }

        toDoList.Add(new(directoryInfo, appConfigFile, 0));

        var result = 0;

        foreach (var (dirInfo, fileInf, index) in toDoList)
        {
            var innerResult = this.InnerExecute(dirInfo, fileInf, index, recursive, noReport);
            if (innerResult > 0) result = innerResult;
        }

        return result;
    }

    private int InnerExecute(DirectoryInfo directoryInfo, FileInfo? appConfigFile, int index, bool recursive, bool noReport)
    {
        logger.CreateMSBuildMessage("ABT0", "Load assemblies", "assembly-binding");
        var cache = AssemblyBindingsBuilder.Create(directoryInfo);

        if (!noReport)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", "Create reports", "assembly-binding");

                if (!cache.CreateReports(index))
                {
                    logger.CreateMSBuildWarning("ABT4", "Reporting process could not be started!", "assembly-binding");
                    return 3;
                }
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT5", ex.Message, "assembly-binding");
                return 4;
            }
        }

        if (appConfigFile != null)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", "Update app config file", "assembly-binding");
                cache.CreateOrUpdateApplicationConfigFile(appConfigFile.FullName);
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT6", ex.Message, "assembly-binding");
                return 5;
            }
        }

        return 0;
    }
}