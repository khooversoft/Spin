using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Xunit.Abstractions;

namespace Toolbox.Test.Tools;

public class OperationQueueTaskProcessorTests
{
    private ITestOutputHelper _testOutputHelper;
    private IHost _host;
    private ILogger _logger;

    public OperationQueueTaskProcessorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(config => config.AddLambda(_testOutputHelper.WriteLine).AddDebug().AddFilter(x => true));
            })
            .Build();

        _logger = _host.Services.GetRequiredService<ILogger<OperationQueueTests>>();
    }

    [Fact]
    public async Task TasksProcessorNoFailures()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);
        const int taskCount = 10;
        var dict = new ConcurrentDictionary<int, int>();

        var tasks = Enumerable.Range(1, taskCount)
            .Select(i => new TaskInvoke(i, async () =>
            {
                dict.AddOrUpdate(i, 1, (_, count) => count + 1);
                if (dict[i] > 2) return true;

                var rndValue = RandomNumberGenerator.GetInt32(0, 100);
                return rndValue % 2 == 0;
            }))
            .ToArray();

        var results = await processor.Run(tasks);
        results.All(x => x.State == TaskState.Succeeded).BeTrue();
    }

    [Fact]
    public async Task TasksProcessorWithAbandonedTasks()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);
        const int taskCount = 5;

        var tasks = Enumerable.Range(1, taskCount)
            .Select(i => new TaskInvoke(i, async () =>
            {
                await Task.Delay(10);
                return false; // Always fail
            }))
            .ToArray();

        var results = await processor.Run(tasks);

        results.Count.Be(taskCount);
        results.All(x => x.State == TaskState.Abandoned).BeTrue();
        results.All(x => x.AttemptCount == 3).BeTrue();
    }

    [Fact]
    public async Task TasksProcessorMixedSuccessAndAbandoned()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);
        var dict = new ConcurrentDictionary<int, int>();

        var tasks = Enumerable.Range(1, 10)
            .Select(i => new TaskInvoke(i, async () =>
            {
                await Task.Delay(10);

                // Tasks with even IDs succeed on second attempt, odd IDs always fail
                if (i % 2 == 0)
                {
                    dict.AddOrUpdate(i, 1, (_, count) => count + 1);
                    return dict[i] >= 2;
                }

                return false;
            }))
            .ToArray();

        var results = await processor.Run(tasks);

        results.Count.Be(10);
        var succeeded = results.Where(x => x.State == TaskState.Succeeded).ToArray();
        var abandoned = results.Where(x => x.State == TaskState.Abandoned).ToArray();

        succeeded.Length.Be(5);
        abandoned.Length.Be(5);
        succeeded.All(x => x.Id % 2 == 0).BeTrue();
        abandoned.All(x => x.Id % 2 == 1).BeTrue();
    }

    [Fact]
    public async Task TasksProcessorThrowsOnNullFunctions()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            processor.Run(null!));
    }

    [Fact]
    public async Task TasksProcessorThrowsOnEmptyFunctions()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            processor.Run(Array.Empty<TaskInvoke>()));
    }

    [Fact]
    public async Task TasksProcessorSingleTaskSuccess()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);

        var tasks = new[]
        {
            new TaskInvoke(1, async () =>
            {
                await Task.Delay(10);
                return true;
            })
        };

        var results = await processor.Run(tasks);

        results.Count.Be(1);
        results[0].State.Be(TaskState.Succeeded);
        results[0].AttemptCount.Be(1);
    }

    [Fact]
    public async Task TasksProcessorSingleTaskFailure()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);

        var tasks = new[]
        {
            new TaskInvoke(1, async () =>
            {
                await Task.Delay(10);
                return false;
            })
        };

        var results = await processor.Run(tasks);

        results.Count.Be(1);
        results[0].State.Be(TaskState.Abandoned);
        results[0].AttemptCount.Be(3);
    }

    [Fact]
    public async Task TasksProcessorVerifyAttemptCounts()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 5);
        var dict = new ConcurrentDictionary<int, int>();

        var tasks = Enumerable.Range(1, 5)
            .Select(i => new TaskInvoke(i, async () =>
            {
                await Task.Delay(10);
                dict.AddOrUpdate(i, 1, (_, count) => count + 1);

                // Each task succeeds after specific number of attempts
                return dict[i] >= i;
            }))
            .ToArray();

        var results = await processor.Run(tasks);

        results.Count.Be(5);
        results.All(x => x.State == TaskState.Succeeded).BeTrue();

        results.First(x => x.Id == 1).AttemptCount.Be(1);
        results.First(x => x.Id == 2).AttemptCount.Be(2);
        results.First(x => x.Id == 3).AttemptCount.Be(3);
        results.First(x => x.Id == 4).AttemptCount.Be(4);
        results.First(x => x.Id == 5).AttemptCount.Be(5);
    }

    [Fact]
    public async Task TasksProcessorVerifyStateTransitions()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);
        var stateHistory = new ConcurrentBag<(int Id, TaskState State)>();

        var tasks = new[]
        {
            new TaskInvoke(1, async () =>
            {
                await Task.Delay(10);
                return true;
            })
        };

        // Capture initial state
        tasks[0].State.Be(TaskState.NotRun);
        tasks[0].AttemptCount.Be(0);

        var results = await processor.Run(tasks);

        // Verify final state
        results[0].State.Be(TaskState.Succeeded);
        results[0].AttemptCount.Be(1);
    }

    [Fact]
    public async Task TasksProcessorAllTasksImmediateSuccess()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);
        const int taskCount = 20;

        var tasks = Enumerable.Range(1, taskCount)
            .Select(i => new TaskInvoke(i, async () =>
            {
                await Task.Delay(5);
                return true;
            }))
            .ToArray();

        var results = await processor.Run(tasks);

        results.Count.Be(taskCount);
        results.All(x => x.State == TaskState.Succeeded).BeTrue();
        results.All(x => x.AttemptCount == 1).BeTrue();
    }

    [Fact]
    public async Task TasksProcessorPartialRetriesBeforeSuccess()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 4);
        var dict = new ConcurrentDictionary<int, int>();

        var tasks = Enumerable.Range(1, 10)
            .Select(i => new TaskInvoke(i, async () =>
            {
                await Task.Delay(10);
                dict.AddOrUpdate(i, 1, (_, count) => count + 1);

                // First 5 tasks succeed on 2nd attempt, rest on 3rd attempt
                int requiredAttempts = i <= 5 ? 2 : 3;
                return dict[i] >= requiredAttempts;
            }))
            .ToArray();

        var results = await processor.Run(tasks);

        results.Count.Be(10);
        results.All(x => x.State == TaskState.Succeeded).BeTrue();

        var firstBatch = results.Where(x => x.Id <= 5).ToArray();
        var secondBatch = results.Where(x => x.Id > 5).ToArray();

        firstBatch.All(x => x.AttemptCount == 2).BeTrue();
        secondBatch.All(x => x.AttemptCount == 3).BeTrue();
    }

    [Fact]
    public async Task TasksProcessorExactlyAtRetryLimit()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 3);
        var dict = new ConcurrentDictionary<int, int>();

        var tasks = new[]
        {
            new TaskInvoke(1, async () =>
            {
                await Task.Delay(10);
                dict.AddOrUpdate(1, 1, (_, count) => count + 1);
                
                // Succeed exactly on the 3rd attempt (maxRetries)
                return dict[1] >= 3;
            })
        };

        var results = await processor.Run(tasks);

        results.Count.Be(1);
        results[0].State.Be(TaskState.Succeeded);
        results[0].AttemptCount.Be(3);
    }

    [Fact]
    public async Task TasksProcessorZeroMaxRetries()
    {
        var processor = ActivatorUtilities.CreateInstance<TasksProcessor>(_host.Services, 0);

        var tasks = new[]
        {
            new TaskInvoke(1, async () =>
            {
                await Task.Delay(10);
                return false;
            })
        };

        var results = await processor.Run(tasks);

        results.Count.Be(1);
        results[0].State.Be(TaskState.Abandoned);
        results[0].AttemptCount.Be(1);
    }
}

