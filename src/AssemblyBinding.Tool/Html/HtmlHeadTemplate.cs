using System.Text;

namespace Oleander.Assembly.Binding.Tool.Html;

internal class HtmlHeadTemplate
{
    internal static string Create(string innerHtml, string title)
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
            .AppendLine(innerHtml)
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }

    internal static string CreateReportTemplate(string innerHtml, string title, IDictionary<string, string> links)
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
            sb.Append($"<li><a href=\"{key}\">{value}</a></li>");
        }
        
        sb.AppendLine(innerHtml)
        .AppendLine("</body>")
        .AppendLine("</html>");

        return sb.ToString();
    }
}