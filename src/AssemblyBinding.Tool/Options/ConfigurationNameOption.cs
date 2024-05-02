using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class ConfigurationNameOption() : Option<string>(name: "--configuration-name", description: "Name of the configuration (Release, Debug)");