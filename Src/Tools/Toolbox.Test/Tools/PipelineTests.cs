using System;
using System.Threading.Tasks;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools;

public class PipelineTests
{
    [Fact]
    public async Task SimplePipeline()
    {
        var context = new Context();

        var pipeline = await Task.FromResult(new Context())
            .FuncAsync(x => Func1(x))
            .FuncAsync(x => Func2(x))
            .FuncAsync(x => Func3(x));

        Type t = pipeline.GetType();
    }

    private async Task<Context> Func1(Task<Context> context)
    {
        await Task.Run(() => Console.WriteLine("Func1"));

        return new Context();
    }

    private async Task<Context2> Func2(Task<Context> context)
    {
        await Task.Run(() => Console.WriteLine("Func2"));
        return new Context2();
    }

    private async Task<Context2> Func3(Task<Context2> context)
    {
        await Task.Run(() => Console.WriteLine("Func3"));
        return new Context2();
    }
}

public class Pipeline
{
    public Pipeline(object Context)
    {
        this.Context = Context;
    }

    public object Context { get; }
}

public class Context
{
}

public class Context2
{
}

public static class FunctionAsyncExtensions
{
    public static async Task<TResult> FuncAsync<T, TResult>(this T subject, Func<T, Task<TResult>> function)
    {
        subject.NotNull();
        function.NotNull();

        return await function(subject);
    }

    //public static async Task<T> ActionAsync<T>(this T subject, Func<T, Task<T>> action)
    //{
    //    subject.NotNull(nameof(subject));
    //    action.NotNull(nameof(action));

    //    await action(subject);
    //    return subject;
    //}
}
