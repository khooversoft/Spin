using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class PlanSingleSequenceTests
{
    private static ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public async Task AllPlan()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.All)
            .Add((planContext, context) => StatusCode.OK)
            .Add((planContext, context) => StatusCode.OK)
            .Add((planContext, context) => StatusCode.OK)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(3);
            x.History.All(x => x.StatusCode == StatusCode.OK);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task IgnoreErrorPlanSingle()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.IgnoreError)
            .Add((planContext, context) => StatusCode.NotFound)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(1);
            x.History.All(x => x.StatusCode == StatusCode.NotFound);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task IgnoreErrorPlanThree()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.IgnoreError)
            .Add((planContext, context) => StatusCode.NotFound)
            .Add((planContext, context) => StatusCode.NotFound)
            .Add((planContext, context) => StatusCode.NotFound)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(3);
            x.History.All(x => x.StatusCode == StatusCode.NotFound);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task IgnoreErrorPlanMixed()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.IgnoreError)
            .Add((planContext, context) => StatusCode.OK)
            .Add((planContext, context) => StatusCode.NotFound)
            .Add((planContext, context) => StatusCode.OK)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(3);
            x.History.First().StatusCode.Should().Be(StatusCode.OK);
            x.History.Skip(1).First().StatusCode.Should().Be(StatusCode.NotFound);
            x.History.Last().StatusCode.Should().Be(StatusCode.OK);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task FirstPlanWithNoError()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.First)
            .Add((planContext, context) => StatusCode.OK)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(1);
            x.History.All(x => x.StatusCode == StatusCode.OK);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task FirstPlanWithError()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.First)
        .Add((planContext, context) => StatusCode.Conflict)
        .Run(service, _context);

        plan.IsError().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(1);
            x.History.All(x => x.StatusCode == StatusCode.Conflict);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task FirstPlanOkToError()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.First)
            .Add((planContext, context) => StatusCode.OK)
            .Add((planContext, context) => StatusCode.NotFound)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(1);
            x.History.First().StatusCode.Should().Be(StatusCode.OK);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task FirstPlanErrorToOk()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.First)
            .Add((planContext, context) => StatusCode.NotFound)
            .Add((planContext, context) => StatusCode.OK)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(2);
            x.History.First().StatusCode.Should().Be(StatusCode.NotFound);
            x.History.Skip(1).First().StatusCode.Should().Be(StatusCode.OK);
            x.States.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task FirstPlanMix()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.First)
            .Add((planContext, context) => StatusCode.NotFound)
            .Add((planContext, context) => StatusCode.NotFound)
            .Add((planContext, context) => StatusCode.OK)
            .Add((planContext, context) => StatusCode.NotFound)
            .Run(service, _context);

        plan.IsOk().Should().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(3);
            x.History.First().StatusCode.Should().Be(StatusCode.NotFound);
            x.History.Skip(1).First().StatusCode.Should().Be(StatusCode.NotFound);
            x.History.Last().StatusCode.Should().Be(StatusCode.OK);
            x.States.Count.Should().Be(0);
        });
    }
}
