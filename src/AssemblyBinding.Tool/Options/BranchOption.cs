using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class BranchOption : Option<string>
{
    public BranchOption() : base(name: "--branch")
    {
        Description = "Name of the branch used as a search filter";
    }
};