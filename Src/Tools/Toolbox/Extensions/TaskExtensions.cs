using System.Diagnostics;

namespace Toolbox.Extensions;

public static class TaskExtensions
{
    [DebuggerStepThrough]
    public static Task<T> ToTaskResult<T>(this T value) => Task.FromResult<T>(value);
}
