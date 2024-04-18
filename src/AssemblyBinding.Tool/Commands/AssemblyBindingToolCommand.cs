using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Oleander.AssemblyBinding.Tool.Data;
using Oleander.AssemblyBinding.Tool.Options;

namespace Oleander.AssemblyBinding.Tool.Commands;


internal class AssemblyBindingCommand : Command
{
    private readonly Dictionary<string, BindingInfo> _bindingInfoCache = new();
    //private readonly Dictionary<string, List<string>> _assemblyDependingOn = new();


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

        foreach (var item in this._bindingInfoCache.OrderBy(x => x.Key))
        {
            if (IsExcludedAssemblies(item.Value.AssemblyName)) continue;

            var dependingList = item.Value.ReferencedByAssembly
                .Where(x => !IsExcludedAssemblies(x.AssemblyName))
                .OrderBy(x => x.AssemblyName)
                .ToList();

            if (dependingList.Count == 0) continue;

            if (assemblyDependingOnSb.Length > 0)
            {
                assemblyDependingOnSb.AppendLine();
            }

            assemblyDependingOnSb.AppendLine(item.Key);

            var versionGroup = dependingList.GroupBy(x => x.ReferencedByAssemblyVersion);

            foreach (var groupItem in versionGroup)
            {
                assemblyDependingOnSb.AppendLine($"  {groupItem.Key}:");

                foreach (var subItem in groupItem)
                {
                    assemblyDependingOnSb.Append("    -> ");
                    assemblyDependingOnSb.AppendLine($"{subItem.AssemblyName}, Version={subItem.AssemblyVersion}, PublicKey={subItem.PublicKey}, Culture={subItem.Culture}");
                }
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

            if (this._bindingInfoCache.TryGetValue(key, out var bindingInfo))
            {
                bindingInfo.AssemblyVersion = version;
            }
            else
            {
                bindingInfo = new BindingInfo(name)
                {
                    AssemblyVersion = version,
                    HighestAssemblyVersion = version,
                    LowestAssemblyVersion = version,
                    PublicKey = publicKey,
                    Culture = culture
                };

                this._bindingInfoCache[key] = bindingInfo;
            }

            foreach (var reference in references)
            {
                name = reference.Name;
                publicKey = reference.PublicKeyTokenAsString;
                version = reference.Version;
                culture = reference.Culture;

                key = BuildAssemblyKey(name, publicKey, culture);

                if (this._bindingInfoCache.TryGetValue(key, out var refBindingInfo))
                {
                    refBindingInfo.Count += 1;
                    if (refBindingInfo.HighestAssemblyVersion == null ||
                        refBindingInfo.HighestAssemblyVersion < version) refBindingInfo.HighestAssemblyVersion = version;
                    if (refBindingInfo.LowestAssemblyVersion == null ||
                        refBindingInfo.LowestAssemblyVersion > version) refBindingInfo.LowestAssemblyVersion = version;
                }
                else
                {
                    refBindingInfo = new BindingInfo(name)
                    {
                        HighestAssemblyVersion = version,
                        LowestAssemblyVersion = version,
                        PublicKey = publicKey,
                        Culture = culture,
                        Count = 1
                    };

                    this._bindingInfoCache[key] = refBindingInfo;
                }

                bindingInfo.ReferencedByAssemblyVersion = bindingInfo.AssemblyVersion;
                refBindingInfo.ReferencedByAssembly.Add(bindingInfo);
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
               assemblyName.StartsWith("System.", StringComparison.InvariantCultureIgnoreCase) ||
               assemblyName.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase);
    }


    private static string BuildAssemblyKey(string assemblyName, string publicKey, string culture)
    {
        return $"Name={assemblyName}, PublicKey={publicKey}, culture={culture}";
    }
}

