using System.CommandLine;

namespace Oleander.Assembly.Binding.Tool.Options;

internal class NoReportOption() : Option<bool>(name: "--no-report", description: "No report will be generated");