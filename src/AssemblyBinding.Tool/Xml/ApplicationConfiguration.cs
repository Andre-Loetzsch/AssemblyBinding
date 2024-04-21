using Oleander.Assembly.Binding.Tool.Data;
using System.Xml;

namespace Oleander.Assembly.Binding.Tool.Xml;
internal class ApplicationConfiguration
{
    internal static void UpdateAssemblyBinding(IEnumerable<AssemblyBindings> bindings, string configPath)
    {
        // Pfad zur app.config Datei

        // XmlDocument-Instanz erstellen und app.config laden
        var doc = LoadXmlDocument(configPath);

        // XmlNamespaceManager für das namespace der assemblyBinding Elemente erstellen
        XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
        manager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");


        XmlNode runtime = doc.SelectSingleNode("//runtime");

        if (runtime == null)
        {
            // TODO add runtime element
            return;
        }


        // assemblyBinding Elemente auswählen
        XmlNodeList assemblyBindingElements = doc.SelectNodes("//asm:assemblyBinding", manager);

        foreach (XmlNode element in assemblyBindingElements)
        {
            element.ParentNode.RemoveChild(element);
        }

        var assemblyBinding = doc.CreateElement("assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
        runtime.AppendChild(assemblyBinding);


        foreach (var bindingInfo in bindings
                     .Where(x => x is { Resolved: true, ReferencedByAssembly.Count: > 0 })
                     .OrderBy(x => x.AssemblyName).ToList())
        {
            var minAssemblyVersion = bindingInfo.ReferencedByAssembly.Min(x => x.ReferencingAssemblyVersion);
            var maxAssemblyVersion = bindingInfo.ReferencedByAssembly.Max(x => x.ReferencingAssemblyVersion);

            if (minAssemblyVersion == bindingInfo.AssemblyVersion &&
                maxAssemblyVersion == bindingInfo.AssemblyVersion) continue;

            var oldVersion = minAssemblyVersion?.ToString();

            if (minAssemblyVersion < maxAssemblyVersion)
            {
                oldVersion += $"-{maxAssemblyVersion}";
            }

            // Neues dependentAssembly Element hinzufügen
            XmlElement dependentAssembly = doc.CreateElement("dependentAssembly", "urn:schemas-microsoft-com:asm.v1");
            assemblyBinding.AppendChild(dependentAssembly);

            // Neues assemblyIdentity Element hinzufügen
            XmlElement assemblyIdentity = doc.CreateElement("assemblyIdentity", "urn:schemas-microsoft-com:asm.v1");
            assemblyIdentity.SetAttribute("name", bindingInfo.AssemblyName);
            assemblyIdentity.SetAttribute("publicKeyToken", bindingInfo.PublicKey);
            assemblyIdentity.SetAttribute("culture", bindingInfo.Culture);
            dependentAssembly.AppendChild(assemblyIdentity);

            // Neues bindingRedirect Element hinzufügen
            XmlElement bindingRedirect = doc.CreateElement("bindingRedirect", "urn:schemas-microsoft-com:asm.v1");
            bindingRedirect.SetAttribute("oldVersion", oldVersion);
            bindingRedirect.SetAttribute("newVersion", bindingInfo.AssemblyVersion.ToString());
            dependentAssembly.AppendChild(bindingRedirect);
        }

        // Änderungen speichern
        doc.Save(configPath);
    }





    private static XmlDocument LoadXmlDocument(string configPath)
    {
        // XmlDocument-Instanz erstellen und app.config laden
        XmlDocument doc = new XmlDocument();

        if (File.Exists(configPath))
        {
            doc.Load(configPath);

            // Überprüfen, ob das runtime-Element existiert
            XmlElement runtime = (XmlElement)doc.SelectSingleNode("//runtime");
            if (runtime == null)
            {
                // runtime-Element hinzufügen
                runtime = doc.CreateElement("runtime");
                XmlElement configuration = (XmlElement)doc.SelectSingleNode("//configuration");
                if (configuration != null)
                {
                    configuration.AppendChild(runtime);
                }
                else
                {
                    // configuration-Element hinzufügen, wenn es nicht existiert
                    configuration = doc.CreateElement("configuration");
                    doc.AppendChild(configuration);
                    configuration.AppendChild(runtime);
                }

                //// Änderungen speichern
                //doc.Save(configPath);
            }
        }
        else
        {
            // Neue app.config Datei erstellen, wenn sie nicht existiert
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlElement configuration = doc.CreateElement("configuration");
            doc.AppendChild(configuration);
            XmlElement runtime = doc.CreateElement("runtime");
            configuration.AppendChild(runtime);
            //doc.Save(configPath);
        }

        return doc;
    }



}