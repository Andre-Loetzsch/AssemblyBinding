namespace Oleander.Assembly.Binding.Tool.Extensions;

internal static class DirectoryFileInfoExtensions
{
    internal static string GetShortPathInfo(this DirectoryInfo? directoryInfo, int tailLen)
    {
        var logDir = directoryInfo?.FullName;

        if (logDir != null && logDir.Length > tailLen)
        {
            var tempList = new List<string>();
            var splitPath = logDir.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var pathLen = 0;

            for (var i = splitPath.Length - 1; i >= 0; i--)
            {
                var path = splitPath[i];
                pathLen += path.Length;

                tempList.Add(path);

                if (pathLen >= tailLen) break;
            }

            logDir = string.Concat("...", Path.DirectorySeparatorChar, string.Join(Path.DirectorySeparatorChar, tempList));
        }

        return logDir ?? string.Empty;
    }

    internal static string GetShortPathInfo(this FileInfo? fileInfo, int tailLen)
    {
        var logDir = fileInfo?.FullName;

        if (logDir != null && logDir.Length > tailLen)
        {
            var tempList = new List<string>();
            var splitPath = logDir.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            var pathLen = 0;

            for (var i = splitPath.Length - 1; i >= 0; i--)
            {
                var path = splitPath[i];
                pathLen += path.Length;

                tempList.Add(path);

                if (pathLen >= tailLen) break;
            }

            logDir = string.Concat("...", Path.DirectorySeparatorChar, string.Join(Path.DirectorySeparatorChar, tempList));
        }

        return logDir ?? string.Empty;
    }
}