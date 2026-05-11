using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class AppFileOption : Option<FileInfo>
{
    public AppFileOption() : base(name: "--app-config")
    {
        this.Description = "The application configuration file";

        this.Validators.Add(result =>
        {
            try
            {
                var fileInfo = result.GetValueOrDefault<FileInfo>();
                if (fileInfo == null || string.Equals(Path.GetExtension(fileInfo.FullName).Trim('\"'), ".config", StringComparison.InvariantCultureIgnoreCase)) return;
                result.AddError($"The file must be an application configuration file! expected: '*.config' current: {fileInfo.FullName}");
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
            }
        });

        this.CompletionSources.Add(ctx => TabCompletions.FileCompletions(ctx.WordToComplete, "*.config"));
    }
}