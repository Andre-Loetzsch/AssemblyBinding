using System.Text;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool.Reports;

internal static class AssemblyReferencesTreeReport
{
    internal static string Create(IDictionary<string, AssemblyBindings> bindings, bool resolved)
    {
        var sb = new StringBuilder();
        sb.Append(resolved ? "# Resolved" : "Unresolved").AppendLine(" assembly references:");

        foreach (var item in bindings
                     .Where(x => x.Value is { Resolved: true })
                     .OrderBy(x => x.Value.AssemblyName)
                     .ThenBy(x => x.Value.AssemblyVersion))
        {

            sb.Append("## ").AppendLine(item.Key);

            GroupItem(sb, bindings, item.Value, 0);
        }

        return sb.ToString();
    }

    private static void GroupItem(StringBuilder sb, IDictionary<string, AssemblyBindings> bindings, AssemblyBindings item, int level)
    {
        foreach (var groupItem in bindings.Values
                     .Where(x => x.Resolved && x.ReferencedByAssembly
                         .Any(x1 => x1.AssemblyName == item.AssemblyName))
                     .GroupBy(x => x.AssemblyVersion)
                     .OrderBy(x => x.Key))
        {
            sb.AppendLine($"### {groupItem.Key}");

            foreach (var subItem in groupItem)
            {
                sb.AppendLine($"- {subItem.BuildFullAssemblyName()}");
                GroupItem(sb, bindings, subItem, level++);
            }

            
        }
    }
}