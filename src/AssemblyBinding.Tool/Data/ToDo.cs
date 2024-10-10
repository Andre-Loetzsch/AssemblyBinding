using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool.Data;

[DebuggerDisplay("{AppConfigFileInfo}")]
internal class ToDo(DirectoryInfo baseDirInfo, FileInfo? appConfigFile)
{
    internal DirectoryInfo BaseDirInfo { get; set; } = baseDirInfo;
    internal FileInfo? AppConfigFileInfo { get; set; } = appConfigFile;
    internal string? HtmlIndexPage { get; set; }
}