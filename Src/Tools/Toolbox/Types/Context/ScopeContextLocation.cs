//using System.Runtime.CompilerServices;

//namespace Toolbox.Types;

//public readonly record struct ScopeContextLocation : ILoggingContext
//{
//    public ScopeContextLocation(ScopeContext context, CodeLocation location)
//    {
//        Context = context;
//        Location = location;
//    }

//    public ScopeContext Context { get; init; }
//    public CodeLocation Location { get; }
//}


//public static class ScopeContextLocationExtensions
//{
//    public static ScopeContextLocation Location(
//        this ScopeContext context,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0
//        )
//    {
//        return new ScopeContextLocation(context, new CodeLocation(function, path, lineNumber));
//    }
//}