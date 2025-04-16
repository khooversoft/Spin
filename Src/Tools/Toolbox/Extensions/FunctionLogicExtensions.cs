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
    public static void IfTrue(this bool subject, Action action)
    {
        action.NotNull();
        if (subject) action();
    }

    [DebuggerStepThrough]
    public static T IfElse<T>(this T subject, Func<T, bool> test, Action<T> trueAction, Action<T> falseAction)
    {
        test.NotNull();
        trueAction.NotNull();
        falseAction.NotNull();

        if (test(subject)) trueAction(subject); else falseAction(subject);
        return subject;
    }

    [DebuggerStepThrough]
    public static void IfElse(this bool subject, Action trueAction, Action falseAction)
    {
        trueAction.NotNull();
        falseAction.NotNull();

        if (subject) trueAction(); else falseAction();
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
