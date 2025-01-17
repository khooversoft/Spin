using System.Runtime.CompilerServices;

namespace Toolbox.Tools.Should;

public static class ShouldEnumExtensions
{
    public static ShouldContext<TEnum> Should<TEnum>(
        this TEnum subject,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerArgumentExpression("subject")] string name = ""
        ) where TEnum : struct, Enum
    {
        return new ShouldContext<TEnum>(subject, function, path, lineNumber, name);
    }

    public static ShouldContext<TEnum> Be<TEnum>(this ShouldContext<TEnum> subject, TEnum value, string? because = null)
         where TEnum : struct, Enum
    {
        if (subject.Value.Equals(value) == false) subject.ThrowException($"Value is '{subject.Value}' but should be '{value}'", because);
        return subject;
    }
}
