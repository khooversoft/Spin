using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly struct CodeLocation
{
    public CodeLocation([CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0)
    {
        CallerFunction = function.NotEmpty();
        CallerFilePath = path.NotEmpty();
        CallerLineNumber = lineNumber;
    }

    public CodeLocation(string function, string path, int lineNumber, string argumentName)
    {
        CallerFunction = function.NotEmpty();
        CallerFilePath = path.NotEmpty();
        CallerLineNumber = lineNumber;
        ArgumentName = argumentName.NotEmpty();
    }

    public string CallerFunction { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }
    public string? ArgumentName { get; }
}
