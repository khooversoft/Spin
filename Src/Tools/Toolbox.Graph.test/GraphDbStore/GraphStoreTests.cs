﻿using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Types;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphStoreTests
{
    [Theory]
    [InlineData("n", "nodes/main/n")]
    [InlineData("node1.json", "nodes/main/node1.json")]
    [InlineData("data/node1.json", "nodes/main/data/data_node1.json")]
    [InlineData("data/company.com/node1.json", "nodes/main/data/company.com/data_company.com_node1.json")]
    [InlineData("Data/User1@company.com/node1.json", "nodes/main/data/user1@company.com/data_user1@company.com_node1.json")]
    public void CreateFileId(string source, string expected)
    {
        string result = GraphStoreAccess.CreateFileId(source, "main");
        result.Should().Be(expected);
    }

    [Fact]
    public async Task SingleFileWithoutNodeShouldFail()
    {
        IFileStore store = new InMemoryFileStore();
        GraphDb db = new GraphDb(store);

        var data = new { Name = "Name1", Value = "Value1" };

        var option = await db.Store.Set("node1", "main", data, NullScopeContext.Instance);
        option.IsError().Should().BeTrue(option.ToString());
    }

    private record NameValue(string name, int value);

    [Fact]
    public async Task SingleFileRoundTrip()
    {
        const string nodeKey = "subscription/node1.json";
        IFileStore store = new InMemoryFileStore();
        GraphDb db = new GraphDb(store);

        (await db.Graph.ExecuteScalar($"add node key={nodeKey};", NullScopeContext.Instance)).ThrowOnError();

        var data = new NameValue("Name1", 10);
        (await db.Store.Set(nodeKey, "main", data, NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue(x.ToString()));

        var readOption = await db.Store.Get<NameValue>(nodeKey, "main", NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue(readOption.ToString());

        var readData = readOption.Return();
        (readData == data).Should().BeTrue();

        (await db.Store.Exist(nodeKey, "main", NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue(x.ToString()));

        TestDirectory(store, ["directory.json", "nodes/main/subscription/subscription_node1.json"]);
    }

    [Fact]
    public async Task SingleFileNode()
    {
        const string nodeKey = "contract/company.com/node1.json";
        IFileStore store = new InMemoryFileStore();
        GraphDb db = new GraphDb(store);

        var data = new NameValue("Name1", 40);
        (await db.Graph.ExecuteScalar($"add node key={nodeKey};", NullScopeContext.Instance)).ThrowOnError();

        var option = await db.Store.Set(nodeKey, "main", data, NullScopeContext.Instance);
        option.IsOk().Should().BeTrue(option.ToString());
        TestDirectory(store, ["directory.json", "nodes/main/contract/company.com/contract_company.com_node1.json"]);

        (await db.Graph.ExecuteScalar($"select (key={nodeKey});", NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue(x.ToString()));
        (await db.Graph.ExecuteScalar($"delete (key={nodeKey});", NullScopeContext.Instance)).Action(x => x.IsError().Should().BeTrue(x.ToString()));

        (await db.Store.Delete(nodeKey, "main", NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue(x.ToString()));
        (await db.Graph.ExecuteScalar($"delete (key={nodeKey});", NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue(x.ToString()));

        TestDirectory(store, ["directory.json"]);
    }

    [Fact]
    public async Task MultipleNode()
    {
        IFileStore store = new InMemoryFileStore();
        GraphDb db = new GraphDb(store);
        int count = 100;
        var nodeKeys = new ConcurrentQueue<string>();

        var addBlock = new ActionBlock<int>(async x =>
        {
            string nodeKey = $"contract/company.com/node_{x}.json";
            nodeKeys.Enqueue(nodeKey);

            var data = new NameValue($"Name_{x}", 40 * x);
            (await db.Graph.ExecuteScalar($"add node key={nodeKey};", NullScopeContext.Instance)).ThrowOnError();

            var option = await db.Store.Set(nodeKey, "main", data, NullScopeContext.Instance);
            option.IsOk().Should().BeTrue(option.ToString());
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        await Enumerable.Range(0, count).ForEachAsync(async x => await addBlock.SendAsync(x));
        addBlock.Complete();
        await addBlock.Completion;

        InMemoryFileStore s = (InMemoryFileStore)store;
        s.Count.Should().Be(count + 1);
        nodeKeys.Count.Should().Be(count);

        int deleteCount = 0;
        var deleteBlock = new ActionBlock<string>(async nodeKey =>
        {
            (await db.Store.Delete(nodeKey, "main", NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue(x.ToString()));
            (await db.Graph.ExecuteScalar($"delete (key={nodeKey});", NullScopeContext.Instance)).Action(x => x.IsOk().Should().BeTrue(x.ToString()));
            var currentCount = Interlocked.Increment(ref deleteCount);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        await nodeKeys.ForEachAsync(async x => await deleteBlock.SendAsync(x));
        deleteBlock.Complete();
        await deleteBlock.Completion;

        deleteCount.Should().Be(100);
        s.Count.Should().Be(1);

        TestDirectory(store, ["directory.json"]);
    }

    private void TestDirectory(IFileStore fileStore, string[] expected)
    {
        InMemoryFileStore s = (InMemoryFileStore)fileStore;
        s.Count.Should().Be(expected.Length);

        string[] a = s.Select(x => x.Key).OrderBy(x => x).ToArray();
        a.SequenceEqual(expected).Should().BeTrue();
    }
}