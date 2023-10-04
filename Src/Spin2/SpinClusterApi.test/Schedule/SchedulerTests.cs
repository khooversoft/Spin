using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SpinCluster.sdk.Actors.Agent;
using SpinCluster.sdk.Actors.Scheduler;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Actors.Smartc;
using SpinCluster.sdk.Models;
using SpinClusterApi.test.Application;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinClusterApi.test.Schedule;

/// <summary>
/// This test verifies that a work can be schedule for a SmartC
/// 
/// SmartC talks to its Actor to see if there is any work
/// SmartC's actor talks to schedule to see if there is any work available
/// </summary>

public class SchedulerTests : IClassFixture<ClusterApiFixture>
{
    private const string _agentId = "agent:test-agent";
    private const string _smartcId = "smartc:company30.com/contract1";
    private const string _principalId = "user1@company30.com";
    private const string _contractId = "contract:company30.com/contract1";
    private const string _sourceId = "source1";
    private const string _command = "create";

    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public SchedulerTests(ClusterApiFixture fixture) => _cluster = fixture;

    [Fact]
    public async Task SingleScheduleLifecycleTest1()
    {
        await Setup();

        (string workId, CreatePayload payload) = await CreateSchedule();
        await VerifyWork(workId, payload, false);

        await AssignToAgent(workId);
        await VerifyWork(workId, payload, true);
        await AddRunResult(workId);

        await Complete(workId);

        SchedulerClient schedulerClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();
        await schedulerClient.Clear("admin@domain.com", _context);

        ScheduleWorkClient workClient = _cluster.ServiceProvider.GetRequiredService<ScheduleWorkClient>();
        Option deleteResponse = await workClient.Delete(workId, _context);
        deleteResponse.IsOk().Should().BeTrue(deleteResponse.ToString());
    }

    private async Task Setup()
    {
        SchedulerClient schedulerClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();
        await schedulerClient.Clear("admin@domain.com", _context);

        var agent = new AgentModel
        {
            AgentId = _agentId,
            Enabled = true,
        };

        Option agentOption = await _cluster.ServiceProvider
            .GetRequiredService<AgentClient>()
            .Set(agent, _context);

        agentOption.IsOk().Should().BeTrue(agentOption.ToString());
    }

    private async Task<(string workId, CreatePayload payload)> CreateSchedule()
    {
        SchedulerClient schedulerClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();

        var payload1 = new CreatePayload
        {
            ContractId = _contractId,
            OwnerId = _principalId,
            Name = "name",
            AccessRights = new[]
            {
                new AccessBlock { BlockType = "blockType1", Grant = BlockGrant.ReadWrite, PrincipalId = _principalId },
                new AccessBlock { BlockType = "blockType2", Grant = BlockGrant.ReadWrite, PrincipalId = _principalId },
            },
            RoleRights = new[]
            {
                new RoleAccessBlock { Grant = BlockRoleGrant.Owner, PrincipalId = _principalId },
            }
        };

        var request = new ScheduleCreateModel
        {
            SmartcId = _smartcId,
            PrincipalId = _principalId,
            SourceId = _sourceId,
            Command = _command,
            Payloads = new DataObjectSetBuilder().Add(payload1).Build(),
        };

        Option response = await schedulerClient.CreateSchedule(request, _context);
        response.IsOk().Should().BeTrue(response.ToString());

        Option<SchedulesModel> schedulesOption = await schedulerClient.GetSchedules(_context);
        schedulesOption.IsOk().Should().BeTrue(schedulesOption.ToString());
        SchedulesModel schedulesModel = schedulesOption.Return();
        schedulesModel.Active.Count.Should().Be(1);
        schedulesModel.Active[0].WorkId.Should().Be(request.WorkId);
        schedulesModel.Active[0].AssignedDate.Should().BeNull();
        schedulesModel.CompletedItems.Count.Should().Be(0);

        return (request.WorkId, payload1);
    }

