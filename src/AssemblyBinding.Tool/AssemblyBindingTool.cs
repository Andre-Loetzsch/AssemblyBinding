using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILogger logger)
{
    internal int Resolve(DirectoryInfo directoryInfo)
    {
        var cache = AssemblyBindingsBuilder.Create(directoryInfo);
        var outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out");

        if (!Directory.Exists(outPath))
        {
            logger.LogInformation("Create output directory: {outDir}", outPath);
            Directory.CreateDirectory(outPath);
        }

        var topLevelAssembliesFileName = Path.Combine(outPath, "topLevelAssemblies.html");
        var referencedByAssembliesFileName = Path.Combine(outPath, "referencedByAssemblies.html");
        var unresolvedAssembliesFileName = Path.Combine(outPath, "unresolvedAssemblies.html");
        var assemblyBindingsFileName = Path.Combine(outPath, "assemblyBindings.html");
        var htmlIndexFileName = Path.Combine(outPath, "index.html");


        File.WriteAllText(topLevelAssembliesFileName, cache.CreateTopLevelAssemblyReport());
        File.WriteAllText(referencedByAssembliesFileName, cache.CreateReferencedByAssembliesReport());
        File.WriteAllText(unresolvedAssembliesFileName, cache.CreateUnresolvedAssembliesReport());
        File.WriteAllText(assemblyBindingsFileName, cache.CreateAssemblyBindingsReport());

        // TODO cache.CreateOrUpdateApplicationConfigFile(appConfigFileName);
        //cache.CreateOrUpdateApplicationConfigFile(appConfigFileName);


        var links = new Dictionary<string, string>
        {
            [topLevelAssembliesFileName] = "Top level assemblies",
            [referencedByAssembliesFileName] = "Referenced by Assemblies",
            [unresolvedAssembliesFileName] = "Unresolved assemblies",
            [assemblyBindingsFileName] = "Assembly bindings"
        };

        File.WriteAllText(htmlIndexFileName, this.CreateHtmlIndex(links));

        var psi = new ProcessStartInfo
        {
            FileName = htmlIndexFileName,
            UseShellExecute = true // Wichtig für .NET Core oder .NET 5+
        };

        return Process.Start(psi) == null ? -1 : 0;
       
    }

    private string CreateHtmlIndex(IDictionary<string, string> links)
    {
        var sb = new StringBuilder()
            .AppendLine("<!DOCTYPE html>")
            .AppendLine("<html lang=\"en\">")
            .AppendLine("<head>")
            .AppendLine("    <meta charset=\"UTF-8\">")
            .AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">")
            .AppendLine("    <title>Meine Webseite</title>")
            .AppendLine("</head>")
            .AppendLine("<body>")
            .AppendLine("    <h1>Willkommen auf meiner Webseite!</h1>")
            .AppendLine("    <ul>");

        var index = 0;

        foreach (var (key, value) in links)
        {
            sb.AppendLine($"        <li><a href=\"{key}\">Link {index++}: {value}</a></li>");
        }

        sb.AppendLine("    </ul>")
            .AppendLine("</body>")
            .AppendLine("</html>");

        return sb.ToString();
    }

}