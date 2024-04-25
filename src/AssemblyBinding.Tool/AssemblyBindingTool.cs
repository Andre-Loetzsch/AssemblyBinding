using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger<AssemblyBindingTool> logger)
{
    internal int Execute(DirectoryInfo directoryInfo, FileInfo? appConfigFile, bool noReport)
    {
        if (appConfigFile == null && noReport)
        {
            logger.CreateMSBuildWarning("ABT2", "No actions were taken! No configuration file was specified and the --no-report option was set to true.", "assembly-binding");
            return 1;
        }

        logger.CreateMSBuildMessage("ABT0", "Load assemblies", "assembly-binding");
        var cache = AssemblyBindingsBuilder.Create(directoryInfo);

        if (!noReport)
        {
            try
            {
                logger.CreateMSBuildMessage("ABT0", "Create reports", "assembly-binding");

                if (!cache.CreateReports())
                {
                    logger.CreateMSBuildWarning("ABT3", "Reporting process could not be started!", "assembly-binding");
                    return 2;
                }
            }
            catch (Exception ex)
            {
                logger.CreateMSBuildError("ABT4", ex.Message, "assembly-binding");
                return 3;
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
                logger.CreateMSBuildError("ABT5", ex.Message, "assembly-binding");
                return 4;
            }
        }

        return 0;
    }
}