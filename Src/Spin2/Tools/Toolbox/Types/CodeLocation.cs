using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Types;

public readonly struct CodeLocation
{
    public CodeLocation([CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0)
    {
        Function = function.NotEmpty();
        Path = path.NotEmpty();
        LineNumber = lineNumber;
    }

    public string Function { get; }
    public string Path { get; }
    public int LineNumber { get; }
}
