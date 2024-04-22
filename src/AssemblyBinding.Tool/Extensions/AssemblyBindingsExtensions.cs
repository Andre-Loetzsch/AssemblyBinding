using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Reports;
using Oleander.Assembly.Binding.Tool.Xml;
using System.Diagnostics.CodeAnalysis;
using Westwind.AspNetCore.Markdown;

namespace Oleander.Assembly.Binding.Tool.Extensions;

internal static class AssemblyBindingsExtensions
{
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

    internal static string CreateAssemblyBindingsReport(this IDictionary<string, AssemblyBindings> bindings)
    {
        return Markdown.Parse(AssemblyBindingsReport.Create(bindings.Values));
    }

    internal static void CreateOrUpdateApplicationConfigFile(this IDictionary<string, AssemblyBindings> bindings, string appConfigurationFile)
    {
        ApplicationConfiguration.CreateOrUpdateAssemblyBinding(bindings.Values, appConfigurationFile);
    }
}