public class TasksProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly int _maxRetries;
    private readonly ILogger<TasksProcessor> _logger;

    public TasksProcessor(IServiceProvider serviceProvider, int maxRetries, ILogger<TasksProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _maxRetries = maxRetries;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskInvoke>> Run(IReadOnlyList<TaskInvoke> functions)
    {
        if (functions == null) throw new ArgumentNullException(nameof(functions));
        if (functions.Count == 0) throw new ArgumentException($"At least one function is required", nameof(functions));

        var dict = new ConcurrentDictionary<int, TaskInvoke>();
        await using var operationQueue = ActivatorUtilities.CreateInstance<SequentialAsyncQueue>(_serviceProvider, 100);

        await seedFunctions();

        int maxPass = _maxRetries + 1;
        int maxDelayMs = 1000;

        while (maxPass-- > 0)
        {
            var getStats = await operationQueue.Get<bool>(async () =>
            {
                var retries = dict.Values.Where(x => x.State == TaskState.Failed).ToArray();
                if (retries.Length == 0) return true;

                foreach (var func in retries)
                {
                    if (func.AttemptCount >= _maxRetries)
                    {
                        dict[func.Id] = func with { State = TaskState.Abandoned };
                        continue;
                    }

                    await operationQueue.Send(async () =>
                    {
                        var success = await func.Function();
                        func.State = success ? TaskState.Succeeded : TaskState.Failed;
                        func.AttemptCount++;
                    });
                }

                await Task.Delay(RandomNumberGenerator.GetInt32(100, maxDelayMs));
                return false;
            });

            if (getStats) break;
        }

        await operationQueue.Drain();
        await operationQueue.Complete();

        return dict.Values.ToArray();

        async Task seedFunctions()
        {
            foreach (var func in functions)
            {
                func.State = TaskState.Running;
                func.AttemptCount = 0;
                dict[func.Id] = func;

                await operationQueue.Send(async () =>
                {
                    var success = await func.Function();
                    func.State = success ? TaskState.Succeeded : TaskState.Failed;
                    func.AttemptCount++;
                });
            }
        }
    }
}

public enum TaskState
{
    NotRun,
    Running,
    Succeeded,
    Failed,
    Abandoned
}

public record TaskInvoke
{
    public TaskInvoke(int id, Func<Task<bool>> function)
    {
        Id = id;
        Function = function.NotNull();
    }

    public int Id { get; }
    public Func<Task<bool>> Function { get; }
    public TaskState State { get; set; } = TaskState.NotRun;
    public int AttemptCount { get; set; }
}