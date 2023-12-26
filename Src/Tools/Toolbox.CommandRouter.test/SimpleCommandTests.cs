using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Logging;
using Xunit.Abstractions;

namespace Toolbox.CommandRouter.test;

public class SimpleCommandTests
{
    private readonly ITestOutputHelper _output;
    public SimpleCommandTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task SimpleCommand()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("run")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .Build();

        int state = await host.Run();
        state.Should().Be(0);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Should().Be(1);
        q.TryDequeue(out string? result).Should().BeTrue();
        result.Should().Be("Run executed");
    }

    [Fact]
    public async Task NotValidCommandShouldFail()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("runxxx")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .Build();

        int state = await host.Run();
        state.Should().Be(1);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Should().Be(0);
    }

    [Fact]
    public async Task SimpleMultipleCommand()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("run")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOption>()
            .Build();

        int state = await host.Run();
        state.Should().Be(0);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Should().Be(1);
        q.TryDequeue(out string? result).Should().BeTrue();
        result.Should().Be("Run executed");
    }

    [Fact]
    public async Task SimpleWithNoOptionCommand()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("create")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOption>()
            .Build();

        int state = await host.Run();
        state.Should().Be(0);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Should().Be(1);
        q.TryDequeue(out string? result).Should().BeTrue();
        result.Should().Be("Created");
    }

    [Fact]
    public async Task SimpleWithOptionCommand()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("create", "--workId", "work#123")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOption>()
            .Build();

        int state = await host.Run();
        state.Should().Be(0);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Should().Be(1);
        q.TryDequeue(out string? result).Should().BeTrue();
        result.Should().Be("Created, workId=work#123");
    }

    [Fact]
    public async Task SimpleWithRequiredOptionCommand()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("create", "--workId", "work#123")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOptionRequired>()
            .Build();

        int state = await host.Run();
        state.Should().Be(0);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Should().Be(1);
        q.TryDequeue(out string? result).Should().BeTrue();
        result.Should().Be("Created, workId=work#123");
    }

    [Fact]
    public async Task SimpleWithRequiredOptionCommandShouldFail()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("create")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<CmdA>()
            .AddCommand<CmdBOptionRequired>()
            .Build();

        int state = await host.Run();
        state.Should().Be(1);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Should().Be(0);
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