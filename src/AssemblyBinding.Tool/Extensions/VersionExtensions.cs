using Oleander.Assembly.Binding.Tool.Data;

namespace Oleander.Assembly.Binding.Tool.Extensions;

internal static class VersionExtensions
{
    internal static string ToVersionString(this Version? version)
    {
        if (version == null) return "0.0.0.0";

        var major = version.Major > 0 ? version.Major : 0;
        var minor = version.Minor > 0 ? version.Minor : 0;
        var build = version.Build > 0 ? version.Build : 0; 
        var revision = version.Revision > 0 ? version.Revision : 0;
        
        return $"{major}.{minor}.{build}.{revision}";
    }
}