    private async Task VerifyWork(string workId, CreatePayload payload, bool verifyAssignment)
    {
        ScheduleWorkClient scheduleWorkClient = _cluster.ServiceProvider.GetRequiredService<ScheduleWorkClient>();

        Option<ScheduleWorkModel> workScheduleOption = await scheduleWorkClient.Get(workId, _context);
        workScheduleOption.IsOk().Should().BeTrue(workScheduleOption.ToString());

        ScheduleWorkModel workModel = workScheduleOption.Return();
        workModel.WorkId.Should().Be(workId);
        workModel.SmartcId.Should().Be(_smartcId);
        workModel.SourceId.Should().Be(_sourceId);
        workModel.CommandType.Should().Be("args");
        workModel.Command.Should().Be(_command);
        workModel.Payloads.Items.Count.Should().Be(1);
        workModel.Payloads.Items.ContainsKey(typeof(CreatePayload).GetTypeName()).Should().BeTrue();
        workModel.RunResults.Count.Should().Be(0);

        DataObject dataObject = workModel.Payloads.Items[typeof(CreatePayload).GetTypeName()];
        CreatePayload readPayload = dataObject.ToObject<CreatePayload>();
        payload.Should().Be(readPayload);

        readPayload = workModel.Payloads.GetObject<CreatePayload>();
        payload.Should().Be(readPayload);

        switch (verifyAssignment)
        {
            case false:
                workModel.Assigned.Should().BeNull();
                break;

            case true:
                workModel.Assigned.Should().NotBeNull();
                workModel.Assigned!.AgentId.Should().Be(_agentId);
                break;
        }
    }

    private async Task AssignToAgent(string workId)
    {
        SchedulerClient schedulerClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();
        ScheduleWorkClient workClient = _cluster.ServiceProvider.GetRequiredService<ScheduleWorkClient>();

        Option<WorkAssignedModel> assignedOption = await schedulerClient.AssignWork(_agentId, _context);
        assignedOption.IsOk().Should().BeTrue(assignedOption.ToString());

        WorkAssignedModel model = assignedOption.Return();
        model.WorkId.Should().Be(workId);
        model.SmartcId.Should().Be(_smartcId);
        model.CommandType.Should().Be("args");
        model.Command.Should().Be(_command);

        var noAssignOption = await schedulerClient.AssignWork(_agentId, _context);
        noAssignOption.IsError().Should().BeTrue();

        Option<SchedulesModel> schedulesOption = await schedulerClient.GetSchedules(_context);
        schedulesOption.IsOk().Should().BeTrue(schedulesOption.ToString());
        SchedulesModel schedulesModel = schedulesOption.Return();
        schedulesModel.Active.Count.Should().Be(1);
        schedulesModel.Active[0].WorkId.Should().Be(workId);
        schedulesModel.Active[0].AssignedDate.Should().NotBeNull();
        schedulesModel.CompletedItems.Count.Should().Be(0);

        var workModelOption = await workClient.Get(model.WorkId, _context);
        workModelOption.IsOk().Should().BeTrue(workModelOption.ToString());

        ScheduleWorkModel workModel = workModelOption.Return();
        workModel.WorkId.Should().Be(workId);
        workModel.SmartcId.Should().Be(_smartcId);
        workModel.SourceId.Should().Be(_sourceId);
        workModel.CommandType.Should().Be("args");
        workModel.Command.Should().Be(_command);
        workModel.Payloads.Items.Count.Should().Be(1);
        workModel.Payloads.Items.ContainsKey(typeof(CreatePayload).GetTypeName()).Should().BeTrue();
        workModel.RunResults.Count.Should().Be(0);

        workModel.Assigned.Should().NotBeNull();
        workModel.Assigned!.AgentId.Should().Be(_agentId);
        workModel.Assigned.AssignedCompleted.Should().BeNull();
    }

    private async Task AddRunResult(string workId)
    {
        var workClient = _cluster.ServiceProvider.GetRequiredService<ScheduleWorkClient>();

        var runResult = new RunResultModel
        {
            AgentId = _agentId,
            WorkId = workId,
            StatusCode = StatusCode.OK,
            Message = "completed",
        };

        var response = await workClient.AddRunResult(runResult, _context);
        response.IsOk().Should().BeTrue(response.ToString());

        Option<ScheduleWorkModel> workOption = await workClient.Get(workId, _context);
        workOption.IsOk().Should().BeTrue(workOption.ToString());

        ScheduleWorkModel model = workOption.Return();
        model.WorkId.Should().Be(workId);
        model.SmartcId.Should().Be(_smartcId);
        model.SourceId.Should().Be(_sourceId);
        model.CommandType.Should().Be("args");
        model.Command.Should().Be(_command);

        model.Assigned.Should().NotBeNull();
        model.Assigned!.AgentId.Should().Be(_agentId);
        model.Assigned!.AssignedCompleted.Should().BeNull();
        model.GetState().Should().Be(ScheduleWorkState.Assigned);

        model.RunResults.Count.Should().Be(1);
        model.RunResults[0].Should().Be(runResult);
    }

