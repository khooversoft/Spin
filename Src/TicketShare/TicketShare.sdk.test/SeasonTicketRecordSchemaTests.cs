using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Types;

namespace TicketShare.sdk.test;

public class SeasonTicketRecordSchemaTests
{
    [Fact]
    public async Task AddSimpleNode()
    {
        var graph = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

        (await testClient.ExecuteBatch("add node key=user:owner1@domain.com;", NullScopeContext.Instance)).IsOk().Should().BeTrue();
        (await testClient.ExecuteBatch("add node key=user:user1@domain.com;", NullScopeContext.Instance)).IsOk().Should().BeTrue();

        // Create
        var d = new SeasonTicketRecord
        {
            SeasonTicketId = "chasat/2025/huskiess/football",
            Name = "name1",
            Description = "test record",
            OwnerPrincipalId = "owner1@domain.com",
            Members = [
                new RoleRecord { PrincipalId = "user1@domain.com", MemberRole = RolePermission.Owner }
                ],
        };

        var cmds = SeasonTicketRecord.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

        string[] matchTo = [
            "upsert node key=seasonTicket:chasat/2025/huskiess/football, entity { 'eyJzZWFzb25UaWNrZXRJZCI6ImNoYXNhdC8yMDI1L2h1c2tpZXNzL2Zvb3RiYWxsIiwibmFtZSI6Im5hbWUxIiwiZGVzY3JpcHRpb24iOiJ0ZXN0IHJlY29yZCIsIm93bmVyUHJpbmNpcGFsSWQiOiJvd25lcjFAZG9tYWluLmNvbSIsInRhZ3MiOm51bGwsInByb3BlcnRpZXMiOltdLCJtZW1iZXJzIjpbeyJwcmluY2lwYWxJZCI6InVzZXIxQGRvbWFpbi5jb20iLCJtZW1iZXJSb2xlIjoib3duZXIifV0sInNlYXRzIjpbXSwiY2hhbmdlTG9ncyI6W119' };",
            "upsert edge fromKey=seasonTicket:chasat/2025/huskiess/football, toKey=user:owner1@domain.com, edgeType=seasonTicket-identity-to-identity;",
            "upsert edge fromKey=seasonTicket:chasat/2025/huskiess/football, toKey=user:user1@domain.com, edgeType=seasonTicket-identity-to-identity;",
        ];

        var cmdsMatchTo = matchTo.Join(Environment.NewLine);
        cmds.Should().Be(cmdsMatchTo);

        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(3);
        graph.Nodes["seasonTicket:chasat/2025/huskiess/football"].Action(x =>
        {
            x.Key.Should().Be("seasonTicket:chasat/2025/huskiess/football");
            x.DataMap.Count.Should().Be(1);
            x.DataMap.Values.First().Name.Should().Be("entity");
        });
        graph.Nodes["user:owner1@domain.com"].Action(x =>
        {
            x.Key.Should().Be("user:owner1@domain.com");
            x.DataMap.Count.Should().Be(0);
        });
        graph.Nodes["user:user1@domain.com"].Action(x =>
        {
            x.Key.Should().Be("user:user1@domain.com");
            x.DataMap.Count.Should().Be(0);
        });

        graph.Edges.Count.Should().Be(2);
        graph.Edges.Get("seasonTicket:chasat/2025/huskiess/football", "user:owner1@domain.com", direction: EdgeDirection.Both, "seasonTicket-identity-to-identity").Count.Should().Be(1);
        graph.Edges.Get("seasonTicket:chasat/2025/huskiess/football", "user:user1@domain.com", direction: EdgeDirection.Both, "seasonTicket-identity-to-identity").Count.Should().Be(1);

        // Select
        string selectCmd = SeasonTicketRecord.Schema.Code(d).BuildSelectCommand();
        selectCmd.Should().Be("select (key=seasonTicket:chasat/2025/huskiess/football) return entity;");
        var selectOption = await testClient.ExecuteBatch(selectCmd, NullScopeContext.Instance);
        selectOption.IsOk().Should().BeTrue();
        selectOption.Return().Items.Length.Should().Be(1);

        // Delete
        cmds = SeasonTicketRecord.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(2);
        graph.Edges.Count.Should().Be(0);
    }
}
