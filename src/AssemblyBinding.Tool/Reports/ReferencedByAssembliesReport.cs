using System.Text;
using Oleander.Assembly.Binding.Tool.Data;

namespace Oleander.Assembly.Binding.Tool.Reports;

internal static class ReferencedByAssembliesReport
{
    internal static string Create(IDictionary<string, AssemblyBindings> bindings, bool resolved)
    {
        var sb = new StringBuilder();
        sb.Append(resolved ? "# Resolved" : "' Unresolved").AppendLine(" assembly referenced by:");

        foreach (var item in bindings
                     .Where(x => x.Value.Resolved == resolved)
                     .OrderBy(x => x.Value.AssemblyName)
                     .ThenBy(x => x.Value.AssemblyVersion))

        {
            var dependingList = item.Value.ReferencedByAssembly
                .OrderBy(x => x.AssemblyFullName)
                .ToList();

            if (dependingList.Count == 0) continue;
            var versionGroup = dependingList
                .GroupBy(x => x.ReferencingAssemblyVersion)
                .ToList();

            if (sb.Length > 0) sb.AppendLine();

            sb.Append("## ").AppendLine(item.Key);

            foreach (var groupItem in versionGroup.OrderBy(x => x.Key))
            {
                sb.AppendLine($"### {groupItem.Key}");

                foreach (var subItem in groupItem)
                {
                    sb.AppendLine($"- {subItem.AssemblyFullName}");
                }
            }
        }

        return sb.ToString();
    }
}