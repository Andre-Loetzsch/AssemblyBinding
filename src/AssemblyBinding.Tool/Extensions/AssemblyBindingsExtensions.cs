using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Html;
using Oleander.Assembly.Binding.Tool.Reports;
using Oleander.Assembly.Binding.Tool.Xml;
using System.Diagnostics.CodeAnalysis;
using Westwind.AspNetCore.Markdown;

namespace Oleander.Assembly.Binding.Tool.Extensions;

internal static class AssemblyBindingsExtensions
{
    private static int index = -1;

    internal static string BuildFullAssemblyName(this AssemblyBindings assemblyBindings)
    {
        return $"{assemblyBindings.AssemblyName}, Version={assemblyBindings.AssemblyVersion}, Culture={assemblyBindings.Culture}, PublicKeyToken={assemblyBindings.PublicKey}";
    }
    internal static string BuildAssemblyKey(this AssemblyBindings assemblyBindings)
    {
        return $"{assemblyBindings.AssemblyName}, PublicKey={assemblyBindings.PublicKey}, culture={assemblyBindings.Culture}";
    }

    internal static bool TryGetMinVersion(this AssemblyBindings assemblyBindings, [MaybeNullWhen(false)] out Version minVersion)
    {
        minVersion = assemblyBindings.ReferencedByAssembly.Min(x => x.ReferencingAssemblyVersion);
        return minVersion != null;
    }

    internal static bool TryGetMaxVersion(this AssemblyBindings assemblyBindings, [MaybeNullWhen(false)] out Version maxVersion)
    {
        maxVersion = assemblyBindings.ReferencedByAssembly.Max(x => x.ReferencingAssemblyVersion);
        return maxVersion != null;
    }



    internal static string CreateReports(this IDictionary<string, AssemblyBindings> bindings, FileInfo? appConfigFileInfo)
    {
        var outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out");

        if (index == -1 && Directory.Exists(outPath)) Directory.Delete(outPath, true);
        index++;

        if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);

        var assemblyBindingsFileName = Path.Combine(outPath, $"{index +1}.1 AssemblyBindings.xml");
        var topLevelAssembliesFileName = Path.Combine(outPath, $"{index + 1}.2 TopLevelAssemblies.html");
        var referencedByAssembliesFileName = Path.Combine(outPath, $"{index + 1}.3 ReferencedByAssemblies.html");
        var unresolvedAssembliesFileName = Path.Combine(outPath, $"{index + 1}.4 UnresolvedAssemblies.html");
        var assemblyDependenciesReportFileName = Path.Combine(outPath, $"{index + 1}.5 ReferencedAssemblies.html");
        var createAssemblyBuildOrderReportFileName = Path.Combine(outPath, $"{index + 1}.6 AssemblyBuildOrder.html");
        var htmlIndexFileName = Path.Combine(outPath, $"{index + 1} index.html");
        var links = new Dictionary<string, string>();

        var result = bindings.CreateAssemblyBindingsReport();
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(assemblyBindingsFileName, result);
            links[assemblyBindingsFileName] = "Assembly bindings";
        }

        result = bindings.CreateTopLevelAssemblyReport();
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(topLevelAssembliesFileName, 
                HtmlHeadTemplate.Create(result, $"{index + 1}.2 Top level assemblies"));
            links[topLevelAssembliesFileName] = "Top level assemblies";
        }

        result = bindings.CreateReferencedByAssembliesReport();
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(referencedByAssembliesFileName, 
                HtmlHeadTemplate.Create(result, $"{index + 1}.3 Referenced by Assemblies"));
            links[referencedByAssembliesFileName] = "Referenced by Assemblies";
        }

        result = bindings.CreateAssemblyDependencyReport();
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(assemblyDependenciesReportFileName,
                HtmlHeadTemplate.Create(result, $"{index + 1}.4 Assembly dependencies"));
            links[assemblyDependenciesReportFileName] = "Assembly dependencies";
        }

        result = bindings.CreateUnresolvedAssembliesReport();
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(unresolvedAssembliesFileName, 
                HtmlHeadTemplate.Create(result, $"{index + 1}.5 Unresolved assemblies"));
            links[unresolvedAssembliesFileName] = "Unresolved assemblies";
        }

        result = bindings.CreateAssemblyBuildOrderReport();
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(createAssemblyBuildOrderReportFileName, 
                HtmlHeadTemplate.Create(result, $"{index + 1}.6 Assembly build order"));
            links[createAssemblyBuildOrderReportFileName] = "Assembly build order";
        }

        var headLine1 = $"Assembly Binding Report {index +1}";
        var headLine2 = appConfigFileInfo == null ? "Reports:" :  string.Concat("Reports: ", HtmlIndex.CreateHtmlLink(appConfigFileInfo.FullName, $"Reports: {appConfigFileInfo.FullName}"));

        File.WriteAllText(htmlIndexFileName, HtmlIndex.Create(links, $"{index + 1} Assembly Binding Report", headLine1, headLine2, index)); 

        return htmlIndexFileName;
    }

    internal static string CreateReferencedByAssembliesReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return Markdown.Parse(ReferencedByAssembliesReport.Create(bindings, true));
    }

    internal static string CreateUnresolvedAssembliesReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return Markdown.Parse(ReferencedByAssembliesReport.Create(bindings, false));
    }

    internal static string CreateTopLevelAssemblyReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return Markdown.Parse(TopLevelAssemblyReport.Create(bindings));
    }
    internal static string CreateAssemblyDependencyReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return Markdown.Parse(AssemblyDependencyReport.Create(bindings));
    }

    internal static string CreateAssemblyBindingsReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return AssemblyBindingsReport.Create(bindings.Values);
    }

    internal static string CreateAssemblyBuildOrderReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return Markdown.Parse(AssemblyBuildOrderReport.Create(bindings));
    }

    internal static void CreateOrUpdateApplicationConfigFile(this IDictionary<string, AssemblyBindings> bindings, string appConfigurationFile)
    {
        ApplicationConfiguration.CreateOrUpdateAssemblyBinding(bindings.Values.ToList(), appConfigurationFile);
    }
}