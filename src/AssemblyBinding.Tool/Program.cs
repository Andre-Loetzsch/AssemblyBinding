using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oleander.Assembly.Binding.Tool.Commands;
using Oleander.Assembly.Binding.Tool.Options;
using Oleander.Extensions.DependencyInjection;
using Oleander.Extensions.Hosting.Abstractions;
using Oleander.Extensions.Logging;
using Oleander.Extensions.Logging.Abstractions;
using Oleander.Extensions.Logging.Providers;

namespace Oleander.Assembly.Binding.Tool;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"), true, false);

        builder.Services
            .AddSingleton<AssemblyBindingTool>()
            .AddConfiguredTypes("loggerTypes");

        builder.Logging
            .ClearProviders()
            .AddConfiguration(builder.Configuration.GetSection("Logging"))
            .Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerSinkProvider>());

        var host = builder.Build();
        host.Services.InitLoggerFactory();

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
        var console = new ToolConsole(logger);
        var assemblyBindingTool = host.Services.GetRequiredService<AssemblyBindingTool>();
        var rootCommand = new RootCommand("assembly-binding-tool");
        var commandLine = new CommandLineBuilder(rootCommand)
            .UseDefaults() // automatically configures dotnet-suggest
            .Build();

        TabCompletions.Logger = logger;

        logger.CreateMSBuildMessage("ABT0", "assembly-binding-tool started...", "Main");
        rootCommand.AddCommand(new ResolveCommand(assemblyBindingTool));

        var exitCode = await commandLine.InvokeAsync(args, console);

        console.Flush();

        const string logMsg = "assembly-binding '{args}' exit with exit code {exitCode}";

        var arguments = string.Join(" ", args);

        if (exitCode == 0)
        {
            logger.LogInformation(logMsg, arguments, exitCode);

            if (!arguments.StartsWith("[suggest:"))
            {
                logger.CreateMSBuildMessage("ABT0", $"assembly-binding {exitCode}", "Main");
            }
        }
        else
        {
            logger.LogError(logMsg, arguments, exitCode);
        }

        await host.LogConfiguredTypesExceptions<Program>(true).WaitForLoggingAsync(TimeSpan.FromSeconds(5));
        return exitCode;
    }
}