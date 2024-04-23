using System.Diagnostics;

namespace Oleander.Assembly.Binding.Tool.Data;

[DebuggerDisplay("{AssemblyName}, Resolved={Resolved}, Count={ReferencedByAssembly.Count}")]
internal class AssemblyBindings(string assemblyName)
{
    public bool Resolved { get; set; } = false;
    public List<ReferencingAssembly> ReferencedByAssembly { get; } = [];
    public List<ReferencingAssembly> ReferencedAssembly { get; } = [];

    public string AssemblyName { get; } = assemblyName;
    public Version? AssemblyVersion { get; set; }

    private string? _publicKey;
    public string? PublicKey
    {
        get => string.IsNullOrEmpty(this._publicKey) ? "null" : this._publicKey;
        set => this._publicKey = value;
    }

    private string? _culture;
    public string? Culture
    {
        get => string.IsNullOrEmpty(this._culture) ? "neutral" : this._culture;
        set => this._culture = value;
    }
}