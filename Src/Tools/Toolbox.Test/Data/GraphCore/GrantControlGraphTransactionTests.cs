using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.Data.Graph;

public class GrantControlGraphTransactionTests
{
    private readonly ITestOutputHelper _output;
    public GrantControlGraphTransactionTests(ITestOutputHelper logOutput) => _output = logOutput;

    private IHost CreateService()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(c => c.AddLambda(_output.WriteLine).AddDebug().AddFilter(_ => true));
                services.AddInMemoryKeyStore();
                services.AddSingleton<GraphCore>();

                services.AddDataSpace(cnfg =>
                {
                    cnfg.Spaces.Add(new SpaceDefinition
                    {
                        Name = "list",
                        ProviderName = "listStore",
                        BasePath = "listBase",
                        SpaceFormat = SpaceFormat.List,
                    });
                    cnfg.Add<ListStoreProvider>("listStore");
                });

                services.AddListStore<DataChangeRecord>("list");

                services.AddTransaction("default", config =>
                {
                    config.ListSpaceName = "list";
                    config.JournalKey = "TestJournal";
                    config.TrxProviders.Add<GraphCore>();
                });
            })
            .Build();

        return host;
    }

    [Fact]
    public async Task AddNodeInTransaction()
    {
        var host = CreateService();
        var graph = host.Services.GetRequiredService<GraphCore>();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        PrincipalIdentity principal;
        await using (var trx = await transaction.Start())
        {
            principal = new PrincipalIdentity("principalId", "nameIdentifier", "userName", "email");
            graph.Nodes.Add(principal.PrincipalId, principal.ToDataETag()).ThrowOnError();
        }

        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            x.Count.Be(1);
            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(1);
            list[0].Action(entry =>
            {
                entry.LogSequenceNumber.NotEmpty();
                entry.TransactionId.NotEmpty();
                entry.Date.IsDateTimeValid().BeTrue();
                entry.SourceName.Be(graph.SourceName);
                entry.ObjectId.Be(principal.PrincipalId);
                entry.Action.Be(ActionOperator.Add);
                entry.Before.BeNull();
                entry.After.NotNull();

                Node node = entry.After.ToObject<Node>().NotNull();
                node.NodeKey.Be(principal.PrincipalId);
                node.Payload.NotNull().ToObject<PrincipalIdentity>().Be(principal);
            });
        });

        (await transaction.Recovery()).BeOk();

        graph.Clear();
        graph.Nodes.Count.Be(0);
        graph.Edges.Count.Be(0);

        (await listStore.Get("TestJournal")).BeOk().Return()
            .Action(x => x.Count.Be(1))
            .Action(x => x[0].Entries.Count.Be(1));

        (await transaction.Recovery()).BeOk();
        graph.Nodes.Count.Be(1);
        graph.Edges.Count.Be(0);

        (await listStore.Get("TestJournal")).BeOk().Return()
            .Action(x => x.Count.Be(1))
            .Action(x => x[0].Entries.Count.Be(1));
    }

    [Fact]
    public async Task AddNodesWithRollback()
    {
        var host = CreateService();
        var graph = host.Services.GetRequiredService<GraphCore>();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        PrincipalIdentity principal1;
        PrincipalIdentity principal2;
        await using (var trx = await transaction.Start())
        {
            principal1 = new PrincipalIdentity("principalId", "nameIdentifier", "userName", "email");
            graph.Nodes.Add(principal1.PrincipalId, principal1.ToDataETag()).ThrowOnError();
        }

        await using (var trx = await transaction.Start())
        {
            principal2 = new PrincipalIdentity("principalId2", "nameIdentifier2", "userName2", "email2");
            graph.Nodes.Add(principal2.PrincipalId, principal2.ToDataETag()).ThrowOnError();

            graph.Nodes.Count.Be(2);
            graph.Nodes.ContainsKey(principal2.PrincipalId).BeTrue();
            graph.Nodes.TryGetValue(principal2.PrincipalId, out var existing).BeTrue();
            existing.NotNull().NodeKey.Be(principal2.PrincipalId);
            existing.Payload.NotNull().ToObject<PrincipalIdentity>().Be(principal2);

            (await transaction.Rollback()).BeOk();
        }

        graph.Nodes.Count.Be(1);
        graph.Nodes.ContainsKey(principal1.PrincipalId).BeTrue();
        graph.Nodes.ContainsKey(principal2.PrincipalId).BeFalse();


        (await listStore.Get("TestJournal")).BeOk().Return().Action(x =>
        {
            x.Count.Be(1);
            var list = x.SelectMany(x => x.Entries).ToList();
            list.Count.Be(1);
            list[0].Action(entry =>
            {
                entry.LogSequenceNumber.NotEmpty();
                entry.TransactionId.NotEmpty();
                entry.Date.IsDateTimeValid().BeTrue();
                entry.SourceName.Be(graph.SourceName);
                entry.ObjectId.Be(principal1.PrincipalId);
                entry.Action.Be(ActionOperator.Add);
                entry.Before.BeNull();
                entry.After.NotNull();

                Node node = entry.After.ToObject<Node>().NotNull();
                node.NodeKey.Be(principal1.PrincipalId);
                node.Payload.NotNull().ToObject<PrincipalIdentity>().Be(principal1);
            });
        });

        (await transaction.Recovery()).BeOk();

        graph.Clear();
        graph.Nodes.Count.Be(0);
        graph.Edges.Count.Be(0);

        (await listStore.Get("TestJournal")).BeOk().Return()
            .Action(x => x.Count.Be(1))
            .Action(x => x[0].Entries.Count.Be(1));

        (await transaction.Recovery()).BeOk();
        graph.Nodes.Count.Be(1);
        graph.Edges.Count.Be(0);

        (await listStore.Get("TestJournal")).BeOk().Return()
            .Action(x => x.Count.Be(1))
            .Action(x => x[0].Entries.Count.Be(1));
    }

    [Fact]
    public async Task AddNodesAndEdgeInTransaction()
    {
        var host = CreateService();
        var graph = host.Services.GetRequiredService<GraphCore>();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        PrincipalIdentity principal1;
        PrincipalIdentity principal2;
        Edge edge;

        await using (var trx = await transaction.Start())
        {
            principal1 = new PrincipalIdentity("principalId1", "nameIdentifier1", "userName1", "email1");
            principal2 = new PrincipalIdentity("principalId2", "nameIdentifier2", "userName2", "email2");

            graph.Nodes.Add(principal1.PrincipalId, principal1.ToDataETag()).ThrowOnError();
            graph.Nodes.Add(principal2.PrincipalId, principal2.ToDataETag()).ThrowOnError();

            edge = new Edge(principal1.PrincipalId, principal2.PrincipalId, "member");
            graph.Edges.TryAdd(edge).ThrowOnError();
        }

        graph.Nodes.Count.Be(2);
        graph.Edges.Count.Be(1);

        (await listStore.Get("TestJournal")).BeOk().Return().Action(records =>
        {
            records.Count.Be(1);

            var entries = records.SelectMany(x => x.Entries).ToList();
            entries.Count.Be(3);

            var nodeEntries = entries.Where(x => x.TypeName == nameof(Node)).ToList();
            nodeEntries.Count.Be(2);
            nodeEntries.All(x => x.Action == ActionOperator.Add).BeTrue();
            nodeEntries.All(x => x.SourceName == graph.SourceName).BeTrue();

            var node1 = nodeEntries.Single(x => x.ObjectId.EqualsIgnoreCase(principal1.PrincipalId));
            node1.After.NotNull();
            node1.Before.BeNull();
            var node1Value = node1.After.ToObject<Node>().NotNull();
            node1Value.NodeKey.Be(principal1.PrincipalId);
            node1Value.Payload.NotNull().ToObject<PrincipalIdentity>().Be(principal1);

            var node2 = nodeEntries.Single(x => x.ObjectId.EqualsIgnoreCase(principal2.PrincipalId));
            node2.After.NotNull();
            node2.Before.BeNull();
            var node2Value = node2.After.ToObject<Node>().NotNull();
            node2Value.NodeKey.Be(principal2.PrincipalId);
            node2Value.Payload.NotNull().ToObject<PrincipalIdentity>().Be(principal2);

            var edgeEntry = entries.Single(x => x.TypeName == nameof(Edge));
            edgeEntry.Action.Be(ActionOperator.Add);
            edgeEntry.SourceName.Be(graph.SourceName);
            edgeEntry.After.NotNull();
            edgeEntry.Before.BeNull();

            var edgeValue = edgeEntry.After.ToObject<Edge>().NotNull();
            edgeValue.FromKey.Be(principal1.PrincipalId);
            edgeValue.ToKey.Be(principal2.PrincipalId);
            edgeValue.EdgeType.Be("member");
        });

        (await transaction.Recovery()).BeOk();

        graph.Clear();
        graph.Nodes.Count.Be(0);
        graph.Edges.Count.Be(0);

        (await listStore.Get("TestJournal")).BeOk().Return()
            .Action(x => x.Count.Be(1))
            .Action(x => x[0].Entries.Count.Be(3));

        (await transaction.Recovery()).BeOk();
        graph.Nodes.Count.Be(2);
        graph.Edges.Count.Be(1);

        (await listStore.Get("TestJournal")).BeOk().Return()
            .Action(x => x.Count.Be(1))
            .Action(x => x[0].Entries.Count.Be(3));
    }

    [Fact]
    public async Task UpdateAndDeleteWithRollbackShouldRestoreGraph()
    {
        var host = CreateService();
        var graph = host.Services.GetRequiredService<GraphCore>();
        var transaction = host.Services.GetRequiredKeyedService<Transaction>("default");
        var listStore = host.Services.GetRequiredService<IListStore<DataChangeRecord>>();

        PrincipalIdentity principal1;
        PrincipalIdentity principal2;
        Edge edge;

        await using (var trx = await transaction.Start())
        {
            principal1 = new PrincipalIdentity("principalId1", "nameIdentifier1", "userName1", "email1");
            principal2 = new PrincipalIdentity("principalId2", "nameIdentifier2", "userName2", "email2");

            graph.Nodes.Add(principal1.PrincipalId, principal1.ToDataETag()).ThrowOnError();
            graph.Nodes.Add(principal2.PrincipalId, principal2.ToDataETag()).ThrowOnError();

            edge = new Edge(principal1.PrincipalId, principal2.PrincipalId, "member");
            graph.Edges.TryAdd(edge).ThrowOnError();
        }

        graph.Nodes.TryGetValue(principal1.PrincipalId, out var originalNode).BeTrue();
        var originalPayload = originalNode.NotNull().Payload.NotNull().ToObject<PrincipalIdentity>().NotNull();

        await using (var trx = await transaction.Start())
        {
            var updatedPrincipal = principal1 with { UserName = "userName1-updated", Email = "email1-updated" };

            Node replaceNode = new(updatedPrincipal.PrincipalId, updatedPrincipal.ToDataETag());
            graph.Nodes.AddOrUpdate(replaceNode).ThrowOnError();
            graph.Edges.Remove(edge.EdgeKey).ThrowOnError();

            graph.Nodes.TryGetValue(principal1.PrincipalId, out var updatedNode).BeTrue();
            updatedNode.NotNull().Payload.NotNull().ToObject<PrincipalIdentity>().UserName.Be("userName1-updated");
            graph.Edges.Count.Be(0);

            (await transaction.Rollback()).BeOk();
        }

        graph.Nodes.TryGetValue(principal1.PrincipalId, out var rolledBackNode).BeTrue();
        rolledBackNode.NotNull().Payload.NotNull().ToObject<PrincipalIdentity>().Be(originalPayload);
        graph.Edges.Contains(edge.EdgeKey).BeTrue();

        (await listStore.Get("TestJournal")).BeOk().Return().Action(records =>
        {
            records.Count.Be(1);
            records.Single().Entries.Count.Be(3);
        });

        (await transaction.Recovery()).BeOk();

        graph.Clear();
        graph.Nodes.Count.Be(0);
        graph.Edges.Count.Be(0);

        (await transaction.Recovery()).BeOk();
        graph.Nodes.Count.Be(2);
        graph.Edges.Count.Be(1);
    }


    private record GroupDetail(string Name);
    private record GroupPolicy(string NameIdentifier, string PrincipalIdentifier);
    private record PrincipalIdentity(string PrincipalId, string NameIdentifier, string UserName, string Email);
}
