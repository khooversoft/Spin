using System.Runtime.CompilerServices;

namespace Toolbox.Types;

public static class ScopeContextTools
{
    public static string AppendMessage(string? message, string? addMessage) => message switch
    {
        null => string.Empty,
        string v => addMessage switch
        {
            null => v,
            string => v + ", " + addMessage,
        }
    };

    public static object?[] AppendArgs(object?[] args, params object[] addArgs)
    {
        object?[] result = (args, addArgs) switch
        {
            (null, { Length: 0 }) => addArgs,
            ({ Length: 0 }, { Length: 0 }) => args,
            _ => append(),
        };

        return result;

        object?[] append()
        {
            Array.Resize(ref args, args!.Length + addArgs.Length);
            Array.Copy(addArgs, 0, args, args.Length - addArgs.Length, addArgs.Length);
            return args;
        }
    }

    public static ScopeContextLocation Location(this ScopeContext context, [CallerMemberName] string function = "", [CallerFilePath] string path = "", [CallerLineNumber] int lineNumber = 0)
    {
        return new ScopeContextLocation(context, new CodeLocation(function, path, lineNumber));
    }
}