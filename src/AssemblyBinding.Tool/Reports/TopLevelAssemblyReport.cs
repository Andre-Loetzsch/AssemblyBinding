using System.Text;
using Oleander.Assembly.Binding.Tool.Data;

namespace Oleander.Assembly.Binding.Tool.Reports;

internal static class TopLevelAssemblyReport
{
    internal static string Create(IDictionary<string, AssemblyBindings> bindings)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# To Level Assemblies");

        foreach (var item in bindings
                     .Where(x => x.Value is { Resolved: true, ReferencedByAssembly.Count: 0 })
                     .OrderBy(x => x.Value.AssemblyName)
                     .ThenBy(x => x.Value.AssemblyVersion))
        {

            sb.Append("## ").AppendLine(item.Key);

            foreach (var groupItem in bindings.Values
                         .Where(x => x.Resolved && x.ReferencedByAssembly
                             .Any(x1 => x1.AssemblyName == item.Value.AssemblyName))
                         .GroupBy(x => x.AssemblyVersion)
                         .OrderBy(x => x.Key))
            {
                sb.AppendLine($"### {groupItem.Key}");

                foreach (var subItem in groupItem)
                {
                    sb.AppendLine($"- {subItem.BuildFullAssemblyName()}");
                }
            }
        }

        return sb.ToString();
    }
}