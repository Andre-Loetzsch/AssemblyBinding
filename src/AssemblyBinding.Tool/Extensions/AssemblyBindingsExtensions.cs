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
        var outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "reports");

        if (index == -1 && Directory.Exists(outPath)) Directory.Delete(outPath, true);
        index++;

        if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);
        var htmlMainIndexFileName = Path.Combine(outPath, "index.html");
        var assemblyBindingsFileName = Path.Combine(outPath, $"{index + 1}.1 AssemblyBindings.xml");
        var topLevelAssembliesFileName = Path.Combine(outPath, $"{index + 1}.2 TopLevelAssemblies.html");
        var referencedByAssembliesFileName = Path.Combine(outPath, $"{index + 1}.3 ReferencedByAssemblies.html");
        var unresolvedAssembliesFileName = Path.Combine(outPath, $"{index + 1}.4 UnresolvedAssemblies.html");
        var assemblyDependenciesReportFileName = Path.Combine(outPath, $"{index + 1}.5 ReferencedAssemblies.html");
        var createAssemblyBuildOrderReportFileName = Path.Combine(outPath, $"{index + 1}.6 AssemblyBuildOrder.html");
        var htmlIndexFileName = Path.Combine(outPath, $"{index + 1} index.html");
        var links = new Dictionary<string, string>();

        var navigationLinks = new Dictionary<string, string>
        {
            [htmlMainIndexFileName] = "Assembly binding results",
            [htmlIndexFileName] = $"Assembly binding reports - {index + 1}"
        };

        var result = bindings.CreateAssemblyBindingsReport();
        var title = "Assembly bindings";

        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(assemblyBindingsFileName, result);
            links[assemblyBindingsFileName] = title;
        }

        #region CreateTopLevelAssemblyReport

        title = $"{index + 1}.2 Top level assemblies";
        result = bindings.CreateTopLevelAssemblyReport(title);
        
        File.WriteAllText(topLevelAssembliesFileName,
            HtmlCreator.CreateReportPage(result, title, navigationLinks));

        links[topLevelAssembliesFileName] = "Top level assemblies";

        #endregion

        #region CreateReferencedByAssembliesReport resolved = true

        title = $"{index + 1}.3 Referenced by Assemblies";
        result = bindings.CreateReferencedByAssembliesReport(title, true);

        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(referencedByAssembliesFileName,
                HtmlCreator.CreateReportPage(result, title, navigationLinks));
            links[referencedByAssembliesFileName] = "Referenced by Assemblies";
        }

        #endregion

        #region CreateReferencedByAssembliesReport resolved = false

        title = $"{index + 1}.4 Unresolved assemblies";
        result = bindings.CreateReferencedByAssembliesReport(title, false);
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(unresolvedAssembliesFileName,
                HtmlCreator.CreateReportPage(result, title, navigationLinks));
            links[unresolvedAssembliesFileName] = "Unresolved assemblies";
        }

        #endregion

        #region CreateAssemblyDependencyReport

        title = $"{index + 1}.5 Assembly dependencies";
        result = bindings.CreateAssemblyDependencyReport(title);

        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(assemblyDependenciesReportFileName,
                HtmlCreator.CreateReportPage(result, title, navigationLinks));
            links[assemblyDependenciesReportFileName] = "Assembly dependencies";
        }

        #endregion

        #region CreateAssemblyBuildOrderReport

        title = $"{index + 1}.6 Assembly build order";
        result = bindings.CreateAssemblyBuildOrderReport(title);
        if (!string.IsNullOrEmpty(result))
        {
            File.WriteAllText(createAssemblyBuildOrderReportFileName,
                HtmlCreator.CreateReportPage(result, title, navigationLinks));
            links[createAssemblyBuildOrderReportFileName] = "Assembly build order";
        }

        #endregion

        if (appConfigFileInfo != null)
        {
            links[appConfigFileInfo.FullName] = appConfigFileInfo.Name;
        }

        File.WriteAllText(htmlIndexFileName, 
            HtmlCreator.CreateReportSelectionPage(links, $"{index + 1} Assembly binding reports", 
                htmlMainIndexFileName, "Assembly binding results", index));

        return htmlIndexFileName;
    }

    internal static void CreateOrUpdateApplicationConfigFile(this IDictionary<string, AssemblyBindings> bindings, string appConfigurationFile)
    {
        ApplicationConfiguration.CreateOrUpdateAssemblyBinding(bindings.Values.ToList(), appConfigurationFile);
    }

    private static string CreateReferencedByAssembliesReport(this IDictionary<string, AssemblyBindings> bindings, string title, bool resolved)
    {
        return Markdown.Parse(ReferencedByAssembliesReport.Create(bindings, title, resolved));
    }
   

    private static string CreateTopLevelAssemblyReport(this IDictionary<string, AssemblyBindings> bindings, string title)
    {
        return Markdown.Parse(TopLevelAssemblyReport.Create(bindings, title));
    }

    private static string CreateAssemblyDependencyReport(this IDictionary<string, AssemblyBindings> bindings, string title)
    {
        return Markdown.Parse(AssemblyDependencyReport.Create(bindings, title));
    }

    private static string CreateAssemblyBindingsReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return AssemblyBindingsReport.Create(bindings.Values);
    }

    private static string CreateAssemblyBuildOrderReport(this IDictionary<string, AssemblyBindings> bindings, string title)
    {
        return Markdown.Parse(AssemblyBuildOrderReport.Create(bindings, title));
    }
}