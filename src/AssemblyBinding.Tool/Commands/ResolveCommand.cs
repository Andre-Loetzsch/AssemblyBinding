using System.CommandLine;
using Oleander.Assembly.Binding.Tool.Options;

namespace Oleander.Assembly.Binding.Tool.Commands;

internal class ResolveCommand : Command
{
    public ResolveCommand(AssemblyBindingTool tool) : base("resolve", "resolve assembly bindings")
    {
        var baseDirOption = new BaseDirOption();
        var appConfigFileOption = new AppFileOption();
        var recursiveOption = new RecursiveOption();
        var noReportOption = new NoReportOption();
        var branchOption = new BranchOption();
        var configurationNameOption = new ConfigurationNameOption();

        this.AddOption(baseDirOption);
        this.AddOption(appConfigFileOption);
        this.AddOption(noReportOption);
        this.AddOption(recursiveOption);
        this.AddOption(branchOption);
        this.AddOption(configurationNameOption);

        this.SetHandler((baseDir, appConfigFile, recursive, noReport, branch, configurationName) =>
                Task.FromResult(tool.Execute(baseDir, appConfigFile, recursive, noReport, branch, configurationName)),
            baseDirOption, appConfigFileOption, recursiveOption, noReportOption, branchOption, configurationNameOption);

        //this.SetHandler(tool.Execute, 
        //    baseDirOption, appConfigFileOption, recursiveOption, noReportOption, branchOption, configurationNameOption);
    }
}