using System.Collections.Concurrent;
using Toolbox.Tools;

namespace Toolbox.Test;

public class AsyncKeyGuardTests
{
    [Fact]
    public async Task SinglePath()
    {
        AsyncKeyGuard keyGuard = new AsyncKeyGuard();
        string path = "key1";

        using (IDisposable scope = await keyGuard.AcquireLock(path))
        {
            keyGuard.IsLocked(path).BeTrue();
            keyGuard.IsRegistered(path).BeTrue();
        }

        keyGuard.IsLocked(path).BeFalse();
        keyGuard.IsRegistered(path).BeFalse();
    }

    [Fact]
    public async Task DifferentKeys_DoNotBlockEachOther()
    {
        AsyncKeyGuard keyGuard = new AsyncKeyGuard();
        string key1 = "key1";
        string key2 = "key2";

        TaskCompletionSource<bool> firstEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<bool> secondEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task firstTask = Task.Run(async () =>
        {
            using (await keyGuard.AcquireLock(key1))
            {
                firstEntered.SetResult(true);
                await secondEntered.Task;
            }
        });

        Task secondTask = Task.Run(async () =>
        {
            using (await keyGuard.AcquireLock(key2))
            {
                secondEntered.SetResult(true);
                await firstEntered.Task;
            }
        });

        await Task.WhenAll(firstEntered.Task, secondEntered.Task).WaitAsync(TimeSpan.FromSeconds(1));
        await Task.WhenAll(firstTask, secondTask);

        keyGuard.IsLocked(key1).BeFalse();
        keyGuard.IsLocked(key2).BeFalse();
        keyGuard.IsRegistered(key1).BeFalse();
        keyGuard.IsRegistered(key2).BeFalse();
    }

    [Fact]
    public async Task SameKey_RequestsAreSerialized()
    {
        AsyncKeyGuard keyGuard = new AsyncKeyGuard();
        string key = "key1";
        ConcurrentQueue<string> events = new ConcurrentQueue<string>();
        SemaphoreSlim releaseFirst = new SemaphoreSlim(0, 1);

        Task firstTask = Task.Run(async () =>
        {
            using (await keyGuard.AcquireLock(key))
            {
                events.Enqueue("first-enter");
                await releaseFirst.WaitAsync();
                events.Enqueue("first-exit");
            }
        });

        Task secondTask = Task.Run(async () =>
        {
            await Task.Delay(20);
            using (await keyGuard.AcquireLock(key))
            {
                events.Enqueue("second-enter");
                events.Enqueue("second-exit");
            }
        });

        await Task.Delay(50);
        events.Contains("second-enter").BeFalse();

        releaseFirst.Release();

        await Task.WhenAll(firstTask, secondTask);

        events.ToArray().SequenceEqual(new[] { "first-enter", "first-exit", "second-enter", "second-exit" }).BeTrue();
        keyGuard.IsLocked(key).BeFalse();
        keyGuard.IsRegistered(key).BeFalse();
    }

    [Fact]
    public async Task AcquireLock_WithCanceledToken_DoesNotRegisterKey()
    {
        AsyncKeyGuard keyGuard = new AsyncKeyGuard();
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(async () => await keyGuard.AcquireLock("canceled", cancellationTokenSource.Token));

        keyGuard.IsRegistered("canceled").BeFalse();
        keyGuard.IsLocked("canceled").BeFalse();
    }

    [Fact]
    public async Task AcquireLock_MultipleWaiters_RemovesRegistrationAfterLastRelease()
    {
        AsyncKeyGuard keyGuard = new AsyncKeyGuard();
        string key = "shared";

        IDisposable firstScope = await keyGuard.AcquireLock(key);
        Task<IDisposable> secondScopeTask = keyGuard.AcquireLock(key);

        await Task.Delay(30);
        secondScopeTask.IsCompleted.BeFalse();

        firstScope.Dispose();

        using (IDisposable secondScope = await secondScopeTask)
        {
            keyGuard.IsRegistered(key).BeTrue();
            keyGuard.IsLocked(key).BeTrue();
        }

        keyGuard.IsRegistered(key).BeFalse();
        keyGuard.IsLocked(key).BeFalse();
    }

    [Fact]
    public async Task Stress_ManyConcurrentAcquisitions_DoNotLeakRegistrations()
    {
        AsyncKeyGuard keyGuard = new AsyncKeyGuard();
        string key = "stress";
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < 25; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                using (await keyGuard.AcquireLock(key))
                {
                    await Task.Delay(5);
                }
            }));
        }

        await Task.WhenAll(tasks);

        keyGuard.IsRegistered(key).BeFalse();
        keyGuard.IsLocked(key).BeFalse();
    }
}
