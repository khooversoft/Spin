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

    public string CallerFunction { get; }
    public string CallerFilePath { get; }
    public int CallerLineNumber { get; }
}
