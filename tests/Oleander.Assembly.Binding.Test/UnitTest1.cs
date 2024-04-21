using System.Text;
using Oleander.Assembly.Binding.Tool.Data;
using Oleander.Assembly.Binding.Tool.Xml;

namespace Oleander.Assembly.Binding.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {

        var sb = new StringBuilder();
        sb.AppendLine("<assemblyBinding xmlns=\"urn:schemas-microsoft-com:asm.v1\">");
        sb.AppendLine("  <dependentAssembly>");
        sb.AppendLine("    <assemblyIdentity name=\"MyAssembly1\" publicKeyToken=\"32ab4ba45e0a69a1\" culture=\"neutral\" />");
        sb.AppendLine("    <bindingRedirect oldVersion=\"1.0.0.0-2.0.14.0\" newVersion=\"2.0.14.0\" />");
        sb.AppendLine("  </dependentAssembly>");
        sb.AppendLine("  <dependentAssembly>");
        sb.AppendLine("    <assemblyIdentity name=\"MyAssembly2\" publicKeyToken=\"32ab4ba45e0a69a1\" culture=\"neutral\" />");
        sb.AppendLine("    <bindingRedirect oldVersion=\"0.0.0.0-4.10.19.0\" newVersion=\".10.19.0\" />");
        sb.AppendLine("  </dependentAssembly>");
        sb.AppendLine("</assemblyBinding>");


        var app1ConfigPath = Path.Combine(AppContext.BaseDirectory, "App1.config");
        var appContent = File.ReadAllText(app1ConfigPath);

        var startIndex = appContent.IndexOf("<assemblyBinding ", StringComparison.InvariantCultureIgnoreCase);
        var endIndex = appContent.IndexOf("</assemblyBinding>", StringComparison.InvariantCultureIgnoreCase);

        if (startIndex >= 0 && endIndex > startIndex)
        {
            var bindings = appContent.Substring(startIndex, endIndex - startIndex + 18);
        }

        var part1 = appContent.Substring(0, startIndex);
        var part2 = appContent.Substring(endIndex, appContent.Length - endIndex);

        appContent = string.Concat(part1, sb, part2);



        foreach (var line in File.ReadAllLines(app1ConfigPath))
        {



        }


    }


    [Fact]
    public void Test2()
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

        var assemblyBindins = new List<AssemblyBindings> { ab1 };

        var app1ConfigPath = Path.Combine(AppContext.BaseDirectory, "App1.config");
        var app1CopyConfigPath = Path.Combine(AppContext.BaseDirectory, "App1Copy.config");

        File.Copy(app1ConfigPath, app1CopyConfigPath, true);

        ApplicationConfiguration.UpdateAssemblyBinding(assemblyBindins, app1CopyConfigPath);

    }
}