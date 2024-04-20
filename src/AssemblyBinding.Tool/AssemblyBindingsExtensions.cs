using Oleander.Assembly.Binding.Tool.Data;

namespace Oleander.Assembly.Binding.Tool;

internal static class AssemblyBindingsExtensions
{
    internal static string BuildFullAssemblyName(this AssemblyBindings assemblyBindings)
    {
        return $"{assemblyBindings.AssemblyName}, Version={assemblyBindings.AssemblyVersion}, Culture={assemblyBindings.Culture}, PublicKeyToken={assemblyBindings.PublicKey}";
    }
    internal static string BuildAssemblyKey(this AssemblyBindings assemblyBindings)
    {
        return $"{assemblyBindings.AssemblyName}, PublicKey={assemblyBindings.PublicKey}, culture={assemblyBindings.Culture}";
    }
}