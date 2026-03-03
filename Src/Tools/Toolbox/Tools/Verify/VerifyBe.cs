using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Toolbox.Tools;

public static class VerifyBe
{
    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(subject))]
    public static T? Be<T>(this T? subject, T? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
        where T : class
    {
        if (subject is Array subjectArray && value is Array valueArray && subject.GetType() == value.GetType())
        {
            if (!subjectArray.Cast<object?>().SequenceEqual(valueArray.Cast<object?>()))
                throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));

            return subject;
        }

        if (subject is IEnumerable subjectEnumerable && value is IEnumerable valueEnumerable && subject.GetType() == value.GetType())
        {
            if (!subjectEnumerable.Cast<object?>().SequenceEqual(valueEnumerable.Cast<object?>()))
                throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));

            return subject;
        }

        if (!EqualityComparer<T?>.Default.Equals(subject, value))
            throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should be '{value}'", because));

        return subject;
    }

    [DebuggerStepThrough]
    [return: NotNullIfNotNull(nameof(subject))]
    public static T? NotBe<T>(this T? subject, T? value, string? because = null, [CallerArgumentExpression("subject")] string name = "")
        where T : class
    {
        if (subject is Array subjectArray && value is Array valueArray && subject.GetType() == value.GetType())
        {
            if (subjectArray.Cast<object?>().SequenceEqual(valueArray.Cast<object?>()))
                throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should NOT be '{value}'", because));

            return subject;
        }

        if (subject is IEnumerable subjectEnumerable && value is IEnumerable valueEnumerable && subject.GetType() == value.GetType())
        {
            if (subjectEnumerable.Cast<object?>().SequenceEqual(valueEnumerable.Cast<object?>()))
                throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should NOT be '{value}'", because));

            return subject;
        }

        if (EqualityComparer<T?>.Default.Equals(subject, value))
            throw new ArgumentException(Verify.FormatException($"Value is '{subject}', should NOT be '{value}'", because));

        return subject;
    }
}
