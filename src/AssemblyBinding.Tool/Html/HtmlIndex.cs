using System.Text;

namespace Oleander.Assembly.Binding.Tool.Html;

internal static class HtmlIndex
{
    internal static string Create(IDictionary<string, string> links, string title, string headline1, string headline2, int index)
    {
        var sb = CreateHeader(title, headline1, headline2);
        var i = 1;

        foreach (var (key, value) in links)
        {
            //sb.AppendLine($"          <li><a    href=\"{key}\" target=\"_blank\" rel=\"noopener noreferrer\">{index}.{i++}: {value}</a></li>");
            //sb.AppendLine($"          <li><a    href=\"{key}\" rel=\"noopener noreferrer\">{index +1}.{i++}: {value}</a></li>");
            sb.AppendLine($"          <li><a href=\"{key}\">{index + 1}.{i++}: {value}</a></li>");

        }

        sb.AppendLine("    </ul>")
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }

    internal static string Create(IDictionary<string, string> links, string title, string headline1, string headline2)
    {
        var sb = CreateHeader(title, headline1, headline2);
        var i = 1;

        foreach (var (key, value) in links)
        {
            sb.AppendLine($"          <li><a href=\"{key}\" target=\"_blank\" rel=\"noopener noreferrer\">{i++}: {value}</a></li>");
        }

        sb.AppendLine("    </ul>")
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }

    private static StringBuilder CreateHeader(string title, string headline1, string headline2)
    {
        return new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html lang=\"en\">")
            .AppendLine("<head>")
            .AppendLine("    <meta charset=\"UTF-8\">")
            .AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">")
            .Append("    <title>").Append(title).AppendLine("</title>")
            .AppendLine("</head>")
            .AppendLine("<body>")
            .Append("    <h1>").AppendLine(headline1).AppendLine("</h1>")
            .Append("    <h2>").Append(headline2).AppendLine("</h2>")
            .AppendLine("    <ul>");
    }

    internal static string CreateHtmlLink(string link, string name)
    {
        return $"<a href=\"{link}\" target=\"_blank\">{name}</a>";

    }
}