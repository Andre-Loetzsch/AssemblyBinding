using System.CommandLine;
using Oleander.Assembly.Binding.Tool.Options;

namespace Oleander.Assembly.Binding.Tool.Commands;

internal class AssemblyBindingCommand : Command
{
    public AssemblyBindingCommand(AssemblyBindingTool tool) : base("resolve", "resolve assembly bindings")
    {
        var baseDirOption = new BaseDirOption();
        var appConfigFileOption = new AppFileOption();
        var noReportOption = new NoReportOption();

        this.AddOption(baseDirOption);
        this.AddOption(appConfigFileOption);
        this.AddOption(noReportOption);

        this.SetHandler((baseDir, appConfigFile, noReport) => 
            Task.FromResult(tool.Execute(baseDir, appConfigFile, noReport)), 
            baseDirOption, appConfigFileOption, noReportOption);
    }
}