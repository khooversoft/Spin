using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;

namespace Toolbox.Tools.Local;

public class LocalProcessBuilder
{
    public string? ExecuteFile { get; set; }
    public string? Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
    public Action<string>? CaptureOutput { get; set; }
    public bool UseShellExecute { get; set; }

    public LocalProcessBuilder SetExecuteFile(string? exeFile) => this.Action(_ => ExecuteFile = exeFile);
    public LocalProcessBuilder SetArguments(string? arguments) => this.Action(_ => Arguments = arguments);
    public LocalProcessBuilder SetWorkingDirectory(string? workingDirectory) => this.Action(_ => WorkingDirectory = workingDirectory);
    public LocalProcessBuilder SetCaptureOutput(Action<string>? captureOutput) => this.Action(_ => CaptureOutput = captureOutput);
    public LocalProcessBuilder SetUseShellExecute(bool useShellExecute) => this.Action(_ => UseShellExecute = useShellExecute);

    public LocalProcessBuilder SetCommandLine(string commandLine)
    {
        IReadOnlyList<IToken> tokens = new StringTokenizer()
            .UseCollapseWhitespace()
            .UseDoubleQuote()
            .UseSingleQuote()
            .Add(" ")
            .Parse(commandLine);

        tokens.Count.Assert(x => x >= 1, "Bad commandline");

        string exeFile = tokens.First().Value;
        exeFile.Assert(x => x.IsNotEmpty(), "Bad commandline - not execute file");

        string? args = tokens.Skip(2).Select(x => x.Value).Aggregate(string.Empty, (a, x) => a += x).ToNullIfEmpty();
        string? workingDirectory = Path.GetDirectoryName(exeFile).ToNullIfEmpty();

        ExecuteFile = exeFile;
        Arguments = args;
        WorkingDirectory = workingDirectory;

        return this;
    }

    public LocalProcessRun Build() => new LocalProcessRun(BuildProcess(), CaptureOutput);

    private Process BuildProcess()
    {
        var process = new Process()
        {
            EnableRaisingEvents = true,

            StartInfo = new ProcessStartInfo
            {
                FileName = ExecuteFile ?? string.Empty,
                Arguments = Arguments ?? string.Empty,
                WorkingDirectory = WorkingDirectory ?? string.Empty,
                UseShellExecute = UseShellExecute,
                CreateNoWindow = true,
                RedirectStandardOutput = UseShellExecute == false,
                RedirectStandardError = UseShellExecute == false,
                RedirectStandardInput = UseShellExecute == false,
            }
        };

        return process;
    }
}
