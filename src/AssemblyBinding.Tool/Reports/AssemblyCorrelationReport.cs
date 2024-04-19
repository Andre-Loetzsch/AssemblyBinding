using System.Text;
using Oleander.AssemblyBinding.Tool.Data;

namespace Oleander.AssemblyBinding.Tool.Reports;

internal static class AssemblyCorrelationReport
{
    internal static string Create(IDictionary<string, AssemblyBindings> bindings)
    {
        var sb = new StringBuilder();

        foreach (var item in bindings
                     .Where(x =>
                         !AssemblyBindings.IsExcludedAssemblies(x.Value.AssemblyName) &&
                         x.Value.AssemblyVersion != null &&
                         x.Value.ReferencedByAssembly
                             .Any(x1 => !AssemblyBindings.IsExcludedAssemblies(x1.AssemblyName)))
                     .OrderBy(x => x.Key))
        {
            var dependingList = item.Value.ReferencedByAssembly
                .Where(x => !AssemblyBindings.IsExcludedAssemblies(x.AssemblyFullName))
                .OrderBy(x => x.AssemblyFullName)
                .ToList();

            if (dependingList.Count == 0) continue;
            var versionGroup = dependingList.GroupBy(x => x.ReferencingAssemblyVersion).ToList();

            if (sb.Length > 0) sb.AppendLine();

            sb.Append("# ").AppendLine(item.Key);

            foreach (var groupItem in versionGroup.OrderBy(x => x.Key))
            {
                sb.AppendLine($"## {groupItem.Key}");

                foreach (var subItem in groupItem)
                {
                    sb.AppendLine($"- {subItem.AssemblyFullName}");
                }
            }
        }

        return sb.ToString();
    }
}