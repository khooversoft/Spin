//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Types;

//namespace Toolbox.Orleans.test.Model;

//public class PrincipalIdentitySchemaTests
//{
//    [Fact]
//    public async Task AddSimpleNode()
//    {
//        var graph = new GraphMap();
//        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

//        // Create
//        var d = new PrincipalIdentity
//        {
//            PrincipalId = "user1@domain.com",
//            UserName = "name1",
//            Email = "user1Email@domain.com",
//            LoginProvider = "logonProvider1",
//            ProviderKey = "providerKey1",

//        };

//        var cmds = PrincipalIdentity.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

//        string[] matchTo = [
//            "upsert node key=user:user1@domain.com, email=user1Email@domain.com, entity { 'eyJwcmluY2lwYWxJZCI6InVzZXIxQGRvbWFpbi5jb20iLCJ1c2VyTmFtZSI6Im5hbWUxIiwiZW1haWwiOiJ1c2VyMUVtYWlsQGRvbWFpbi5jb20iLCJlbWFpbENvbmZpcm1lZCI6ZmFsc2UsInBhc3N3b3JkSGFzaCI6bnVsbCwibm9ybWFsaXplZFVzZXJOYW1lIjpudWxsLCJhdXRoZW50aWNhdGlvblR5cGUiOm51bGwsImlzQXV0aGVudGljYXRlZCI6ZmFsc2UsIm5hbWUiOm51bGwsImxvZ2luUHJvdmlkZXIiOiJsb2dvblByb3ZpZGVyMSIsInByb3ZpZGVyS2V5IjoicHJvdmlkZXJLZXkxIiwicHJvdmlkZXJEaXNwbGF5TmFtZSI6bnVsbH0=' };",
//            "upsert node key=userName:name1, uniqueIndex;",
//            "upsert edge fromKey=userName:name1, toKey=user:user1@domain.com, edgeType=uniqueIndex;",
//            "upsert node key=userEmail:user1email@domain.com, uniqueIndex;",
//            "upsert edge fromKey=userEmail:user1email@domain.com, toKey=user:user1@domain.com, edgeType=uniqueIndex;",
//            "upsert node key=logonProvider:logonprovider1/providerkey1, uniqueIndex;",
//            "upsert edge fromKey=logonProvider:logonprovider1/providerkey1, toKey=user:user1@domain.com, edgeType=uniqueIndex;",
//            ];

//        var cmdsMatchTo = matchTo.Join(Environment.NewLine);
//        cmds.Should().Be(cmdsMatchTo);

//        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        graph.Nodes.Count.Should().Be(4);
//        graph.Nodes["user:user1@domain.com"].Action(x =>
//        {
//            x.Key.Should().Be("user:user1@domain.com");
//            x.TagsString.Should().Be("email=user1Email@domain.com");
//            x.DataMap.Count.Should().Be(1);
//            x.DataMap.Values.First().Name.Should().Be("entity");
//        });
//        graph.Nodes["userName:name1"].Action(x =>
//        {
//            x.Key.Should().Be("userName:name1");
//            x.TagsString.Should().Be("uniqueIndex");
//            x.DataMap.Count.Should().Be(0);
//        });
//        graph.Nodes["userEmail:user1email@domain.com"].Action(x =>
//        {
//            x.Key.Should().Be("userEmail:user1email@domain.com");
//            x.TagsString.Should().Be("uniqueIndex");
//            x.DataMap.Count.Should().Be(0);
//        });
//        graph.Nodes["logonProvider:logonprovider1/providerkey1"].Action(x =>
//        {
//            x.Key.Should().Be("logonProvider:logonprovider1/providerkey1");
//            x.TagsString.Should().Be("uniqueIndex");
//            x.DataMap.Count.Should().Be(0);
//        });

//        graph.Edges.Count.Should().Be(3);
//        graph.Edges.Get("userName:name1", "user:user1@domain.com", direction: EdgeDirection.Both, "uniqueIndex").Count.Should().Be(1);
//        graph.Edges.Get("userEmail:user1email@domain.com", "user:user1@domain.com", direction: EdgeDirection.Both, "uniqueIndex").Count.Should().Be(1);
//        graph.Edges.Get("logonProvider:logonprovider1/providerkey1", "user:user1@domain.com", direction: EdgeDirection.Both, "uniqueIndex").Count.Should().Be(1);

//        // Select
//        string selectCmd = PrincipalIdentity.Schema.Code(d).BuildSelectCommand();
//        selectCmd.Should().Be("select (key=user:user1@domain.com) return entity;");
//        var selectOption = await testClient.ExecuteBatch(selectCmd, NullScopeContext.Instance);
//        selectOption.IsOk().Should().BeTrue();
//        selectOption.Return().Items.Length.Should().Be(1);

//        // Delete
//        cmds = PrincipalIdentity.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

//        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
//        newMapOption.IsOk().Should().BeTrue();

//        graph.Nodes.Count.Should().Be(0);
//        graph.Edges.Count.Should().Be(0);
//    }
//}
