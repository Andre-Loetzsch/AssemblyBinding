using System.Text;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool.Reports;

internal static class AssemblyBindingsReport
{
    internal static string Create(IEnumerable<AssemblyBindings> bindings)
    {
        var sb = new StringBuilder()
            .AppendLine("<assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">");

        foreach (var bindingInfo in bindings
                     .Where(x => x is { Resolved: true, ReferencedByAssembly.Count: > 0 })
                     .OrderBy(x => x.AssemblyName).ToList())
        {
            if (!bindingInfo.TryGetMinVersion(out var minAssemblyVersion)) continue;
            if (!bindingInfo.TryGetMaxVersion(out var maxAssemblyVersion)) continue;

            if (minAssemblyVersion == bindingInfo.AssemblyVersion &&
                maxAssemblyVersion == bindingInfo.AssemblyVersion) continue;

            var oldVersion = minAssemblyVersion.ToVersionString();

            if (minAssemblyVersion < maxAssemblyVersion)
            {
                oldVersion += $"-{maxAssemblyVersion.ToVersionString()}";
            }

            sb.AppendLine("  <dependentAssembly>");
            sb.AppendLine($"    <assemblyIdentity name=\"{bindingInfo.AssemblyName}\" publicKeyToken=\"{bindingInfo.PublicKey}\" culture=\"{bindingInfo.Culture}\" />");
            sb.AppendLine($"    <bindingRedirect oldVersion=\"{oldVersion}\" newVersion=\"{bindingInfo.AssemblyVersion.ToVersionString()}\" />");
            sb.AppendLine("  </dependentAssembly>");
        }

        sb.AppendLine("</assemblyBinding>");

        return sb.ToString();
    }
}