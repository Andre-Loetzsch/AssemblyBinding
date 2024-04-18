using System.CommandLine;
using System.Text;
using Microsoft.Extensions.Logging;
using Oleander.AssemblyBinding.Tool.Data;
using Oleander.AssemblyBinding.Tool.Options;

namespace Oleander.AssemblyBinding.Tool.Commands;


internal class AssemblyBindingCommand : Command
{
    private readonly Dictionary<string, BindingInfo> _bindingInfoCache = new();
    private readonly Dictionary<string, List<string>> _assemblyDependingOn = new();


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

        var assemblyBindingSb = new StringBuilder();
        var missingAssembliesSb = new StringBuilder();
        var assemblyDependingOnSb = new StringBuilder();

        foreach (var item in this._assemblyDependingOn)
        {
            if (IsExcludedAssemblies(item.Key)) continue;

            var dependingList = item.Value.Where(x => !IsExcludedAssemblies(x)).ToList();

            if (dependingList.Count == 0) continue;

            if (assemblyDependingOnSb.Length > 0)
            {
                assemblyDependingOnSb.AppendLine();
            }

            assemblyDependingOnSb.AppendLine(item.Key);

            foreach (var subItem in dependingList)
            {
                assemblyDependingOnSb.Append("  -> ");
                assemblyDependingOnSb.AppendLine(subItem);
            }
        }

        var outPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "out");
        if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);

        File.WriteAllText(Path.Combine(outPath, "DependingOn.txt"), assemblyDependingOnSb.ToString());

        assemblyBindingSb.AppendLine("<assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">");

        foreach (var bindingInfo in this._bindingInfoCache.Values)
        {
            if (bindingInfo.AssemblyVersion == null)
            {
                if (IsExcludedAssemblies(bindingInfo.AssemblyName)) continue;
                missingAssembliesSb.AppendLine(bindingInfo.AssemblyName);
                missingAssembliesSb.AppendLine(bindingInfo.LowestAssemblyVersion == bindingInfo.HighestAssemblyVersion ? 
                    $"version: {bindingInfo.LowestAssemblyVersion}" : 
                    $"oldVersion: {bindingInfo.LowestAssemblyVersion} newVersion: {bindingInfo.HighestAssemblyVersion}");

                missingAssembliesSb.AppendLine("-----------------------------------------------------------------------");
                continue;
            }

            if (bindingInfo.LowestAssemblyVersion == bindingInfo.AssemblyVersion &&
                bindingInfo.HighestAssemblyVersion == bindingInfo.AssemblyVersion) continue;

            var oldVersion = bindingInfo.LowestAssemblyVersion?.ToString();

            if (bindingInfo.LowestAssemblyVersion < bindingInfo.HighestAssemblyVersion)
            {
                oldVersion += $"-{bindingInfo.HighestAssemblyVersion}";
            }

            var publicKey = string.IsNullOrEmpty(bindingInfo.PublicKey) ? "null" : bindingInfo.PublicKey;
            var culture = string.IsNullOrEmpty(bindingInfo.Culture) ? "neutral" : bindingInfo.Culture;

            assemblyBindingSb.AppendLine("  <dependentAssembly>");
            assemblyBindingSb.AppendLine($"    <assemblyIdentity name=\"{bindingInfo.AssemblyName}\" publicKeyToken=\"{publicKey}\" culture=\"{culture}\" />");
            assemblyBindingSb.AppendLine($"    <bindingRedirect oldVersion=\"{oldVersion}\" newVersion=\"{bindingInfo.AssemblyVersion}\" />");
            assemblyBindingSb.AppendLine("  </dependentAssembly>");
        }

        assemblyBindingSb.AppendLine("</assemblyBinding>");

        File.WriteAllText(Path.Combine(outPath, "bindings.txt"), assemblyBindingSb.ToString());
        File.WriteAllText(Path.Combine(outPath, "missing.txt"), missingAssembliesSb.ToString());
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
            var culture = assemblyDefinition.Name.Culture;

            Console.WriteLine(name);

            var key = BuildAssemblyKey(name, publicKey, culture);

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
                    LowestAssemblyVersion = version,
                    PublicKey = publicKey,
                    Culture = culture
                };

                this._bindingInfoCache[key] = item;
            }

            foreach (var reference in references)
            {

                if (!this._assemblyDependingOn.TryGetValue(reference.FullName, out var dependingOnList))
                {
                    dependingOnList = new List<string>();
                    this._assemblyDependingOn[reference.FullName] = dependingOnList;
                }

                if (!dependingOnList.Contains(assemblyDefinition.FullName)) dependingOnList.Add(assemblyDefinition.FullName);

                name = reference.Name;
                publicKey = reference.PublicKeyTokenAsString;
                version = reference.Version;
                culture = reference.Culture;

                key = BuildAssemblyKey(name, publicKey, culture);

                if (this._bindingInfoCache.TryGetValue(key, out item))
                {
                    item.Count += 1;
                    if (item.HighestAssemblyVersion == null ||
                        item.HighestAssemblyVersion < version) item.HighestAssemblyVersion = version;
                    if (item.LowestAssemblyVersion == null ||
                        item.LowestAssemblyVersion > version) item.LowestAssemblyVersion = version;
                }
                else
                {
                    item = new BindingInfo(name)
                    {
                        HighestAssemblyVersion = version,
                        LowestAssemblyVersion = version,
                        PublicKey = publicKey,
                        Culture = culture,
                        Count = 1
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

    private static bool IsExcludedAssemblies(string assemblyName)
    {
        return assemblyName.Equals("mscorlib", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.Equals("System", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.Equals("PresentationFramework", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.Equals("PresentationCore", StringComparison.InvariantCultureIgnoreCase) || 
               assemblyName.StartsWith("mscorlib,", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("System,", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("PresentationFramework,", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("PresentationCore,", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase);
    }


    private static string BuildAssemblyKey(string assemblyName, string publicKey, string culture)
    {
        return $"Name={assemblyName}, PublicKey={publicKey}, culture={culture}";
    }
}

