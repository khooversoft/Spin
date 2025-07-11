using System.CommandLine;
using System.CommandLine.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.CommandRouter;

internal class ConsoleCapture : IConsole
{
    public ConsoleCapture()
    {
        var writer = new Writer();
        Out = writer;
        Error = writer;
    }

    public IStandardStreamWriter Out { get; }
    public bool IsOutputRedirected => true;
    public IStandardStreamWriter Error { get; }

    public bool IsErrorRedirected => true;
    public bool IsInputRedirected => true;

    public void Dump(ScopeContext context) => ((Writer)Out).Dump(context);

    private class Writer : IStandardStreamWriter
    {
        private readonly StringBuilder _line = new();
        private object _lock = new object();

        public void Write(string? value)
        {
            lock (_lock)
            {
                if (value != null) _line.Append(value);
            }
        }

        public void Dump(ScopeContext context)
        {
            lock (_lock)
            {
                var line = _line.ToString();
                _line.Clear();
                if (line.IsEmpty()) return;

                var logText = line.Replace("\n", null).Replace("\r", Environment.NewLine).TrimEnd();
                context.LogInformation("From Command Router: {line}", logText);
            }
        }
    }
}
