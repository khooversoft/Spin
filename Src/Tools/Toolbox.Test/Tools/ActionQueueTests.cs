using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class ActionQueueTests
{
    [Fact]
    public void ActionQueue_WithSyncAction_ShouldCreate()
    {
        // Arrange & Act
        var queue = new ActionBlock2<int>(x => { }, maxQueue: 10, maxWorkers: 1);

        // Assert
        queue.NotNull();
        queue.InputCount.Be(0);
        queue.IsCompleted.BeFalse();
    }

    [Fact]
    public void ActionQueue_WithAsyncAction_ShouldCreate()
    {
        // Arrange & Act
        var queue = new ActionBlock2<int>(async x => await Task.CompletedTask, maxQueue: 10, maxWorkers: 1);

        // Assert
        queue.NotNull();
        queue.InputCount.Be(0);
        queue.IsCompleted.BeFalse();
    }

    [Fact]
    public void ActionQueue_WithNullAction_ShouldThrow()
    {
        // Arrange
        Action<int> nullAction = null!;

        // Act & Assert
        Verify.Throws<ArgumentNullException>(() => new ActionBlock2<int>(nullAction));
    }

    [Fact]
    public void ActionQueue_WithNullAsyncAction_ShouldThrow()
    {
        // Arrange
        Func<int, Task> nullAction = null!;

        // Act & Assert
        Verify.Throws<ArgumentNullException>(() => new ActionBlock2<int>(nullAction));
    }

    [Fact]
    public void ActionQueue_WithInvalidMaxQueue_ShouldThrow()
    {
        // Act & Assert
        Verify.Throws<ArgumentException>(() => new ActionBlock2<int>(x => { }, maxQueue: 0, maxWorkers: 1));
    }

    [Fact]
    public void ActionQueue_WithNegativeMaxQueue_ShouldThrow()
    {
        // Act & Assert
        Verify.Throws<ArgumentException>(() => new ActionBlock2<int>(x => { }, maxQueue: -1, maxWorkers: 1));
    }

    [Fact]
    public void ActionQueue_WithInvalidMaxWorkers_ShouldThrow()
    {
        // Act & Assert
        Verify.Throws<ArgumentException>(() => new ActionBlock2<int>(x => { }, maxQueue: 10, maxWorkers: 0));
    }

    [Fact]
    public void ActionQueue_WithNegativeMaxWorkers_ShouldThrow()
    {
        // Act & Assert
        Verify.Throws<ArgumentException>(() => new ActionBlock2<int>(x => { }, maxQueue: 10, maxWorkers: -1));
    }

    [Fact]
    public async Task Post_SingleItem_ShouldProcessSuccessfully()
    {
        // Arrange
        int processedValue = 0;
        var queue = new ActionBlock2<int>(x => processedValue = x);

        // Act
        bool result = queue.Post(42);
        await queue.CloseAsync();

        // Assert
        result.BeTrue();
        processedValue.Be(42);
    }

    [Fact]
    public async Task Post_MultipleItems_ShouldProcessSuccessfully()
    {
        // Arrange
        var processedValues = new List<int>();
        var queue = new ActionBlock2<int>(x => processedValues.Add(x));
        var items = new[] { 1, 2, 3, 4, 5 };

        // Act
        bool result = queue.Post(items);
        await queue.CloseAsync();

        // Assert
        result.BeTrue();
        processedValues.BeEquivalent(items);
    }

    [Fact]
    public void Post_NullItems_ShouldThrow()
    {
        // Arrange
        var queue = new ActionBlock2<int>(x => { });
        IEnumerable<int> nullItems = null!;

        // Act & Assert
        Verify.Throws<ArgumentNullException>(() => queue.Post(nullItems));
    }

    [Fact]
    public async Task SendAsync_SingleItem_ShouldProcessSuccessfully()
    {
        // Arrange
        int processedValue = 0;
        var queue = new ActionBlock2<int>(x => processedValue = x);

        // Act
        bool result = await queue.SendAsync(42);
        await queue.CloseAsync();

        // Assert
        result.BeTrue();
        processedValue.Be(42);
    }

    [Fact]
    public async Task SendAsync_WithCancellationToken_ShouldProcessSuccessfully()
    {
        // Arrange
        int processedValue = 0;
        var queue = new ActionBlock2<int>(x => processedValue = x);
        var cts = new CancellationTokenSource();

        // Act
        bool result = await queue.SendAsync(42, cts.Token);
        await queue.CloseAsync();

        // Assert
        result.BeTrue();
        processedValue.Be(42);
    }

    [Fact]
    public async Task SendAsync_MultipleItems_ShouldProcessSuccessfully()
    {
        // Arrange
        var processedValues = new List<int>();
        var queue = new ActionBlock2<int>(x => processedValues.Add(x));
        var items = new[] { 1, 2, 3, 4, 5 };

        // Act
        bool result = await queue.SendAsync(items);
        await queue.CloseAsync();

        // Assert
        result.BeTrue();
        processedValues.BeEquivalent(items);
    }

    [Fact]
    public async Task SendAsync_MultipleItemsWithCancellationToken_ShouldProcessSuccessfully()
    {
        // Arrange
        var processedValues = new List<int>();
        var queue = new ActionBlock2<int>(x => processedValues.Add(x));
        var items = new[] { 1, 2, 3, 4, 5 };
        var cts = new CancellationTokenSource();

        // Act
        bool result = await queue.SendAsync(items, cts.Token);
        await queue.CloseAsync();

        // Assert
        result.BeTrue();
        processedValues.BeEquivalent(items);
    }

    [Fact]
    public async Task SendAsync_NullItems_ShouldThrow()
    {
        // Arrange
        var queue = new ActionBlock2<int>(x => { });
        IEnumerable<int> nullItems = null!;

        // Act & Assert
        await Verify.ThrowsAsync<ArgumentNullException>(async () => await queue.SendAsync(nullItems));
    }

    [Fact]
    public async Task SendAsync_CancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var queue = new ActionBlock2<int>(async x => await Task.Delay(1000), maxQueue: 1);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Verify.ThrowsAsync<OperationCanceledException>(async () => await queue.SendAsync(42, cts.Token));
    }

    [Fact]
    public async Task InputCount_ShouldReflectQueuedItems()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var queue = new ActionBlock2<int>(async x => await tcs.Task, maxQueue: 10, maxWorkers: 1);

        // Act
        queue.Post(1);
        queue.Post(2);
        queue.Post(3);

        // Assert
        await Task.Delay(100); // Allow time for first item to start processing
        queue.InputCount.Be(2); // First item being processed, 2 in queue

        tcs.SetResult(true);
        await queue.CloseAsync();
    }

    [Fact]
    public async Task IsCompleted_ShouldBeTrueAfterClose()
    {
        // Arrange
        var queue = new ActionBlock2<int>(x => { });

        // Act
        queue.IsCompleted.BeFalse();
        await queue.CloseAsync();

        // Assert
        queue.IsCompleted.BeTrue();
    }

    [Fact]
    public void Close_SyncMethod_ShouldWaitForCompletion()
    {
        // Arrange
        int processedValue = 0;
        var queue = new ActionBlock2<int>(x => { Thread.Sleep(100); processedValue = x; });

        // Act
        queue.Post(42);
        queue.Close();

        // Assert
        queue.IsCompleted.BeTrue();
        processedValue.Be(42);
    }

    [Fact]
    public async Task CloseAsync_ShouldWaitForCompletion()
    {
        // Arrange
        int processedValue = 0;
        var queue = new ActionBlock2<int>(async x => { await Task.Delay(100); processedValue = x; });

        // Act
        queue.Post(42);
        await queue.CloseAsync();

        // Assert
        queue.IsCompleted.BeTrue();
        processedValue.Be(42);
    }

    [Fact]
    public async Task CloseAsync_WithCancellationToken_ShouldComplete()
    {
        // Arrange
        var queue = new ActionBlock2<int>(x => { });
        var cts = new CancellationTokenSource();

        // Act
        queue.Post(42);
        await queue.CloseAsync(cts.Token);

        // Assert
        queue.IsCompleted.BeTrue();
    }

    [Fact]
    public async Task DisposeAsync_ShouldCloseQueue()
    {
        // Arrange
        int processedValue = 0;
        var queue = new ActionBlock2<int>(x => processedValue = x);

        // Act
        queue.Post(42);
        await queue.DisposeAsync();

        // Assert
        queue.IsCompleted.BeTrue();
        processedValue.Be(42);
    }

    [Fact]
    public async Task MaxWorkers_SingleWorker_ShouldProcessSequentially()
    {
        // Arrange
        var processOrder = new List<int>();
        var queue = new ActionBlock2<int>(async x =>
        {
            await Task.Delay(50);
            lock (processOrder) processOrder.Add(x);
        }, maxWorkers: 1);

        // Act
        for (int i = 1; i <= 5; i++)
        {
            await queue.SendAsync(i);
        }
        await queue.CloseAsync();

        // Assert
        Enumerable.SequenceEqual(processOrder, new[] { 1, 2, 3, 4, 5 }).BeTrue();
    }

    [Fact]
    public async Task MaxWorkers_MultipleWorkers_ShouldProcessConcurrently()
    {
        // Arrange
        var concurrentCount = 0;
        var maxConcurrent = 0;
        var lockObj = new object();

        var queue = new ActionBlock2<int>(async x =>
        {
            lock (lockObj)
            {
                concurrentCount++;
                maxConcurrent = Math.Max(maxConcurrent, concurrentCount);
            }

            await Task.Delay(100);

            lock (lockObj)
            {
                concurrentCount--;
            }
        }, maxWorkers: 3);

        // Act
        for (int i = 1; i <= 5; i++)
        {
            await queue.SendAsync(i);
        }
        await queue.CloseAsync();

        // Assert
        (maxConcurrent > 1).BeTrue();
        (maxConcurrent <= 3).BeTrue();
    }

    [Fact]
    public async Task MaxQueue_ExceedingCapacity_PostShouldReturnFalse()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var queue = new ActionBlock2<int>(async x => await tcs.Task, maxQueue: 2, maxWorkers: 1);

        // Act
        var result1 = queue.Post(1);
        var result2 = queue.Post(2);
        var result3 = queue.Post(3); // Should exceed capacity

        // Assert
        result1.BeTrue();
        result2.BeTrue();
        result3.BeFalse();

        tcs.SetResult(true);
        await queue.CloseAsync();
    }

    [Fact]
    public async Task ActionQueue_WithAsyncAction_ShouldHandleExceptions()
    {
        // Arrange
        var processedCount = 0;
        var queue = new ActionBlock2<int>(async x =>
        {
            await Task.CompletedTask;
            if (x == 2) throw new InvalidOperationException("Test exception");
            processedCount++;
        });

        // Act
        await queue.SendAsync(1);
        await queue.SendAsync(2);
        await queue.SendAsync(3);

        bool throwExceptions = false;
        try
        {
            await queue.CloseAsync();
        }
        catch (InvalidOperationException)
        {
            // Expected exception
            throwExceptions = true;
        }

        throwExceptions.BeTrue();

        // Assert - other items should still process
        processedCount.Be(1);
    }

    [Fact]
    public async Task ActionQueue_WithComplexType_ShouldProcess()
    {
        // Arrange
        var processedItems = new List<string>();
        var queue = new ActionBlock2<TestItem>(x => processedItems.Add(x.Name));
        var items = new[]
        {
            new TestItem { Id = 1, Name = "Item1" },
            new TestItem { Id = 2, Name = "Item2" },
            new TestItem { Id = 3, Name = "Item3" }
        };

        // Act
        await queue.SendAsync(items);
        await queue.CloseAsync();

        // Assert
        processedItems.BeEquivalent(new[] { "Item1", "Item2", "Item3" });
    }

    [Fact]
    public async Task Post_AfterClose_ShouldReturnFalse()
    {
        // Arrange
        var queue = new ActionBlock2<int>(x => { });
        await queue.CloseAsync();

        // Act
        bool result = queue.Post(42);

        // Assert
        result.BeFalse();
    }

    [Fact]
    public async Task SendAsync_AfterClose_ShouldReturnFalse()
    {
        // Arrange
        var queue = new ActionBlock2<int>(x => { });
        await queue.CloseAsync();

        // Act
        bool result = await queue.SendAsync(42);

        // Assert
        result.BeFalse();
    }

    private class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
