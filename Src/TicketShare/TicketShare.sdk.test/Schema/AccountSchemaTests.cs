//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Types;

//namespace TicketShare.sdk.test.Schema;

//public class AccountSchemaTests
//{
//    [Fact]
//    public async Task AddSimpleNode()
//    {
//        var graph = new GraphMap();
//        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

//        // Create
//        var d = new AccountRecord
//        {
//            PrincipalId = "user1@domain.com",
//            Name = "name1",
//            ContactItems = [new ContactRecord { Type = ContactType.Cell, Value = "425" }],
//        };

//        var cmds = AccountRecord.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

//        string[] matchTo = [
//            "upsert node key=account:user1@domain.com, entity { 'eyJwcmluY2lwYWxJZCI6InVzZXIxQGRvbWFpbi5jb20iLCJuYW1lIjoibmFtZTEiLCJjb250YWN0SXRlbXMiOlt7InR5cGUiOiJjZWxsIiwidmFsdWUiOiI0MjUifV0sImFkZHJlc3MiOltdLCJjYWxlbmRhckl0ZW1zIjpbXX0=' };"
//            ];

//        var cmdsMatchTo = matchTo.Join(Environment.NewLine);
//        cmds.Should().Be(cmdsMatchTo);

//        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        graph.Nodes.Count.Should().Be(1);
//        graph.Nodes.First().Action(x =>
//        {
//            x.Key.Should().Be("account:user1@domain.com");
//            x.DataMap.Count.Should().Be(1);
//            x.DataMap.Values.First().Name.Should().Be("entity");
//        });
//        graph.Edges.Count.Should().Be(0);

//        // Select
//        string selectCmd = AccountRecord.Schema.Code(d).BuildSelectCommand();
//        selectCmd.Should().Be("select (key=account:user1@domain.com) return entity;");
//        var selectOption = await testClient.ExecuteBatch(selectCmd, NullScopeContext.Instance);
//        selectOption.IsOk().Should().BeTrue();
//        selectOption.Return().Items.Length.Should().Be(1);

//        // Delete
//        cmds = AccountRecord.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

//        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        graph.Nodes.Count.Should().Be(0);
//        graph.Edges.Count.Should().Be(0);
//    }
//}
