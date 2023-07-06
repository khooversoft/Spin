using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class FunctionExtensions
{
    /// <summary>
    /// Execute function
    /// </summary>
    /// <typeparam name="T">subject type</typeparam>
    /// <typeparam name="TResult">return type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="function">lambda execute</param>
    /// <returns>return from lambda</returns>
    public static TResult Func<T, TResult>(this T subject, Func<T, TResult> function) => function.NotNull()(subject);

    /// <summary>
    /// Exceute Async function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="subject"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public static Task<TResult> FuncAsync<T, TResult>(this T subject, Func<T, Task<TResult>> function) => function.NotNull()(subject);


    /// <summary>
    /// Execute action
    /// </summary>
    /// <typeparam name="T">any type</typeparam>
    /// <param name="subject">subject</param>
    /// <param name="action">action</param>
    /// <returns>subject</returns>
    public static T Action<T>(this T subject, Action<T> action)
    {
        action.NotNull();

        action(subject);
        return subject;
    }
}
