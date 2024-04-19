using Oleander.AssemblyBinding.Tool.Data;
using System.Text;

namespace Oleander.AssemblyBinding.Tool.Reports;

internal static class AssemblyWithOutCorrelationReport
{
    internal static string Create(IDictionary<string, AssemblyBindings> bindings)
    {
        var usedSb = new StringBuilder();
        var unusedSb = new StringBuilder();

        foreach (var item in bindings
                     .Where(x =>
                         !AssemblyBindings.IsExcludedAssemblies(x.Value.AssemblyName) &&
                         x.Value.AssemblyVersion != null &&
                         x.Value.ReferencedByAssembly.All(x1 => AssemblyBindings.IsExcludedAssemblies(x1.AssemblyName)))
                     .OrderBy(x => x.Key))
        {
            if (bindings.Values
                .Any(x => x.ReferencedByAssembly
                    .Any(x1 => x1.AssemblyName == item.Value.AssemblyName)))
            {
                usedSb.Append("- ").AppendLine(item.Key);
                continue;
            }

            unusedSb.Append("- ").AppendLine(item.Key);
        }

        if (unusedSb.Length > 0)
        {
            unusedSb.Insert(0, $"# Assemblies not used in any assemblies: {Environment.NewLine}");
        }

        if (usedSb.Length > 0)
        {
            usedSb.Insert(0, $"# Assemblies used in other assemblies: {Environment.NewLine}");
        }

        return string.Concat(unusedSb, usedSb);
    }
}