using Oleander.Assembly.Comparers.Cecil;
using Oleander.AssemblyBinding.Tool.Data;

namespace Oleander.AssemblyBinding.Tool;

internal static class AssemblyBindingsBuilder
{
    internal static Dictionary<string, AssemblyBindings> Create(DirectoryInfo directoryInfo)
    {
        var cache = new Dictionary<string, AssemblyBindings>();

        foreach (var assemblyFile in directoryInfo.GetFiles("*.exe"))
        {
            try
            {
                PrivateCreate(cache, assemblyFile.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        foreach (var assemblyFile in directoryInfo.GetFiles("*.dll"))
        {
            try
            {
                PrivateCreate(cache, assemblyFile.FullName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        return cache;
    }

    private static void PrivateCreate(IDictionary<string, AssemblyBindings> cache, string path)
    {
        try
        {
            var assemblyDefinition = GlobalAssemblyResolver.Instance.GetAssemblyDefinition(path);
            if (assemblyDefinition == null) return;

            Console.WriteLine(assemblyDefinition.Name.Name);

            var key = assemblyDefinition.BuildAssemblyKey();

            if (cache.TryGetValue(key, out var binding))
            {
                binding.AssemblyVersion = assemblyDefinition.Name.Version;
            }
            else
            {
                binding = new AssemblyBindings(assemblyDefinition.Name.Name)
                {
                    AssemblyVersion = assemblyDefinition.Name.Version,
                    PublicKey = assemblyDefinition.Name.PublicKeyTokenAsString,
                    Culture = assemblyDefinition.Name.Culture
                };

                cache[key] = binding;
            }

            foreach (var reference in assemblyDefinition.Modules
                         .SelectMany(x => x.AssemblyReferences)
                         .Where(x => !AssemblyBindings.IsExcludedAssemblies(x.Name)))
            {
                key = reference.BuildAssemblyKey();

                if (!cache.TryGetValue(key, out var refBinding))
                {
                    refBinding = new AssemblyBindings(reference.Name)
                    {
                        AssemblyVersion = reference.Version,
                        PublicKey = reference.PublicKeyTokenAsString,
                        Culture = reference.Culture
                    };

                    cache[key] = refBinding;
                }

                refBinding.ReferencedByAssembly.Add(
                    new ReferencingAssembly(
                        assemblyDefinition.BuildFullAssemblyName(),
                        assemblyDefinition.Name.Name,
                        assemblyDefinition.Name.Version, reference.Version));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}