# Oleander.Assembly.Binding
Assembly binding tool creates or updates the assemblyBinding section of an application configuration file based on the
assemblies in the bin directory. In addition, some reports are generated to obtain more information about the assemblies used.

## Install
dotnet tool install --global dotnet-assembly-binding-tool --version *%version%*
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
  - --base-dir&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*The base application directory*
  - --app-config&nbsp;&nbsp;&nbsp;*The application configuration file*
  - --no-report&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;*No report will be generated*
  - -?, -h, --help&nbsp;&nbsp;&nbsp;&nbsp;*Show help and usage information*


