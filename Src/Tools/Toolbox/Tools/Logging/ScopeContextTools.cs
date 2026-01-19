//namespace Toolbox.Tools;

//public static class ScopeContextTools
//{
//    public static string AppendMessage(string? message, string? addMessage) => message switch
//    {
//        null => string.Empty,
//        string v => addMessage switch
//        {
//            null => v,
//            string v2 => v + ", " + v2,
//        }
//    };

//    public static object?[] AppendArgs(object?[] args, params object[] addArgs)
//    {
//        object?[] result = (args, addArgs) switch
//        {
//            (null, { Length: 0 }) => addArgs,
//            ({ Length: 0 }, { Length: 0 }) => args,
//            _ => append(),
//        };

//        return result;

//        object?[] append()
//        {
//            Array.Resize(ref args, args!.Length + addArgs.Length);
//            Array.Copy(addArgs, 0, args, args.Length - addArgs.Length, addArgs.Length);
//            return args;
//        }
//    }

//}
