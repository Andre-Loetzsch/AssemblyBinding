using System.Text;

namespace Oleander.Assembly.Binding.Tool.Html;

internal static class HtmlCreator
{
    internal static string CreateMainIndexPage(IDictionary<string, string> links, string title, string headline1, string headline2)
    {
        var sb = new StringBuilder()
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
        
        var i = 1;

        foreach (var (key, value) in links)
        {
            sb.AppendLine($"          <li><a href=\"{key}\">{i++}: {value}</a></li>");
        }

        sb.AppendLine("    </ul>")
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }

    internal static string CreateReportSelectionPage(IDictionary<string, string> links, string title, string htmlMainIndexFileName, string htmlMainIndexTitle, int index)
    {
        var sb = new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html lang=\"en\">")
            .AppendLine("<head>")
            .AppendLine("    <meta charset=\"UTF-8\">")
            .AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">")
            .AppendLine($"    <title>{title}</title>")
            .AppendLine("</head>")
            .AppendLine("<body>")
            .AppendLine("<h3>Navigation</h3>")
            .AppendLine($"<li><a href=\"{htmlMainIndexFileName}\">{htmlMainIndexTitle}</a></li>")
            .Append("    <h1>").Append(title).AppendLine("</h1>");

        var i = 1;

        foreach (var (key, value) in links)
        {
            if (key.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase))
            {
                sb.AppendLine($"          <li><a href=\"{key}\">{index + 1}.{i++}: {value}</a></li>");
                continue;
            }

            sb.AppendLine($"          <li><a href=\"{key}\" target=\"_blank\">{index + 1}.{i++}: {value}</a></li>");
        }

        sb.AppendLine("    </ul>")
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }

    internal static string CreateReportPage(string reportHtml, string title, IDictionary<string, string> links)
    {
        var sb = new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html lang=\"en\">")
            .AppendLine("<head>")
            .AppendLine("    <meta charset=\"UTF-8\">")
            .AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">")
            .AppendLine($"    <title>{title}</title>")
            .AppendLine("</head>")
            .AppendLine("<body>")
            .AppendLine("<h3>Navigation</h3>");

        foreach (var (key, value) in links)
        {
            sb.AppendLine($"<li><a href=\"{key}\">{value}</a></li>");
        }

        sb.AppendLine(reportHtml)
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }
}