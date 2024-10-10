using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class BranchOption() : Option<string>(name: "--branch", description: "Name of the branch used as a search filter");