using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger<AssemblyBindingTool> logger)
{
    internal int Execute(DirectoryInfo directoryInfo1, FileInfo? appConfigFile1, bool recursive, bool noReport)
    {


        if (recursive)
        {
            if (appConfigFile != null)
            {
                logger.CreateMSBuildError("ABT2", "Option --app-config is not supported when --recursive is true!", "assembly-binding");
                return 1;
            }

          


        }
        else if (appConfigFile == null && noReport)
        {
            logger.CreateMSBuildWarning("ABT3", "No actions were taken! No configuration file was specified and the --no-report option was set to true.", "assembly-binding");
            return 2;
        }



        logger.CreateMSBuildMessage("ABT0", "Load assemblies", "assembly-binding");
        var cache = AssemblyBindingsBuilder.Create(directoryInfo);



        if (!noReport)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", "Create reports", "assembly-binding");

                if (!cache.CreateReports(recursive))
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