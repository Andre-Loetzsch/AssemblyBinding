using System.Text;

namespace Oleander.Assembly.Binding.Tool.Html
{
    internal class HtmlIndex
    {
        internal static string Create(IDictionary<string, string> links, int index)
        {
            var sb = new StringBuilder()
                .AppendLine("<!DOCTYPE html>")
                .AppendLine("<html lang=\"en\">")
                .AppendLine("<head>")
                .AppendLine("    <meta charset=\"UTF-8\">")
                .AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">")
                .AppendLine("    <title>Assembly Bindings Report Index</title>")
                .AppendLine("</head>")
                .AppendLine("<body>")
                .AppendLine("    <h1>Assembly Bindings Report</h1>")
                .AppendLine("    <h2>Select a report:</h2>")
                .AppendLine("    <ul>");

            var i = 1;

            foreach (var (key, value) in links)
            {
                sb.AppendLine($"          <li><a    href=\"{key}\" target=\"_blank\" rel=\"noopener noreferrer\">{index}.{i++}: {value}</a></li>");
            }

            sb.AppendLine("    </ul>")
                .AppendLine("</body>")
                .AppendLine("</html>");

            return sb.ToString();
        }
    }
}
