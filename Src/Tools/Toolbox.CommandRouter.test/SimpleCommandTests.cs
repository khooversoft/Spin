using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.CommandRouter.test;

public class SimpleCommandTests
{
    private readonly ITestOutputHelper _output;
    public SimpleCommandTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task SimpleCommand()
    {
        var host = new CommandRouterTestHost()
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .Build();

        var context = host.ServiceProvider.GetRequiredService<ILogger<SimpleCommandTests>>().ToScopeContext();
        var state = await host.Command.Run(context, "run");
        state.BeOk();

        ConcurrentQueue<string> q = host.ServiceProvider.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(1);
        q.TryDequeue(out string? result).BeTrue();
        result.Be("Run executed");
    }

    [Fact]
    public async Task NotValidCommandShouldFail()
    {
        var host = new CommandRouterTestHost()
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .Build();

        var context = host.ServiceProvider.GetRequiredService<ILogger<SimpleCommandTests>>().ToScopeContext();
        var state = await host.Command.Run(context, "runxxx");
        state.BeError();

        ConcurrentQueue<string> q = host.ServiceProvider.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(0);
    }

    [Fact]
    public async Task SimpleMultipleCommand()
    {
        var host = new CommandRouterTestHost()
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOption>()
            .Build();

        var context = host.ServiceProvider.GetRequiredService<ILogger<SimpleCommandTests>>().ToScopeContext();
        var state = await host.Command.Run(context, "run");
        state.BeOk();

        ConcurrentQueue<string> q = host.ServiceProvider.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(1);
        q.TryDequeue(out string? result).BeTrue();
        result.Be("Run executed");
    }

    [Fact]
    public async Task SimpleWithNoOptionCommand()
    {
        var host = new CommandRouterTestHost()
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOption>()
            .Build();

        var context = host.ServiceProvider.GetRequiredService<ILogger<SimpleCommandTests>>().ToScopeContext();
        var state = await host.Command.Run(context, "create");
        state.BeOk();

        ConcurrentQueue<string> q = host.ServiceProvider.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(1);
        q.TryDequeue(out string? result).BeTrue();
        result.Be("Created");
    }

    [Fact]
    public async Task SimpleWithOptionCommand()
    {
        var host = new CommandRouterTestHost()
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOption>()
            .Build();

        var context = host.ServiceProvider.GetRequiredService<ILogger<SimpleCommandTests>>().ToScopeContext();
        var state = await host.Command.Run(context, "create", "--workId", "work#123");
        state.BeOk();

        ConcurrentQueue<string> q = host.ServiceProvider.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(1);
        q.TryDequeue(out string? result).BeTrue();
        result.Be("Created, workId=work#123");
    }

    [Fact]
    public async Task SimpleWithRequiredOptionCommand()
    {
        var host = new CommandRouterTestHost()
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOptionRequired>()
            .Build();

        var context = host.ServiceProvider.GetRequiredService<ILogger<SimpleCommandTests>>().ToScopeContext();
        var state = await host.Command.Run(context, "create", "--workId", "work#123");
        state.BeOk();

        ConcurrentQueue<string> q = host.ServiceProvider.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(1);
        q.TryDequeue(out string? result).BeTrue();
        result.Be("Created, workId=work#123");
    }

    [Fact]
    public async Task SimpleWithRequiredOptionCommandShouldFail()
    {
        var host = new CommandRouterTestHost()
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOptionRequired>()
            .Build();

        var context = host.ServiceProvider.GetRequiredService<ILogger<SimpleCommandTests>>().ToScopeContext();
        var state = await host.Command.Run(context, "create");
        state.BeError();

        ConcurrentQueue<string> q = host.ServiceProvider.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(0);
    }

    private class CmdA : ICommandRoute
    {
        private readonly ConcurrentQueue<string> _queue;
        public CmdA(ConcurrentQueue<string> queue) => _queue = queue;

        public CommandSymbol CommandSymbol() => new CommandSymbol("run", "Create contract").Action(x =>
        {
            x.SetHandler(Run);
        });

        private Task Run()
        {
            _queue.Enqueue("Run executed");
            return Task.CompletedTask;
        }
    }

    private class CmdBOption : ICommandRoute
    {
        private readonly ConcurrentQueue<string> _queue;
        public CmdBOption(ConcurrentQueue<string> queue) => _queue = queue;

        public CommandSymbol CommandSymbol() => new CommandSymbol("create", "Create contract").Action(x =>
        {
            var workId = x.AddOption<string>("--workId", "WorkId to execute");
            x.SetHandler(Run, workId);
        });

        private Task Run(string workId)
        {
            switch (workId)
            {
                case string: _queue.Enqueue($"Created, workId={workId}"); break;
                default: _queue.Enqueue("Created"); break;
            }

            return Task.CompletedTask;
        }
    }

    private class CmdBOptionRequired : ICommandRoute
    {
        private readonly ConcurrentQueue<string> _queue;
        public CmdBOptionRequired(ConcurrentQueue<string> queue) => _queue = queue;

        public CommandSymbol CommandSymbol() => new CommandSymbol("create", "Create contract").Action(x =>
        {
            var workId = x.AddOption<string>("--workId", "WorkId to execute");
            workId.IsRequired = true;
            x.SetHandler(Run, workId);
        });

        private Task Run(string workId)
        {
            switch (workId)
            {
                case string: _queue.Enqueue($"Created, workId={workId}"); break;
                default: _queue.Enqueue("Created"); break;
            }

            return Task.CompletedTask;
        }
    }
}