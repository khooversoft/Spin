using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
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
        int x = 0;
        AsyncReaderWriterLock rwLock = new AsyncReaderWriterLock();

        // Writer Task
        var writerTask = Task.Run(async () =>
        {
            using (var release = await rwLock.WriterLockAsync())
            {
                x = 5;
            }
        });

        // Reader Task
        var readerTask = Task.Run(async () =>
        {
            using (var release = await rwLock.ReaderLockAsync())
            {
                x.Should().Be(5);
            }
        });

        await Task.WhenAll(writerTask, readerTask);
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
            current.Should().Be(0);
            using (var releaser = await rwLock.WriterLockAsync())
            {
                count++;
                currentCount = count;
            }
            var last = Interlocked.Decrement(ref writerCount);
            last.Should().Be(1);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        var readerBlock = new ActionBlock<int>(async x =>
        {
            writerCount.Should().Be(0);
            using (var releaser = await rwLock.ReaderLockAsync())
            {
                writerCount.Should().Be(0);

                int read = count;
                int cCount = currentCount;
                read.Should().Be(cCount);
            }
            writerCount.Should().Be(0);

        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        int range = 1000;
        var tasks = Enumerable.Range(0, range).Select(x => x % 2 == 0 ? writerBlock.SendAsync(x) : readerBlock.SendAsync(x)).ToArray();
        await Task.WhenAll(tasks);
    }
}
