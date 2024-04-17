using System;
using System.CommandLine;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Oleander.AssemblyBinding.Tool.Options;

namespace Oleander.AssemblyBinding.Tool.Commands;


internal class AssemblyBindingCommand : Command
{
    public AssemblyBindingCommand(ILogger logger, AssemblyBindingTool tool) : base("resolve", "resolve assembly bindings")
    {

        var baseDirOption = new BaseDirOption();

        this.AddOption(baseDirOption);

        this.SetHandler(baseDir => Task.FromResult(this.Resolve(baseDir)), baseDirOption);
    }

    private int Resolve(DirectoryInfo directoryInfo)
    {
        var sb = new StringBuilder();

        foreach (var assemblyFile in directoryInfo.GetFiles("*.exe"))
        {
            this.LoadAssembly(sb, assemblyFile.FullName);
            sb.AppendLine();
        }

        foreach (var assemblyFile in directoryInfo.GetFiles("*.dll"))
        {
            this.LoadAssembly(sb, assemblyFile.FullName);
            sb.AppendLine();
        }

        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bindings.txt"), sb.ToString());
        return 0;
    }

    private void LoadAssembly(StringBuilder sb , string path)
    {
        

        try
        {
            var assembly = Assembly.LoadFrom(path);

            //var assemblyVersion = assembly.GetName().Version?.ToString();

            sb.AppendLine(assembly.FullName);

            foreach (var refAssembly in assembly.GetReferencedAssemblies())
            {
                sb.AppendLine($"  -> {refAssembly.FullName}");
            }

        }
        catch (Exception ex)
        {
            sb.AppendLine($"  -> {ex.Message}");
        }

      
    }

}