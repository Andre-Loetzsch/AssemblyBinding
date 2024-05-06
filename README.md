# Oleander.Assembly.Binding
Version conflicts often occur when using NuGet packages in the Dotnet Framework. These then need to be fixed in App.config by adding 
assembly version redirects. This can quickly become a tedious task. This little tool is intended to provide support. It examines all 
assembly references in the execution directory and creates the necessary binding redirects from them. It will always redirect to the
assembly with the highest version. In addition, some reports are generated which provide additional 
information.

## Install:
dotnet tool install dotnet-assembly-binding-tool --global --prerelease

## Update:
dotnet tool update  dotnet-assembly-binding-tool --global --prerelease

## Install:
dotnet tool uninstall dotnet-assembly-binding-tool --global



## Tool name:
  assembly-binding-tool

## Usage:
    Oleander.Assembly.Binding.Tool [command] [options]

### Options:
 - **--version**<br>*Show version information*
 - **-?, -h, --help**<br>*Show help and usage information*

## Commands:
  - resolve<br>*Resolve assembly bindings*

### Options:
 - **--base-dir**<br>*The base application directory*
 - **--app-config**<br>*The application configuration file*
 - **--no-report**<br>*No report will be generated*
 - **--recursive**<br>*Recursively updates all configuration files*
 - **--branch**<br>*Name of the branch used as a search filter (only valid with option --recursive true)*
 - **--configuration-name**<br>*Name of the configuration (Release, Debug) (only valid with option --recursive true)*
 - **-?, -h, --help**<br>*Show help and usage information*

### Example:
assembly-binding resolve --app-config D:/dev/MyApp/MyApp.exe.config --base-dir D:/dev/MyApp/

