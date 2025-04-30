//using System.Diagnostics;
//using System.Runtime.CompilerServices;

//namespace Toolbox.Tools.Should;

//[DebuggerStepThrough]
//public static class ShouldIntExtensions
//{
//    public static ShouldContext<int> Should(
//        this int subject,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0,
//        [CallerArgumentExpression("subject")] string name = ""
//        )
//    {
//        return new ShouldContext<int>(subject, function, path, lineNumber, name);
//    }

//    public static ShouldContext<int?> Should(
//        this int? subject,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0,
//        [CallerArgumentExpression("subject")] string name = ""
//        )
//    {
//        return new ShouldContext<int?>(subject, function, path, lineNumber, name);
//    }

//    public static ShouldContext<int> Be(this ShouldContext<int> subject, int value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<int?> Be(this ShouldContext<int?> subject, int? value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<int> NotBe(this ShouldContext<int> subject, int value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<int?> NotBe(this ShouldContext<int?> subject, int? value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<int?> BeNull(this ShouldContext<int?> subject, string? because = null)
//    {
//        if (subject.Value != null) subject.ThrowException($"Value is not null'", because);
//        return subject;
//    }

//    public static ShouldContext<int?> NotBeNull(this ShouldContext<int?> subject, string? because = null)
//    {
//        if (subject.Value == null) subject.ThrowException($"Value is not null'", because);
//        return subject;
//    }
//}
