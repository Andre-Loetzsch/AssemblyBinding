using System.CommandLine;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Extensions.Logging;
using Oleander.AssemblyBinding.Tool.Options;

namespace Oleander.AssemblyBinding.Tool.Commands;


internal class AssemblyBindingCommand : Command
{
    private Dictionary<string, BindingInfo> _bindingInfoCache = new();
    public AssemblyBindingCommand(ILogger logger, AssemblyBindingTool tool) : base("resolve", "resolve assembly bindings")
    {

        var baseDirOption = new BaseDirOption();

        this.AddOption(baseDirOption);

        this.SetHandler(baseDir => Task.FromResult(this.Resolve(baseDir)), baseDirOption);
    }

    private int Resolve(DirectoryInfo directoryInfo)
    {
       

        foreach (var assemblyFile in directoryInfo.GetFiles("*.exe"))
        {
            this.LoadAssembly(assemblyFile.FullName);
            
        }

        foreach (var assemblyFile in directoryInfo.GetFiles("*.dll"))
        {
            this.LoadAssembly(assemblyFile.FullName);
           
        }


        var sb = new StringBuilder();


        foreach (var bindingInfo in this._bindingInfoCache.Values)
        {

            if (bindingInfo.AssemblyVersion == null)
            {
                sb.AppendLine($"{bindingInfo.AssemblyName} not loaded! VMin: {bindingInfo.LowestAssemblyVersion} VMax: { bindingInfo.HighestAssemblyVersion}");
                continue;
            }

            if (bindingInfo.LowestAssemblyVersion == bindingInfo.AssemblyVersion &&
                bindingInfo.LowestAssemblyVersion == bindingInfo.AssemblyVersion) continue;


            var oldVersion = bindingInfo.LowestAssemblyVersion?.ToString();

            if (bindingInfo.LowestAssemblyVersion < bindingInfo.HighestAssemblyVersion)
            {
                oldVersion += $"-{bindingInfo.HighestAssemblyVersion}";
            }

            sb.AppendLine($"oldVersion={oldVersion} PublicKey={bindingInfo.PublicKey} newVersion={bindingInfo.AssemblyVersion}");
        }

        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bindings.txt"), sb.ToString());
        return 0;
    }

    private void LoadAssembly(string path)
    {
        try
        {
            var assemblyDefinition = Assembly.Comparers.Cecil.GlobalAssemblyResolver.Instance.GetAssemblyDefinition(path);

            if (assemblyDefinition == null) return;

            var name = assemblyDefinition.Name.Name;
            var references = assemblyDefinition.Modules.SelectMany(x => x.AssemblyReferences);
            var publicKey = assemblyDefinition.Name.PublicKeyTokenAsString;
            var version = assemblyDefinition.Name.Version;
            var key = $"Name={name}, PublicKey={publicKey}";

            if (this._bindingInfoCache.TryGetValue(key, out var item))
            {
                item.AssemblyVersion = version;
            }
            else
            {
                item = new BindingInfo(name)
                {
                    AssemblyVersion = version,
                    HighestAssemblyVersion = version,
                    LowestAssemblyVersion = version
                };
                this._bindingInfoCache[key] = item;
            }

            foreach (var reference in references)
            {
                name = reference.Name;
                publicKey = reference.PublicKeyTokenAsString;
                version = reference.Version;
                key = $"Name={name}, PublicKey={publicKey}";

                if (this._bindingInfoCache.TryGetValue(key, out item))
                {
                    if (item.HighestAssemblyVersion == null || 
                        item.HighestAssemblyVersion  < version) item.HighestAssemblyVersion = version;
                    if (item.LowestAssemblyVersion == null || 
                        item.LowestAssemblyVersion  > version) item.LowestAssemblyVersion = version;
                }
                else
                {
                    item = new BindingInfo(name)
                    {
                        HighestAssemblyVersion = version, 
                        LowestAssemblyVersion = version
                    };

                    this._bindingInfoCache[key] = item;
                }
            }
        }
        catch (Exception ex)
        {
           Console.WriteLine(ex);
        }
    }

}

internal class BindingInfo(string assemblyName)
{
    public string AssemblyName { get; } = assemblyName;
    public Version? AssemblyVersion { get; set; } 

    public Version? HighestAssemblyVersion { get; set; } 

    public Version? LowestAssemblyVersion { get; set; } 

    public string? PublicKey { get; set; }

}