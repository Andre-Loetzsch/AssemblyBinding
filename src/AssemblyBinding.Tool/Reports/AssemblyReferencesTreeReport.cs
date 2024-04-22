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

            var text = GroupItem(bindings, item.Value, resolved, 0);
            if (string.IsNullOrEmpty(text)) continue;

            sb.AppendLine(text);
        }

        return sb.ToString();
    }

    private static string GroupItem(IDictionary<string, AssemblyBindings> bindings,
        AssemblyBindings item, bool resolved, int level)
    {
        var sb = new StringBuilder();
        foreach (var refAssembly in item.ReferencedAssembly)
        {
            var assembliy = bindings.Values
                .FirstOrDefault(x => x.AssemblyName == refAssembly.AssemblyName &&
                                     x.Resolved == resolved);
            if (assembliy == null) continue;

            sb.Append("".PadLeft(level, '-'));
            sb.AppendLine($"{assembliy.BuildFullAssemblyName()}");
            GroupItem(bindings, assembliy, resolved, level++);
        }

        return sb.ToString();

        //var refItems = bindings.Values
        //    .Where(x => x.Resolved == resolved && x.ReferencedByAssembly
        //        .Any(x1 => x1.AssemblyName == item.AssemblyName))
        //    .OrderBy(x => x.AssemblyName).ThenBy(x => x.AssemblyVersion)
        //    .Distinct().ToList();

        //foreach (var refItem in refItems)
        //{
        //    sb.Append("".PadLeft(level));
        //    sb.AppendLine($"- {refItem.BuildFullAssemblyName()}");
        //    GroupItem(sb, bindings, refItem, resolved, level++);
        //}
    }

    //private static void GroupItem(StringBuilder sb, IDictionary<string, AssemblyBindings> bindings,
    //    AssemblyBindings item, bool resolved, int level)
    //{
    //    foreach (var groupItem in bindings.Values
    //                 .Where(x => x.Resolved == resolved && 
    //                             x.ReferencedAssembly
    //                                .Any(x1 => x1.AssemblyName == item.AssemblyName && 
    //                                x1.ReferencingAssemblyVersion == item.AssemblyVersion))
    //                             .GroupBy(x => x.AssemblyVersion)
    //                             .OrderBy(x => x.Key))
    //    {
    //        sb.AppendLine($"### {groupItem.Key}");

    //        foreach (var subItem in groupItem)
    //        {
    //            sb.Append("".PadLeft(level));
    //            sb.AppendLine($"- {subItem.BuildFullAssemblyName()}");
    //            GroupItem(sb, bindings, subItem, resolved, level++);
    //        }
    //    }
    //}
}