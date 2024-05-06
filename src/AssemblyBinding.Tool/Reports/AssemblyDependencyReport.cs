using System.Text;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool.Reports;

internal static class AssemblyDependencyReport
{
    internal static string Create(IDictionary<string, AssemblyBindings> bindings, string title)
    {
        var sb = new StringBuilder();
        sb.Append("# ").AppendLine(title);

        foreach (var item in bindings
                     .Where(x => x.Value.Resolved)
                     .OrderBy(x => x.Value.AssemblyName)
                     .ThenBy(x => x.Value.AssemblyVersion))
        {

            sb.Append("## ").AppendLine(item.Value.BuildFullAssemblyName());

            var references = new List<string>();
            var transitiveReferences = new List<string>();

            AddReferences(references, transitiveReferences, bindings, item.Value, false);

            if (references.Count == 0) continue;

            sb.AppendLine("### Dependencies:");

            foreach (var reference in references.OrderBy(x => x))
            {
                sb.AppendLine($"- {reference}<br>");
            }

            if (transitiveReferences.Count == 0) continue;

            sb.AppendLine("### Indirect dependencies:");

            foreach (var reference in transitiveReferences.OrderBy(x => x))
            {
                sb.AppendLine($"- {reference}<br>");
            }
        }

        return sb.ToString();
    }

    private static void AddReferences(ICollection<string> references, ICollection<string> transitiveReferences,
        IDictionary<string, AssemblyBindings> bindings, AssemblyBindings item, bool transitive)
    {

        foreach (var refAssembly in item.ReferencedAssembly
                     .OrderBy(x => x.AssemblyName))
        {
            var assembly = bindings.Values
                .FirstOrDefault(x => x.AssemblyName == refAssembly.AssemblyName);

            if (references.Contains(refAssembly.AssemblyFullName)) continue;
            if (transitiveReferences.Contains(refAssembly.AssemblyFullName)) continue;

            if (transitive)
            {
                transitiveReferences.Add(refAssembly.AssemblyFullName);
            }
            else
            {
                references.Add(refAssembly.AssemblyFullName);
            }

            if (assembly == null) continue;

            AddReferences(references, transitiveReferences, bindings, assembly, true);
        }
    }
}