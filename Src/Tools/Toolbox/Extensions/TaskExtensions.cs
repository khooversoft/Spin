namespace Toolbox.Extensions;

public static class TaskExtensions
{
    public static Task<T> ToTaskResult<T>(this T value) => Task.FromResult<T>(value);
}
