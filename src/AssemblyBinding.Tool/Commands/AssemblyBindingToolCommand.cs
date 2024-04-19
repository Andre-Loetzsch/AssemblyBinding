using System.CommandLine;
using System.Text;
using Microsoft.Extensions.Logging;
using Oleander.AssemblyBinding.Tool.Data;
using Oleander.AssemblyBinding.Tool.Options;
using Oleander.AssemblyBinding.Tool.Reports;

namespace Oleander.AssemblyBinding.Tool.Commands;


internal class AssemblyBindingCommand : Command
{

    public AssemblyBindingCommand(ILogger logger, AssemblyBindingTool tool) : base("resolve", "resolve assembly bindings")
    {

        var baseDirOption = new BaseDirOption();

        this.AddOption(baseDirOption);

        this.SetHandler(baseDir => Task.FromResult(tool.Resolve(baseDir)), baseDirOption);
    }

    
}