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

        this.Options.Add(baseDirOption);
        this.Options.Add(appConfigFileOption);
        this.Options.Add(noReportOption);
        this.Options.Add(recursiveOption);
        this.Options.Add(branchOption);
        this.Options.Add(configurationNameOption);

        this.SetAction(parseResult =>
        {
            Task.FromResult(tool.Execute(
                parseResult.GetRequiredValue(baseDirOption),
                parseResult.GetValue(appConfigFileOption),
                parseResult.GetValue(recursiveOption),
                parseResult.GetValue(noReportOption),
                parseResult.GetRequiredValue(branchOption),
                parseResult.GetRequiredValue(configurationNameOption)));
        });
    }
}