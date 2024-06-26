﻿using Oleander.Assembly.Comparers.Cecil;

namespace Oleander.Assembly.Binding.Tool.Extensions;

internal static class AssemblyDefinitionExtensions
{
    internal static string BuildFullAssemblyName(this AssemblyDefinition assemblyDefinition)
    {
        var culture = assemblyDefinition.Name.Culture;
        var publicKey = assemblyDefinition.Name.PublicKeyTokenAsString;
        culture = string.IsNullOrEmpty(culture) ? "neutral" : culture;
        publicKey = string.IsNullOrEmpty(publicKey) ? "null" : publicKey;
        return $"{assemblyDefinition.Name.Name}, Version={assemblyDefinition.Name.Version}, Culture={culture}, PublicKeyToken={publicKey}";
    }

    internal static string BuildAssemblyKey(this AssemblyDefinition assemblyDefinition)
    {
        var culture = assemblyDefinition.Name.Culture;
        var publicKey = assemblyDefinition.Name.PublicKeyTokenAsString;

        culture = string.IsNullOrEmpty(culture) ? "neutral" : culture;
        publicKey = string.IsNullOrEmpty(publicKey) ? "null" : publicKey;
        return $"{assemblyDefinition.Name.Name}, PublicKey={publicKey}, culture={culture}";
    }
}