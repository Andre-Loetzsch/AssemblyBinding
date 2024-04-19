using System.Text;
using Oleander.AssemblyBinding.Tool.Data;

namespace Oleander.AssemblyBinding.Tool.Reports;

internal static class MissingAssembliesReport
{
    internal static string Create(IEnumerable<AssemblyBindings> bindings)
    {
        var sb = new StringBuilder();

        foreach (var bindingInfo in bindings.Where(x => x.AssemblyVersion == null))
        {
            var minAssemblyVersion = bindingInfo.ReferencedByAssembly.Min(x => x.ReferencingAssemblyVersion);
            var maxAssemblyVersion = bindingInfo.ReferencedByAssembly.Max(x => x.ReferencingAssemblyVersion);

            sb.Append("# ").AppendLine(bindingInfo.AssemblyName);
            sb.Append("- ").AppendLine(minAssemblyVersion == maxAssemblyVersion ?
                $"version: {minAssemblyVersion}" :
                $"oldVersion: {minAssemblyVersion} newVersion: {maxAssemblyVersion}");
        }

        return sb.ToString();
    }
}