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

        this.AddOption(baseDirOption);
        this.AddOption(appConfigFileOption);
        this.AddOption(noReportOption);
        this.AddOption(recursiveOption);

        this.SetHandler((baseDir, appConfigFile, recursive,  noReport) => 
            Task.FromResult(tool.Execute(baseDir, appConfigFile, recursive, noReport)), 
            baseDirOption, appConfigFileOption, recursiveOption, noReportOption);
    }
}