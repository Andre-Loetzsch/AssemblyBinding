using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class ConfigurationNameOption : Option<string>
{
    public ConfigurationNameOption() : base(name: "--configuration-name")
    {
        this.Description = "Name of the configuration (Release, Debug)";
    }
}