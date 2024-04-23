using System.CommandLine.IO;

namespace Oleander.Assembly.Binding.Tool;

internal class StreamWriterDelegate(Action<string> writeAction) : IStandardStreamWriter
{
    public void Write(string? value)
    {
        if (value == null) return;

        writeAction(value);
    }
}