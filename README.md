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
  - **--version**&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*Show version information*
  - **-?, -h, --help**  &nbsp;*Show help and usage information*

## Commands:
  - resolve&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*Resolve assembly bindings*

### Options:
  - **--base-dir**&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*The base application directory*
  - **--app-config**&nbsp;&nbsp;&nbsp;*The application configuration file*
  - **--no-report**&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*No report will be generated*
  - **-?, -h, --help**&nbsp;&nbsp;&nbsp;&nbsp;*Show help and usage information*

### Example:
assembly-binding resolve --app-config D:/dev/MyApp/MyApp.exe.config --base-dir D:/dev/MyApp/


