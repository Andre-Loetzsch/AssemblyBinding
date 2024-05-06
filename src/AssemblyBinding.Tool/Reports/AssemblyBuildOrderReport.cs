using System.Text;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool.Reports;

internal static class AssemblyBuildOrderReport
{
    internal static string Create(IDictionary<string, AssemblyBindings> bindings, string title)
    {
        var sb = new StringBuilder();
        sb.Append("# ").AppendLine(title);

        var bindingList = bindings.Values
            .Where(x => x.Resolved)
            .OrderBy(x => x.AssemblyName).ToList();

        foreach (var binding in BuildOrder(bindingList))
        {
            sb.Append("## ").AppendLine(binding.BuildFullAssemblyName());

            foreach (var refAssembly in binding.ReferencedAssembly
                         .OrderBy(x => x.AssemblyFullName))
            {
                sb.Append("- ").AppendLine(refAssembly.AssemblyFullName);
            }
        }

        return sb.ToString();
    }

    private static List<AssemblyBindings> BuildOrder(List<AssemblyBindings> bindings)
    {
        for (var i = 0; i < bindings.Count; i++)
        {
            var binding = bindings[i];

            foreach (var refAssembly in binding.ReferencedAssembly)
            {
                var referencedAssembly = bindings.FirstOrDefault(x => x.AssemblyName == refAssembly.AssemblyName);
                if (referencedAssembly == null) continue;

                var itemIndex = bindings.IndexOf(binding);
                var referencedIndex = bindings.IndexOf(referencedAssembly);

                if (itemIndex > referencedIndex) continue;

                bindings.RemoveAt(itemIndex);
                bindings.Insert(referencedIndex, binding);
                i = 0;
            }
        }

        return bindings;
    }
}