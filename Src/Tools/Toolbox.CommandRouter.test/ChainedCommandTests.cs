using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.CommandRouter.test;
public class ChainedCommandTests
{
    private readonly ITestOutputHelper _output;
    public ChainedCommandTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public async Task ChainCommands()
    {
        var host = new CommandRouterBuilder()
            .SetArgs("run")
            .ConfigureAppConfiguration((config, service) => service.AddLogging(builder => builder.AddLambda(x => _output.WriteLine(x))))
            .ConfigureService(service => service.AddSingleton<ConcurrentQueue<string>>())
            .AddCommand<RunCommand>()
            .AddCommand<CreateCommand>()
            .AddCommand<FinishCommand>()
            .Build();

        int state = await host.Run();
        state.Be(0);

        ConcurrentQueue<string> q = host.Service.GetRequiredService<ConcurrentQueue<string>>();
        q.Count.Be(3);
        q.TryDequeue(out string? result).BeTrue();
        result.Be("Run executed");

        q.TryDequeue(out result).BeTrue();
        result.Be("Created, workId=work#123");

        q.TryDequeue(out result).BeTrue();
        result.Be("Finished");
    }


    private class RunCommand : ICommandRoute
    {
        private readonly ICommandRouterHost _host;
        private readonly ConcurrentQueue<string> _queue;
        private readonly ILogger<RunCommand> _logger;

        public RunCommand(ICommandRouterHost host, ConcurrentQueue<string> queue, ILogger<RunCommand> logger)
        {
            _host = host;
            _queue = queue;
            _logger = logger;
        }

        public CommandSymbol CommandSymbol() => new CommandSymbol("run", "Create contract").Action(x =>
        {
            x.SetHandler(Run);
        });

        private Task Run()
        {
            _logger.LogInformation("RunCommand - executing run");
            _queue.Enqueue("Run executed");
            _host.Enqueue("create", "--workId", "work#123");
            return Task.CompletedTask;
        }
    }

    private class CreateCommand : ICommandRoute
    {
        private readonly ICommandRouterHost _host;
        private readonly ConcurrentQueue<string> _queue;
        private readonly ILogger<RunCommand> _logger;

        public CreateCommand(ICommandRouterHost host, ConcurrentQueue<string> queue, ILogger<RunCommand> logger)
        {
            _host = host;
            _queue = queue;
            _logger = logger;
        }

        public CommandSymbol CommandSymbol() => new CommandSymbol("create", "Create contract").Action(x =>
        {
            var workId = x.AddOption<string>("--workId", "WorkId to execute");
            x.SetHandler(Run, workId);
        });

        private Task Run(string workId)
        {
            _logger.LogInformation("CreateCommand - executing create, workId={workId}", workId);

            switch (workId)
            {
                case string: _queue.Enqueue($"Created, workId={workId}"); break;
                default: _queue.Enqueue("Created"); break;
            }

            _host.Enqueue("finish");


            return Task.CompletedTask;
        }
    }

    private class FinishCommand : ICommandRoute
    {
        private readonly ConcurrentQueue<string> _queue;
        private readonly ILogger<RunCommand> _logger;

        public FinishCommand(ConcurrentQueue<string> queue, ILogger<RunCommand> logger)
        {
            _queue = queue;
            _logger = logger;
        }

        public CommandSymbol CommandSymbol() => new CommandSymbol("finish", "Create contract").Action(x =>
        {
            x.SetHandler(Finish);
        });

        private Task Finish()
        {
            _logger.LogInformation("Finished");
            _queue.Enqueue("Finished");
            return Task.CompletedTask;
        }
    }
}
