using Oleander.Assembly.Binding.Tool.Data;
using System.Xml;
using Oleander.Assembly.Binding.Tool.Extensions;

namespace Oleander.Assembly.Binding.Tool.Xml;
internal static class ApplicationConfiguration
{
    internal static void CreateOrUpdateAssemblyBinding(IEnumerable<AssemblyBindings> bindings, string appConfigurationFile)
    {
        var doc = LoadOrCreateXmlDocument(appConfigurationFile);
        var manager = new XmlNamespaceManager(doc.NameTable);
        
        manager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");

        var runtime = doc.SelectSingleNode("//runtime");
        if (runtime == null) throw new NullReferenceException("The runtime element cannot be null!");


        var assemblyBindingElements = doc.SelectNodes("//asm:assemblyBinding", manager);

        if (assemblyBindingElements != null)
        {
            foreach (XmlNode element in assemblyBindingElements)
            {
                element.ParentNode?.RemoveChild(element);
            }
        }

        var assemblyBinding = doc.CreateElement("assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
        runtime.AppendChild(assemblyBinding);

        foreach (var bindingInfo in bindings
                     .Where(x => x is { Resolved: true, ReferencedByAssembly.Count: > 0 })
                     .OrderBy(x => x.AssemblyName).ToList())
        {

            if (!bindingInfo.TryGetMinVersion(out var minAssemblyVersion)) continue;
            if (!bindingInfo.TryGetMaxVersion(out var maxAssemblyVersion)) continue;

            if (minAssemblyVersion == bindingInfo.AssemblyVersion &&
                maxAssemblyVersion == bindingInfo.AssemblyVersion) continue;

            var oldVersion = minAssemblyVersion?.ToString();

            if (minAssemblyVersion < maxAssemblyVersion)
            {
                oldVersion += $"-{maxAssemblyVersion}";
            }

            var dependentAssembly = doc.CreateElement("dependentAssembly", "urn:schemas-microsoft-com:asm.v1");
            assemblyBinding.AppendChild(dependentAssembly);

            var assemblyIdentity = doc.CreateElement("assemblyIdentity", "urn:schemas-microsoft-com:asm.v1");
            assemblyIdentity.SetAttribute("name", bindingInfo.AssemblyName);
            assemblyIdentity.SetAttribute("publicKeyToken", bindingInfo.PublicKey);
            assemblyIdentity.SetAttribute("culture", bindingInfo.Culture);
            dependentAssembly.AppendChild(assemblyIdentity);

            var bindingRedirect = doc.CreateElement("bindingRedirect", "urn:schemas-microsoft-com:asm.v1");
            bindingRedirect.SetAttribute("oldVersion", oldVersion);
            bindingRedirect.SetAttribute("newVersion", bindingInfo.AssemblyVersion.ToString());
            dependentAssembly.AppendChild(bindingRedirect);
        }

        // Änderungen speichern
        doc.Save(appConfigurationFile);
    }

    private static XmlDocument LoadOrCreateXmlDocument(string appConfigurationFile)
    {
        var doc = new XmlDocument();

        if (File.Exists(appConfigurationFile))
        {
            doc.Load(appConfigurationFile);

            if (doc.SelectSingleNode("//runtime") != null) return doc;

            var runtime = doc.CreateElement("runtime");

            if (doc.SelectSingleNode("//configuration") is XmlElement configuration)
            {
                configuration.AppendChild(runtime);
            }
            else
            {
                configuration = doc.CreateElement("configuration");
                doc.AppendChild(configuration);
                configuration.AppendChild(runtime);
            }
        }
        else
        {
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));
            var configuration = doc.CreateElement("configuration");
            doc.AppendChild(configuration);
            var runtime = doc.CreateElement("runtime");
            configuration.AppendChild(runtime);
        }

        return doc;
    }
}