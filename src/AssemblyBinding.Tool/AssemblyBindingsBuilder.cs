using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Extensions;
using Oleander.Assembly.Comparers.Cecil;
using Oleander.Extensions.Logging.Abstractions;
namespace Oleander.Assembly.Binding.Tool;

internal class AssemblyBindingsBuilder
{
    private static readonly ILogger logger = LoggerFactory.CreateLogger<AssemblyBindingsBuilder>();

    internal static Dictionary<string, AssemblyBindings> Create(DirectoryInfo directoryInfo)
    {
        var cache = new Dictionary<string, AssemblyBindings>();
        var assembliesInDirectory = new List<FileInfo>();

        logger.LogInformation("Load files from directory: '{directory}'.", directoryInfo.FullName);

        assembliesInDirectory.AddRange(directoryInfo.GetFiles("*.exe"));
        assembliesInDirectory.AddRange(directoryInfo.GetFiles("*.dll"));

        foreach (var assemblyFile in assembliesInDirectory)
        {
            try
            {
                PrivateCreate(cache, assemblyFile.FullName);
            }
            catch (Exception ex)
            {
                logger.LogError("Loading the file '{fileName}' caused an error: {ex}", assemblyFile.FullName, ex.GetAllMessages());
            }
        }

        return cache;
    }

    private static void PrivateCreate(IDictionary<string, AssemblyBindings> cache, string path)
    {
        try
        {
            var assemblyDefinition = GlobalAssemblyResolver.Instance.GetAssemblyDefinition(path);
            if (assemblyDefinition == null)
            {
                logger.LogInformation("File '{fileName}' is skipped because it is not a dotnet assembly.", Path.GetFileName(path));
                return;
            }

            logger.LogDebug("Collect information from the assembly: '{assemblyName}'.", assemblyDefinition.Name.Name);

            var key = assemblyDefinition.BuildAssemblyKey();

            if (cache.TryGetValue(key, out var binding))
            {
                binding.Resolved = true;
                binding.AssemblyVersion = assemblyDefinition.Name.Version;
            }
            else
            {
                binding = new AssemblyBindings(assemblyDefinition.Name.Name)
                {
                    Resolved = true,
                    AssemblyVersion = assemblyDefinition.Name.Version,
                    PublicKey = assemblyDefinition.Name.PublicKeyTokenAsString,
                    Culture = assemblyDefinition.Name.Culture
                };

                cache[key] = binding;
            }

            foreach (var reference in assemblyDefinition.Modules
                         .SelectMany(x => x.AssemblyReferences))
            {
                key = reference.BuildAssemblyKey();

                if (!cache.TryGetValue(key, out var refBinding))
                {
                    refBinding = new AssemblyBindings(reference.Name)
                    {
                        PublicKey = reference.PublicKeyTokenAsString,
                        Culture = reference.Culture
                    };

                    cache[key] = refBinding;
                }

                binding.ReferencedAssembly.Add(
                    new ReferencingAssembly(
                        reference. BuildFullAssemblyName(),
                        reference.Name,
                        reference.Version,
                        assemblyDefinition.Name.Version));

                refBinding.ReferencedByAssembly.Add(
                    new ReferencingAssembly(
                        assemblyDefinition.BuildFullAssemblyName(),
                        assemblyDefinition.Name.Name,
                        assemblyDefinition.Name.Version,
                        reference.Version));
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Collect information from the assembly '{fileName}' failed! {ex}.", Path.GetFileName(path), ex.GetAllMessages());
        }
    }
}