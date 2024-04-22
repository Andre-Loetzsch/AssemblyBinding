using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class AppFileOption : Option<FileInfo>
{
    public AppFileOption() : base(name: "--app-config", description: "The application configuration file")
    {
        this.AddValidator(result =>
        {
            try
            {
                var fileInfo = result.GetValueOrDefault<FileInfo>();

                if (fileInfo != null &&!string.Equals(Path.GetExtension(fileInfo.FullName).Trim('\"'), ".config"))
                {
                    result.ErrorMessage = $"The file must be an application configuration file! expected: '*.config' current: {fileInfo.FullName}";
                    return;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }
        });

        this.AddCompletions(ctx => TabCompletions.FileCompletions(ctx.WordToComplete, "*.config"));
    }
}