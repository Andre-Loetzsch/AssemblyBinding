using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Reports;

namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingTool(ILoggerFactory loggerFactory)
{
    internal int Resolve(DirectoryInfo directoryInfo)
    {
        var cache = AssemblyBindingsBuilder.Create(directoryInfo);
        var outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out");

        if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);

        File.WriteAllText(Path.Combine(outPath, "assemblyDependency.md"), string.Concat(
            AssemblyWithOutCorrelationReport.Create(cache),
            Environment.NewLine,
            AssemblyCorrelationReport.Create(cache, true)));

        File.WriteAllText(Path.Combine(outPath, "unresolvedAssemblyDependency.md"), 
            AssemblyCorrelationReport.Create(cache, false));


        var assemblyBindingsContents = CreateAssemblyBindings(cache.Values);
        var missingAssembliesContents = MissingAssembliesReport.Create(cache.Values);

        if (!string.IsNullOrEmpty(assemblyBindingsContents))
            File.WriteAllText(Path.Combine(outPath, "assemblyBindings.xml"), assemblyBindingsContents);

        if (!string.IsNullOrEmpty(missingAssembliesContents))
            File.WriteAllText(Path.Combine(outPath, "missingAssemblies.md"), missingAssembliesContents);
        return 0;
    }


    private static string CreateAssemblyBindings(IEnumerable<AssemblyBindings> bindings)
    {
        var sb = new StringBuilder()
            .AppendLine("<assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">");

        foreach (var bindingInfo in bindings
                     .Where(x => x is { Resolved: true, ReferencedByAssembly.Count: > 0 } )
                     .OrderBy(x => x.AssemblyName).ToList())
        {
            var minAssemblyVersion = bindingInfo.ReferencedByAssembly.Min(x => x.ReferencingAssemblyVersion);
            var maxAssemblyVersion = bindingInfo.ReferencedByAssembly.Max(x => x.ReferencingAssemblyVersion);

            if (minAssemblyVersion == bindingInfo.AssemblyVersion &&
                maxAssemblyVersion == bindingInfo.AssemblyVersion) continue;

            var oldVersion = minAssemblyVersion?.ToString();

            if (minAssemblyVersion < maxAssemblyVersion)
            {
                oldVersion += $"-{maxAssemblyVersion}";
            }

            sb.AppendLine("  <dependentAssembly>");
            sb.AppendLine($"    <assemblyIdentity name=\"{bindingInfo.AssemblyName}\" publicKeyToken=\"{bindingInfo.PublicKey}\" culture=\"{bindingInfo.Culture}\" />");
            sb.AppendLine($"    <bindingRedirect oldVersion=\"{oldVersion}\" newVersion=\"{bindingInfo.AssemblyVersion}\" />");
            sb.AppendLine("  </dependentAssembly>");
        }

        sb.AppendLine("</assemblyBinding>");

        return sb.ToString();
    }
}