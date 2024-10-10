using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class RecursiveOption() : Option<bool>(name: "--recursive", description: "Recursively updates all configuration files");