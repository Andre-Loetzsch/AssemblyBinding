namespace Oleander.AssemblyBinding.Tool.Data;

internal class ReferencingAssembly(string assemblyFullName, string assemblyName, Version assemblyVersion, Version referencingAssemblyVersion)
{
    public string AssemblyName { get; } = assemblyName;
    public string AssemblyFullName { get; } = assemblyFullName;

    public Version AssemblyVersion { get; } = assemblyVersion;

    public Version ReferencingAssemblyVersion { get; } = referencingAssemblyVersion;
}