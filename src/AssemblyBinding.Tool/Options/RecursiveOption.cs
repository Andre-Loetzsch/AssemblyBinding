using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class RecursiveOption : Option<bool>
{
    public RecursiveOption() : base(name: "--recursive")
    {
        this.Description = "Recursively updates all configuration files";
    }
}
   