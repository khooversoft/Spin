using System.CommandLine;
using System.CommandLine.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Toolbox.Types;

namespace Toolbox.CommandRouter;

internal class ConsoleCapature : IConsole
{
    public ConsoleCapature(ScopeContextLocation context)
    {
        var writer = new Writer(context);
        Out = writer;
        Error = writer;
    }

    public IStandardStreamWriter Out { get; }
    public bool IsOutputRedirected => true;
    public IStandardStreamWriter Error { get; }

    public bool IsErrorRedirected => true;
    public bool IsInputRedirected => true;

    public void Dump() => ((Writer)Out).Dump();

    private class Writer : IStandardStreamWriter
    {
        private readonly StringBuilder _line = new();
        private readonly ScopeContextLocation _context;

        public Writer(ScopeContextLocation context) => _context = context;

        public void Write(string? value)
        {
            (bool term, string resultValue) = value switch
            {
                string v when v == "\r" => (true, string.Empty),
                string v when v == "\n" => (true, string.Empty),
                string v when v == "\r\n" => (true, string.Empty),
                string v when v.EndsWith("\r\n") => (true, value[0..^3]),
                string v when v.EndsWith("\n") => (true, value[0..^2]),
                string v when v.EndsWith("\r") => (true, value[0..^2]),

                _ => (false, value ?? string.Empty)
            };

            _line.Append(resultValue);

            if (term) Dump();
        }

        public void Dump()
        {
            if (_line.Length == 0) return;

            _context.LogInformation("From Command Router: {line}", _line.ToString());
            _line.Clear();
        }
    }
}
