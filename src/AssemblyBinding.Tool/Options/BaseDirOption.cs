using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class BaseDirOption : Option<DirectoryInfo>
{
    public BaseDirOption() : base(name: "--base-dir", description: "The base application directory")
    {
        this.AddCompletions(ctx => TabCompletions.FileCompletions(ctx.WordToComplete));
    }
}