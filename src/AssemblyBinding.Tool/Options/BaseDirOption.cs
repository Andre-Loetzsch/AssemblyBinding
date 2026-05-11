using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class BaseDirOption : Option<DirectoryInfo>
{
    public BaseDirOption() : base(name: "--base-dir")
    {
        this.Description = "The base application directory";

        this.Validators.Add(result =>
        {
            var dirInfo = result.GetValueOrDefault<DirectoryInfo>();

            if (dirInfo == null) return;

            if (!dirInfo.Exists)
            {
                result.AddError($"Directory does not exists! '{dirInfo.FullName}'");
            }
        });

        this.Required = true;
        this.CompletionSources.Add(ctx => TabCompletions.FileCompletions(ctx.WordToComplete));
    }
}