using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool.Data;

[DebuggerDisplay("{AppConfigFileInfo}, Index={Index}")]
internal class ToDo(DirectoryInfo baseDirInfo, FileInfo? appConfigFile, int index)
{
    internal DirectoryInfo BaseDirInfo { get; set; } = baseDirInfo;
    internal FileInfo? AppConfigFileInfo { get; set; } = appConfigFile;
    internal int Index { get; set; } = index;
    internal string? HtmlIndexPage { get; set; }
}