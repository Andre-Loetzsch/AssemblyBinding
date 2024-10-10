using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class BaseDirOption : Option<DirectoryInfo>
{
    public BaseDirOption() : base(name: "--base-dir", description: "The base application directory")
    {
        this.AddValidator(result =>
        {
            var dirInfo = result.GetValueOrDefault<DirectoryInfo>();

            if (dirInfo == null) return;

            if (!dirInfo.Exists)
            {
                result.ErrorMessage = $"Directory does not exists! '{dirInfo.FullName}'";
            }
        });

        this.IsRequired = true;
        this.AddCompletions(ctx => TabCompletions.FileCompletions(ctx.WordToComplete));
    }
}