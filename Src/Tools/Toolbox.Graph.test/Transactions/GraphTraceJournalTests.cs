using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Journal;
using Toolbox.Store;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Transactions;

public class GraphTraceJournalTests
{
    [Fact]
    public async Task SingleJournal()
    {
        IFileStore fileStorage = new InMemoryFileStore(new NullLogger<InMemoryFileStore>());

        await using (var testClient = GraphTestStartup.CreateGraphTestHost(config: x => x.AddSingleton<IFileStore>(fileStorage)))
        {
            ILogger<GraphTraceJournalTests> logger = testClient.ServiceProvider.GetRequiredService<ILogger<GraphTraceJournalTests>>();
            var context = new ScopeContext(logger);

            string q = """
                add node key=node1 set t1;
                add node key=node2 set t2,client;
                """;

            (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
            {
                x.IsOk().Should().BeTrue(x.ToString());
                x.Value.Items.Count.Should().Be(2);
                x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
                x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
            });
        }

        await using (var testClient = GraphTestStartup.CreateGraphTestHost(config: x => x.AddSingleton<IFileStore>(fileStorage)))
        {
            IJournalFile traceLog = testClient.ServiceProvider.GetRequiredKeyedService<IJournalFile>(GraphConstants.Trace.DiKeyed);
            ILogger<GraphTraceJournalTests> logger = testClient.ServiceProvider.GetRequiredService<ILogger<GraphTraceJournalTests>>();
            var context = new ScopeContext(logger);

            var journals = await traceLog.ReadJournals(context);
            journals.Count.Should().Be(5);

            journals[0].Type.Should().Be(JournalType.Start);
            journals[1].Type.Should().Be(JournalType.Action);
            journals[2].Type.Should().Be(JournalType.Action);
            journals[3].Type.Should().Be(JournalType.Action);
            journals[4].Type.Should().Be(JournalType.Commit);
        }
    }

    [Fact]
    public async Task MultipleJournals()
    {
        IFileStore fileStorage = new InMemoryFileStore(new NullLogger<InMemoryFileStore>());

        await using (var testClient = GraphTestStartup.CreateGraphTestHost(config: x => x.AddSingleton<IFileStore>(fileStorage)))
        {
            ILogger<GraphTraceJournalTests> logger = testClient.ServiceProvider.GetRequiredService<ILogger<GraphTraceJournalTests>>();
            var context = new ScopeContext(logger);

            string q = """
                add node key=node1 set t1;
                add node key=node2 set t2,client;
                """;

            (await testClient.ExecuteBatch(q, NullScopeContext.Default)).Action(x =>
            {
                x.IsOk().Should().BeTrue(x.ToString());
                x.Value.Items.Count.Should().Be(2);
                x.Value.Items[0].Action(y => TestReturn(y, StatusCode.OK));
                x.Value.Items[1].Action(y => TestReturn(y, StatusCode.OK));
            });

            string q2 = """
                add node key=node3 set t1;
                add edge from=node1, to=node2, type=default;
                add node key=node4 set t2,client;
                add edge from=node3, to=node4, type=default;
                """;

            (await testClient.ExecuteBatch(q2, NullScopeContext.Default)).Action(x =>
            {
                x.IsOk().Should().BeTrue(x.ToString());
                x.Value.Items.Count.Should().Be(4);

                int index = 0;
                x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
                x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
                x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
                x.Value.Items[index++].Action(y => TestReturn(y, StatusCode.OK));
            });
        }

        await using (var testClient = GraphTestStartup.CreateGraphTestHost(config: x => x.AddSingleton<IFileStore>(fileStorage)))
        {
            IJournalFile traceLog = testClient.ServiceProvider.GetRequiredKeyedService<IJournalFile>(GraphConstants.Trace.DiKeyed);
            ILogger<GraphTraceJournalTests> logger = testClient.ServiceProvider.GetRequiredService<ILogger<GraphTraceJournalTests>>();
            var context = new ScopeContext(logger);

            var journals = await traceLog.ReadJournals(context);
            journals.Count.Should().Be(12);

            int index = 0;
            journals[index++].Type.Should().Be(JournalType.Start);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Commit);

            journals[index++].Type.Should().Be(JournalType.Start);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Action);
            journals[index++].Type.Should().Be(JournalType.Commit);
        }
    }

    private void TestReturn(QueryResult graphResult, StatusCode statusCode)
    {
        graphResult.Option.StatusCode.Should().Be(statusCode, graphResult.Option.ToString());
    }
}
