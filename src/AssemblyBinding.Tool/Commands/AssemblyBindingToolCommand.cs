using System.CommandLine;
using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Options;

namespace Oleander.Assembly.Binding.Tool.Commands;


internal class AssemblyBindingCommand : Command
{

    public AssemblyBindingCommand(ILogger logger, AssemblyBindingTool tool) : base("resolve", "resolve assembly bindings")
    {

        var baseDirOption = new BaseDirOption();

        this.AddOption(baseDirOption);

        this.SetHandler(baseDir => Task.FromResult(tool.Resolve(baseDir)), baseDirOption);
    }

    
}