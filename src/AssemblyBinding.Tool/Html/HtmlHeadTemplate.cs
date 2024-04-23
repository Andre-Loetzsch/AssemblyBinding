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
}