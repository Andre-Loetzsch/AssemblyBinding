using System.CommandLine;
using Oleander.Assembly.Binding.Tool.Options;

namespace Oleander.Assembly.Binding.Tool.Commands;

internal class AssemblyBindingCommand : Command
{
    public AssemblyBindingCommand(AssemblyBindingTool tool) : base("resolve", "resolve assembly bindings")
    {
        var baseDirOption = new BaseDirOption();

        this.AddOption(baseDirOption);
        this.SetHandler(baseDir => Task.FromResult(tool.Resolve(baseDir)), baseDirOption);
    }
}