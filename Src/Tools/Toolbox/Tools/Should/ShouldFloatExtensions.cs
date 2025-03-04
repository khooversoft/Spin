namespace Toolbox.Tools.Should;

//[DebuggerStepThrough]
//public static class ShouldFloatExtensions
//{
//    public static ShouldContext<float> Should(
//        this float subject,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0,
//        [CallerArgumentExpression("subject")] string name = ""
//        )
//    {
//        return new ShouldContext<float>(subject, function, path, lineNumber, name);
//    }

//    public static ShouldContext<float?> Should(
//        this float? subject,
//        [CallerMemberName] string function = "",
//        [CallerFilePath] string path = "",
//        [CallerLineNumber] int lineNumber = 0,
//        [CallerArgumentExpression("subject")] string name = ""
//        )
//    {
//        return new ShouldContext<float?>(subject, function, path, lineNumber, name);
//    }

//    public static ShouldContext<float> Be(this ShouldContext<float> subject, float value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<float?> Be(this ShouldContext<float?> subject, float? value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<float> NotBe(this ShouldContext<float> subject, float value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }

//    public static ShouldContext<float?> NotBe(this ShouldContext<float?> subject, float? value, string? because = null)
//    {
//        if (subject.Value.Equals(value) == true) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
//        return subject;
//    }
//}
