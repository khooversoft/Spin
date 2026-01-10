using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class ReaderLockAsyncTests
{
    [Fact]
    public async Task ReaderLockAsyncTest()
    {
        AsyncReaderWriterLock asyncReaderWriterLock = new AsyncReaderWriterLock();
        Task<AsyncReaderWriterLock.Releaser> task = asyncReaderWriterLock.ReaderLockAsync();

        Assert.True(task.IsCompleted);

        using (AsyncReaderWriterLock.Releaser releaser = await task)
        {
            releaser.NotNull();
        }
    }

    [Fact]
    public async Task WriterLockAsyncTest()
    {
        AsyncReaderWriterLock asyncReaderWriterLock = new AsyncReaderWriterLock();
        Task<AsyncReaderWriterLock.Releaser> task = asyncReaderWriterLock.ReaderLockAsync();

        Assert.True(task.IsCompleted);

        using (AsyncReaderWriterLock.Releaser releaser = await task)
        {
            releaser.NotNull();
        }
    }

    [Fact]
    public async Task ReaderLockAsyncTest_WithWriter()
    {
        int result = await RunScenarioAsync();
        result.Be(5);

        static async Task<int> RunScenarioAsync()
        {
            int value = 0;
            int readValue = 0;
            AsyncReaderWriterLock rwLock = new AsyncReaderWriterLock();
            TaskCompletionSource<bool> writerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            Task writerTask = Task.Run(async () =>
            {
                using (AsyncReaderWriterLock.Releaser release = await rwLock.WriterLockAsync())
                {
                    writerEntered.SetResult(true);
                    value = 5;
                    await Task.Delay(10);
                }
            });

            Task readerTask = Task.Run(async () =>
            {
                await writerEntered.Task;
                using (AsyncReaderWriterLock.Releaser release = await rwLock.ReaderLockAsync())
                {
                    readValue = value;
                }
            });

            await Task.WhenAll(writerTask, readerTask);
            return readValue;
        }
    }

    [Fact]
    public async Task StressReaderWriter()
    {
        int count = 0;
        int writerCount = 0;
        int currentCount = count;
        AsyncReaderWriterLock rwLock = new AsyncReaderWriterLock();

        var writerBlock = new ActionBlock<int>(async x =>
        {
            int current = Interlocked.Increment(ref writerCount);
            current.Be(0);
            using (var releaser = await rwLock.WriterLockAsync())
            {
                count++;
                currentCount = count;
            }
            var last = Interlocked.Decrement(ref writerCount);
            last.Be(1);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        var readerBlock = new ActionBlock<int>(async x =>
        {
            writerCount.Be(0);
            using (var releaser = await rwLock.ReaderLockAsync())
            {
                writerCount.Be(0);

                int read = count;
                int cCount = currentCount;
                read.Be(cCount);
            }
            writerCount.Be(0);

        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        int range = 1000;
        var tasks = Enumerable.Range(0, range).Select(x => x % 2 == 0 ? writerBlock.SendAsync(x) : readerBlock.SendAsync(x)).ToArray();
        await Task.WhenAll(tasks);
    }
}
