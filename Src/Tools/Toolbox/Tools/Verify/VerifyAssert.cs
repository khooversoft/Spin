using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class VerifyAssert
{
    /// <summary>
    /// Verify state
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="test">test func</param>
    /// <param name="message">message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    public static T Assert<T>(
            this T subject,
            Func<T, bool> test,
            string? message = null,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        if (test(subject)) return subject;
        var location = new CodeLocation(function, path, lineNumber, name);

        var structLine = new StructureLineBuilder()
            .Add(message)
            .Add(location)
            .Build()
            .Format();

        throw new ArgumentException(structLine);
    }

    /// <summary>
    /// Verify state
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="test">test func</param>
    /// <param name="getMessage">get message</param>
    /// <returns>subject</returns>
    [DebuggerStepThrough]
    public static T Assert<T>(
            this T subject,
            Func<T, bool> test,
            Func<T, string?> getMessage,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0,
            [CallerArgumentExpression("subject")] string name = ""
        )
    {
        if (test(subject)) return subject;
        getMessage.NotNull();
        var location = new CodeLocation(function, path, lineNumber, name);

        var structLine = new StructureLineBuilder()
            .Add(getMessage(subject))
            .Add(location)
            .Build()
            .Format();

        throw new ArgumentException(structLine);
    }
}
