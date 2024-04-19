using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool.Data;

[DebuggerDisplay("{AssemblyName}, AssemblyVersion={AssemblyVersion}")]
internal class ReferencingAssembly(string assemblyFullName, string assemblyName, Version assemblyVersion, Version referencingAssemblyVersion)
{
    public string AssemblyName { get; } = assemblyName;
    public string AssemblyFullName { get; } = assemblyFullName;

    public Version AssemblyVersion { get; } = assemblyVersion;

    public Version ReferencingAssemblyVersion { get; } = referencingAssemblyVersion;
}