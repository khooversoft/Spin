using System.Diagnostics;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class FunctionLogicExtensions
{
    /// <summary>
    /// Execute action if test is true
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="subject">instance of type</param>
    /// <param name="test">test function, if true, action will be executed</param>
    /// <param name="action">lambda to be executed if test is true</param>
    /// <returns>value in subject</returns>
    [DebuggerStepThrough]
    public static T IfTrue<T>(this T subject, Func<T, bool> test, Action<T> action)
    {
        test.NotNull();
        action.NotNull();

        if (test(subject)) action(subject);
        return subject;
    }

    [DebuggerStepThrough]
    public static T IfTrue<T>(this T subject, Func<T, bool> test, Action action)
    {
        test.NotNull();
        action.NotNull();

        if (test(subject)) action();
        return subject;
    }

    [DebuggerStepThrough]
    public static void IfTrue(this bool subject, Action action)
    {
        action.NotNull();
        if (subject) action();
    }

    [DebuggerStepThrough]
    public static void IfFalse(this bool subject, Action action)
    {
        action.NotNull();
        if (!subject) action();
    }

    [DebuggerStepThrough]
    public static T? IfNull<T>(this T? subject, Action action) where T : class
    {
        action.NotNull();
        if (subject == null) action();
        return subject;
    }

    [DebuggerStepThrough]
    public static T? IfNotNull<T>(this T? subject, Action<T> action) where T : class
    {
        action.NotNull();
        if (subject != null) action(subject);
        return subject;
    }

    [DebuggerStepThrough]
    public static string? IfEmpty(this string? subject, Action action)
    {
        action.NotNull();
        if (subject.IsEmpty()) action();
        return subject;
    }

    [DebuggerStepThrough]
    public static string? IfNotEmpty(this string? subject, Action<string?> action)
    {
        action.NotNull();
        if (subject.IsNotEmpty()) action(subject);
        return subject;
    }

    /// <summary>
    /// Execute async function if test is true
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="subject">instance of type</param>
    /// <param name="test">test function, if true, action will be executed</param>
    /// <param name="func">lambda to be executed if test is true</param>
    /// <returns>value in subject</returns>
    [DebuggerStepThrough]
    public static async Task<T> IfTrueAsync<T>(this Task<T> subject, Func<T, Task<bool>> test, Func<T, Task> func)
    {
        test.NotNull();
        func.NotNull();

        var subjectValue = await subject;
        bool result = await test(subjectValue);

        if (!result) await func(subjectValue);

        return subjectValue;
    }
}
