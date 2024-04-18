namespace Oleander.AssemblyBinding.Tool.Data;

internal class BindingInfo(string assemblyName)
{
    public List<BindingInfo> ReferencedByAssembly { get; } = [];
    public Version? ReferencedByAssemblyVersion { get; set; } 

    public string AssemblyName { get; } = assemblyName;
    public Version? AssemblyVersion { get; set; }

    public Version? HighestAssemblyVersion { get; set; }

    public Version? LowestAssemblyVersion { get; set; }

    public string? PublicKey { get; set; }
    public string? Culture { get; set; }

    public int Count { get; set; }
}