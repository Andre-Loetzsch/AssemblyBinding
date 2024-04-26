using System.Xml;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Xml;

namespace Oleander.Assembly.Binding.Test;

public class CreateOrUpdateAssemblyBindingTest
{
    [Fact]
    public void TestCreateAppConfigFile()
    {
        var ab1 = new AssemblyBindings("Test.Assembly1")
        {
            AssemblyVersion = new Version(2, 76, 0, 3),
            PublicKey = "1234567890",
            Culture = "de-De", 
            Resolved = true
        };

        // Version 3 digit format!
        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(1, 9, 47)));
        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(2, 29, 47, 0)));

        var assemblyBindings = new List<AssemblyBindings> { ab1 };
        var expectedConfigPath = Path.Combine(AppContext.BaseDirectory, "appNew.expected.config");
        var appCopyConfigPath = Path.Combine(AppContext.BaseDirectory, "app.config");

        if (File.Exists(appCopyConfigPath)) File.Delete(appCopyConfigPath);

        ApplicationConfiguration.CreateOrUpdateAssemblyBinding(assemblyBindings, appCopyConfigPath);

        Assert.True(File.Exists(appCopyConfigPath));
        Assert.Equal(File.ReadAllText(expectedConfigPath), File.ReadAllText(appCopyConfigPath));
    }


    [Fact]
    
    public void TestCreateConfigurationAndRuntimeXmlElement()
    {
        var ab1 = new AssemblyBindings("Test.Assembly1")
        {
            AssemblyVersion = new Version(2, 76, 0, 3),
            PublicKey = "1234567890",
            Culture = "de-De",
            Resolved = true
        };
        
        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(1, 9, 47, 0)));
        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(2, 29, 47, 0)));

        var assemblyBindings = new List<AssemblyBindings> { ab1 };
        var appCopyConfigPath = Path.Combine(AppContext.BaseDirectory, "app1.config");

       Assert.Throws<XmlException>(() =>
        {
            ApplicationConfiguration.CreateOrUpdateAssemblyBinding(assemblyBindings, appCopyConfigPath);
        });
    }

    [Fact]
    public void TestCreateRuntimeXmlElement()
    {
        var ab1 = new AssemblyBindings("Test.Assembly1")
        {
            AssemblyVersion = new Version(2, 76, 0, 3),
            PublicKey = "1234567890",
            Culture = "de-De",
            Resolved = true
        };

        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(1, 9, 47, 0)));
        // Version 3 digit format!
        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(2, 29, 47)));

        var assemblyBindings = new List<AssemblyBindings> { ab1 };
        var expectedConfigPath = Path.Combine(AppContext.BaseDirectory, "app2.expected.config");
        var appCopyConfigPath = Path.Combine(AppContext.BaseDirectory, "app2.config");

        ApplicationConfiguration.CreateOrUpdateAssemblyBinding(assemblyBindings, appCopyConfigPath);

        Assert.True(File.Exists(appCopyConfigPath));
        Assert.Equal(File.ReadAllText(expectedConfigPath), File.ReadAllText(appCopyConfigPath));
    }

    [Fact]
    public void TestUpdateApplicationConfig()
    {
        var ab1 = new AssemblyBindings("Test.Assembly1")
        {
            AssemblyVersion = new Version(2, 76, 0, 3),
            PublicKey = "1234567890",
            Culture = "de-De",
            Resolved = true
        };

        // Version 3 digit format!
        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(1, 9, 47)));
        // Version 3 digit format!
        ab1.ReferencedByAssembly.Add(new ReferencingAssembly("x.y", "x", new Version(), new Version(2, 29, 47)));

        var assemblyBindings = new List<AssemblyBindings> { ab1 };
        var expectedConfigPath = Path.Combine(AppContext.BaseDirectory, "app3.expected.config");
        var appCopyConfigPath = Path.Combine(AppContext.BaseDirectory, "app3.config");

        ApplicationConfiguration.CreateOrUpdateAssemblyBinding(assemblyBindings, appCopyConfigPath);

        Assert.True(File.Exists(appCopyConfigPath));
        Assert.Equal(File.ReadAllText(expectedConfigPath), File.ReadAllText(appCopyConfigPath));
    }
}