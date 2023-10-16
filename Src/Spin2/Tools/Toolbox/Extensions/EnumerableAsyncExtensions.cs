using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class EnumerableAsyncExtensions
{

    /// <summary>
    /// Execute 'action' on each item
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subjects"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task ForEachAsync<T>(this IEnumerable<T> subjects, Func<T, Task> action)
    {
        subjects.NotNull();
        action.NotNull();

        foreach (var item in subjects)
        {
            await action(item);
        }
    }

    public static async Task<IEnumerable<TO>> SelectAsync<TO, T>(this IEnumerable<T> subject, Func<T, Task<TO>> func)
    {
        var response = new List<TO>();

        foreach (var item in subject)
        {
            response.Add(await func(item));
        }

        return response;
    }

    public static async Task<IEnumerable<TO>> SelectAsync<TO, T>(this Task<IEnumerable<T>> subject, Func<T, Task<TO>> func)
    {
        IEnumerable<T> list = await subject;
        var response = new List<TO>();

        foreach (var item in list)
        {
            response.Add(await func(item));
        }

        return response;
    }

    public static async Task<IEnumerable<T>> WhereAsync<T>(this Task<IEnumerable<T>> subject, Func<T, bool> func)
    {
        IEnumerable<T> list = await subject;
        var response = new List<T>();

        foreach (var item in list)
        {
            var op = func(item);
            if (op) response.Add(item);
        }

        return response;
    }

    public static async Task<IReadOnlyList<T>> ToReadOnlyListAsync<T>(this Task<IEnumerable<T>> subject)
    {
        IEnumerable<T> list = await subject;
        return list.ToArray();
    }
}
