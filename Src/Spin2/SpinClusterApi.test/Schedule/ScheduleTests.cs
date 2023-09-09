using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Models;
using SpinClusterApi.test.Application;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Types;

namespace SpinClusterApi.test.Schedule;

/// <summary>
/// This test verifies that a work can be schedule for a SmartC
/// 
/// SmartC talks to its Actor to see if there is any work
/// SmartC's actor talks to schedule to see if there is any work available
/// </summary>

public class ScheduleTests : IClassFixture<ClusterApiFixture>
{
    private const string _agentId = "agent:testAgent";

    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public ScheduleTests(ClusterApiFixture fixture) => _cluster = fixture;

    //[Fact]
    //public async Task SingleScheduleWithAgentAndSmartC()
    //{
    //    ObjectBuilderOption option = ObjectBuilderOptionTool
    //        .ReadFromResource(typeof(ScheduleTests), "SpinClusterApi.test.Schedule.ScheduleSetup.json");

    //    await BuildData(option);
    //    await ScheduleWork(option);

    //    AgentAssignmentModel assignment = await Agent_AskForWork();
    //    await SmartcRunning(option, assignment.SmartcId);
    //}

    private async Task BuildData(ObjectBuilderOption option)
    {
        await new TestObjectBuilder()
            .SetOption(option)
            .SetService(_cluster.ServiceProvider)
            .AddStandard()
            .Build(_context);
    }

    private async Task ScheduleWork(ObjectBuilderOption option)
    {
        ScheduleClient client = _cluster.ServiceProvider.GetRequiredService<ScheduleClient>();

        var work = new ScheduleWorkModel
        {
            SmartcId = option.Accounts.First().AccountId,
            SourceId = "test",
            Command = "ping",
        };

        var queueResult = await client.EnqueueSchedule(work, _context);
        queueResult.IsOk().Should().BeTrue();
    }

    private async Task<AgentAssignmentModel> Agent_AskForWork()
    {
        AgentClient client = _cluster.ServiceProvider.GetRequiredService<AgentClient>();

        Option<AgentAssignmentModel> agentResponseOption = await client.GetAssignment(_agentId, _context);
        agentResponseOption.IsOk().Should().BeTrue();

        return agentResponseOption.Return();
    }

    private async Task SmartcRunning(ObjectBuilderOption option, string contractId)
    {
        SmartcClient client = _cluster.ServiceProvider.GetRequiredService<SmartcClient>();

        // Get assignment
        Option<AgentAssignmentModel> modelOption = await client.GetAssignment(contractId, _context);
        modelOption.IsError().Should().BeTrue();

        AgentAssignmentModel assignment = modelOption.Return();
        assignment.Should().NotBeNull();

        assignment!.AgentId.Should().Be(_agentId);
        assignment.WorkId.Should().NotBeNullOrEmpty();
        assignment.SmartcId.Should().Be(contractId);
        assignment.CommandType.Should().Be("args");
        assignment.Command.Should().Be("ping");

        // Set run state
        var model = new SmartcRunResultModel
        {
            SmartcId = option.Accounts.First().AccountId,
            StatusCode = StatusCode.OK,
            Message = "completed",
        };

        var completedOption = await client.CompletedWork(model, _context);
        completedOption.IsOk().Should().BeTrue();
    }
}