    private async Task Complete(string workId)
    {
        var schedulerClient = _cluster.ServiceProvider.GetRequiredService<SchedulerClient>();
        var workClient = _cluster.ServiceProvider.GetRequiredService<ScheduleWorkClient>();

        var complete = new AssignedCompleted
        {
            AgentId = _agentId,
            WorkId = workId,
            StatusCode = StatusCode.OK,
            Message = "completed"
        };

        var response = await workClient.CompletedWork(complete, _context);
        response.IsOk().Should().BeTrue(response.ToString());

        Option<ScheduleWorkModel> workOption = await workClient.Get(workId, _context);
        workOption.IsOk().Should().BeTrue(workOption.ToString());

        ScheduleWorkModel model = workOption.Return();
        model.WorkId.Should().Be(workId);
        model.SmartcId.Should().Be(_smartcId);
        model.SourceId.Should().Be(_sourceId);
        model.CommandType.Should().Be("args");
        model.Command.Should().Be(_command);

        model.GetState().Should().Be(ScheduleWorkState.Completed);
        model.Assigned.Should().NotBeNull();
        model.Assigned!.AgentId.Should().Be(_agentId);
        model.Assigned!.AssignedCompleted.Should().NotBeNull();
        model.Assigned.AssignedCompleted!.AgentId.Should().Be(_agentId);
        model.Assigned.AssignedCompleted.WorkId.Should().Be(workId);
        model.Assigned.AssignedCompleted.StatusCode.Should().Be(StatusCode.OK);
        model.Assigned.AssignedCompleted.Message.Should().Be("completed");
        model.RunResults.Count.Should().Be(1);

        Option<SchedulesModel> schedulesOption = await schedulerClient.GetSchedules(_context);
        schedulesOption.IsOk().Should().BeTrue(schedulesOption.ToString());
        SchedulesModel schedulesModel = schedulesOption.Return();
        schedulesModel.Active.Count.Should().Be(0);
        schedulesModel.CompletedItems.Count.Should().Be(1);
        schedulesModel.CompletedItems[0].AgentId.Should().Be(_agentId);
        schedulesModel.CompletedItems[0].WorkId.Should().Be(workId);
        schedulesModel.CompletedItems[0].StatusCode.Should().Be(StatusCode.OK);
        schedulesModel.CompletedItems[0].Message.Should().Be("completed");
    }

    private sealed record CreatePayload
    {
        public string ContractId { get; init; } = null!;
        public string OwnerId { get; init; } = null!;
        public string Name { get; init; } = null!;
        public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
        public IReadOnlyList<AccessBlock> AccessRights { get; init; } = Array.Empty<AccessBlock>();
        public IReadOnlyList<RoleAccessBlock> RoleRights { get; init; } = Array.Empty<RoleAccessBlock>();

        public bool Equals(CreatePayload? obj) => obj is CreatePayload document &&
            ContractId == document.ContractId &&
            OwnerId == document.OwnerId &&
            Name == document.Name &&
            CreatedDate.ToUniversalTime() == document.CreatedDate.ToUniversalTime() &&
            AccessRights.Count == document.AccessRights.Count &&
            AccessRights.OrderBy(x => x.ToString()).Zip(document.AccessRights.OrderBy(x => x.ToString())).All(x => x.First == x.Second) &&
            RoleRights.Count == document.RoleRights.Count &&
            RoleRights.OrderBy(x => x.ToString()).Zip(document.RoleRights.OrderBy(x => x.ToString())).All(x => x.First == x.Second);

        public override int GetHashCode() => HashCode.Combine(ContractId, OwnerId, Name, CreatedDate);
    }
}
