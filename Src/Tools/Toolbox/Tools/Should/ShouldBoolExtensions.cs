//using System.Diagnostics;
//using System.Runtime.CompilerServices;

//namespace Toolbox.Tools.Should;

//[DebuggerStepThrough]
//public static class ShouldBoolExtensions
//{
//    public static ShouldContext<bool> Should(
//        this bool subject,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0,
//        [CallerArgumentExpression("subject")] string name = ""
//    )
//    {
//        return new ShouldContext<bool>(subject, function, path, lineNumber, name);
//    }

//    public static ShouldContext<bool?> Should(
//        this bool? subject,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0,
//        [CallerArgumentExpression("subject")] string name = ""
//        )
//    {
//        return new ShouldContext<bool?>(subject, function, path, lineNumber, name);
//    }

//    public static ShouldContext<bool> Be(this ShouldContext<bool> subject, bool value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<bool?> Be(this ShouldContext<bool?> subject, bool value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<bool> NotBe(this ShouldContext<bool> subject, bool value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<bool?> NotBe(this ShouldContext<bool?> subject, bool value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should not be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<bool> BeTrue(this ShouldContext<bool> subject, string? because = null)
//    {
//        if (subject.Value == false) subject.ThrowException($"Value is '{subject.Value}' should be true", because);
//        return subject;
//    }

//    public static ShouldContext<bool?> BeTrue(this ShouldContext<bool?> subject, string? because = null)
//    {
//        if (subject.Value == false) subject.ThrowException($"Value is '{subject.Value}' should be true", because);
//        return subject;
//    }

//    public static ShouldContext<bool> BeFalse(this ShouldContext<bool> subject, string? because = null)
//    {
//        if (subject.Value == true) subject.ThrowException($"Value is '{subject.Value}' should be false", because);
//        return subject;
//    }

//    public static ShouldContext<bool?> BeFalse(this ShouldContext<bool?> subject, string? because = null)
//    {
//        if (subject.Value == true) subject.ThrowException($"Value is '{subject.Value}' should be false", because);
//        return subject;
//    }
//}
