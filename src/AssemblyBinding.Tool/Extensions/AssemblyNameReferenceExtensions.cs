using Oleander.Assembly.Comparers.Cecil;

namespace Oleander.Assembly.Binding.Tool.Extensions;

internal static class AssemblyNameReferenceExtensions
{
    internal static string BuildAssemblyKey(this AssemblyNameReference assemblyNameReference)
    {
        var culture = assemblyNameReference.Culture;
        var publicKey = assemblyNameReference.PublicKeyTokenAsString;

        culture = string.IsNullOrEmpty(culture) ? "neutral" : culture;
        publicKey = string.IsNullOrEmpty(publicKey) ? "null" : publicKey;
        return $"{assemblyNameReference.Name}, PublicKey={publicKey}, culture={culture}";
    }
}