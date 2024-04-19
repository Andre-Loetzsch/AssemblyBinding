namespace Oleander.AssemblyBinding.Tool.Data;

internal class AssemblyBindings(string assemblyName)
{
    public List<ReferencingAssembly> ReferencedByAssembly { get; } = [];
    
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

    internal static bool IsExcludedAssemblies(string assemblyName)
    {
        return assemblyName.Equals("mscorlib", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.Equals("System", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.Equals("PresentationFramework", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.Equals("PresentationCore", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase);
    }
}