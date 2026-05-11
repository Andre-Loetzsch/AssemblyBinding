using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class NoReportOption : Option<bool>
{
    public NoReportOption() : base(name: "--no-report")
    {
        this.Description = "No report will be generated";
    }
}
