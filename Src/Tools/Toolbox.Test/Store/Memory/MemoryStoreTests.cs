using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Store.Memory;

public class MemoryStoreTests
{
    [Fact]
    public void AddGetRemove()
    {
        var store = new MemoryStore();
        const string path = "path/data";
        const string dataValue = "data";
        const string dataValue2 = "data2";
        DataETag data = dataValue.ToDataETag();
        DataETag data2 = dataValue2.ToDataETag();

        store.Exist(path).Should().BeFalse();
        store.IsLeased(path).Should().BeFalse();

        store.Add(path, data).IsOk().Should().BeTrue();
        store.Exist(path).Should().BeTrue();

        store.Add(path, data2).IsConflict().Should().BeTrue();

        var readDataTag = store.Get(path);
        readDataTag.IsOk().Should().BeTrue();
        readDataTag.Return().Data.BytesToString().Should().Be(dataValue);

        store.Search("*").Action(x =>
        {
            x.Count.Should().Be(1);
            x.First().Path.Should().Be(path);
        });

        var delete = store.Remove(path);
        delete.IsOk().Should().BeTrue();
        store.Exist(path).Should().BeFalse();
        store.Search("*").Count.Should().Be(0);

        readDataTag = store.Get(path);
        readDataTag.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public void AddGetSetRemove()
    {
        var store = new MemoryStore();
        const string path = "path/data";
        const string dataValue = "data";
        const string dataValue2 = "data2";
        DataETag data = dataValue.ToDataETag();
        DataETag data2 = dataValue2.ToDataETag();

        store.Exist(path).Should().BeFalse();

        store.Add(path, data).IsOk().Should().BeTrue();
        store.Exist(path).Should().BeTrue();

        store.Add(path, data2).IsConflict().Should().BeTrue();

        var readDataTag = store.Get(path);
        readDataTag.IsOk().Should().BeTrue();
        readDataTag.Return().Data.BytesToString().Should().Be(dataValue);

        store.Search("*").Action(x =>
        {
            x.Count.Should().Be(1);
            x.First().Path.Should().Be(path);
        });

        store.Set(path, data2).IsOk().Should().BeTrue();
        store.Exist(path).Should().BeTrue();

        readDataTag = store.Get(path);
        readDataTag.IsOk().Should().BeTrue();
        readDataTag.Return().Data.BytesToString().Should().Be(dataValue2);

        store.Search("*").Action(x =>
        {
            x.Count.Should().Be(1);
            x.First().Path.Should().Be(path);
        });

        var delete = store.Remove(path);
        delete.IsOk().Should().BeTrue();
        store.Exist(path).Should().BeFalse();
        store.Search("*").Count.Should().Be(0);

        readDataTag = store.Get(path);
        readDataTag.IsNotFound().Should().BeTrue();
    }

    [Fact]
    public async Task ManyAddThreads()
    {
        var store = new MemoryStore();
        const int count = 100;
        int rowCount = 0;

        var tasks1 = Enumerable.Range(0, count).Select(x => add(x)).ToArray();
        var tasks2 = Enumerable.Range(0, count).Reverse().Select(x => add(x)).ToArray();

        var taskEnd = Task.WhenAll(tasks1.Concat(tasks2));
        await taskEnd;

        store.Search("*").Count.Should().Be(count);
        rowCount.Should().Be(count);

        Task add(int index) => Task.Run(() =>
        {
            var option = store.Add($"path/{index}", index.ToString().ToDataETag());
            if (option.IsOk()) Interlocked.Increment(ref rowCount);
        });
    }


    [Fact]
    public async Task AddTwoThreads()
    {
        var store = new MemoryStore();
        const int count = 100;
        int rowCount = 0;
        int attemptCount = 0;
        int runnerCount = 0;
        ManualResetEvent readyEvent = new ManualResetEvent(false);

        var tasks1 = Task.Run(() => runner(count));
        var tasks2 = Task.Run(() => runner(count));

        while (runnerCount != 2) await Task.Yield();
        readyEvent.Set();

        var taskEnd = Task.WhenAll(tasks1, tasks2);
        await taskEnd;

        store.Search("*").Count.Should().Be(count);
        rowCount.Should().Be(count);
        attemptCount.Should().Be(count * 2);

        Task runner(int count)
        {
            Interlocked.Increment(ref runnerCount);
            readyEvent.WaitOne();

            Enumerable.Range(0, count).ForEach(x =>
            {
                Interlocked.Increment(ref attemptCount);

                var result = store.Add($"path/{x}", x.ToString().ToDataETag());
                if(result.IsOk()) Interlocked.Increment(ref rowCount);
            });

            return Task.CompletedTask;
        }
    }
}
