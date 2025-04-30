using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class PlanTests
{
    private static ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public async Task NoPlanForAll()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.All).Run(service, _context);
        plan.IsOk().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(0);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task NoPlanForIgnoreError()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.IgnoreError).Run(service, _context);
        plan.IsOk().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(0);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task NoPlanWhileError()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.First).Run(service, _context);
        plan.IsError().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(0);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task SinglePlan()
    {
        var service = new ServiceCollection().BuildServiceProvider();
        int value = 0;

        Option<PlanContext> plan = await new Plan(PlanMode.All)
            .Add((planContext, context) =>
            {
                value = 1;
                return StatusCode.OK;
            })
            .Run(service, _context);

        plan.IsOk().BeTrue();
        value.Be(1);
        plan.Return().Action(x =>
        {
            x.History.Count.Be(1);
            x.History.First().StatusCode.Be(StatusCode.OK);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task SingleSimplePlan()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.All)
            .Add((planContext, context) => StatusCode.OK)
            .Run(service, _context);

        plan.IsOk().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(1);
            x.History.First().StatusCode.Be(StatusCode.OK);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task SinglePlanFail()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        Option<PlanContext> plan = await new Plan(PlanMode.All)
            .Add((planContext, context) => StatusCode.NotFound)
            .Run(service, _context);

        plan.IsError().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(1);
            x.History.First().StatusCode.Be(StatusCode.NotFound);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task SinglePlanAsync()
    {
        var service = new ServiceCollection().BuildServiceProvider();
        int value = 0;

        var plan = await new Plan(PlanMode.All)
            .AddAsync((planContext, context) =>
            {
                value = 1;
                return new Option(StatusCode.OK).ToTaskResult();
            })
            .Run(service, _context);

        plan.IsOk().BeTrue();
        value.Be(1);
        plan.Return().Action(x =>
        {
            x.History.Count.Be(1);
            x.History.First().StatusCode.Be(StatusCode.OK);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task SinglePlanAsyncFail()
    {
        var service = new ServiceCollection().BuildServiceProvider();

        var plan = await new Plan(PlanMode.All)
            .AddAsync((planContext, context) =>
            {
                return new Option(StatusCode.Conflict).ToTaskResult();
            })
            .Run(service, _context);

        plan.IsError().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(1);
            x.History.First().StatusCode.Be(StatusCode.Conflict);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task SingleStepPlan()
    {
        var service = new ServiceCollection()
            .AddSingleton<StepState>()
            .AddSingleton<ObjectStep>()
            .BuildServiceProvider();

        var plan = await new Plan(PlanMode.All)
            .Add<ObjectStep>()
            .Run(service, _context);

        plan.IsOk().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(1);
            x.History.First().StatusCode.Be(StatusCode.OK);
            x.States.Count.Be(0);
        });

        var stepState = service.GetRequiredService<StepState>();
        stepState.NotNull();
        stepState.Value.Be(1);
    }

    [Fact]
    public async Task SingleStepPreConstructedPlan()
    {
        var service = new ServiceCollection()
            .AddSingleton<StepState>()
            .AddSingleton<ObjectStep>()
            .BuildServiceProvider();

        var objectStep = service.GetRequiredService<ObjectStep>();

        var plan = await new Plan(PlanMode.All)
            .Add(objectStep)
            .Run(service, _context);

        plan.IsOk().BeTrue();
        plan.Return().Action(x =>
        {
            x.History.Count.Be(1);
            x.History.First().StatusCode.Be(StatusCode.OK);
            x.States.Count.Be(0);
        });

        var stepState = service.GetRequiredService<StepState>();
        stepState.NotNull();
        stepState.Value.Be(1);
    }

    private record StepState
    {
        public int Value { get; set; } = 0;
    }

    private record ObjectStep : IPlan
    {
        private readonly StepState _stepState;
        public ObjectStep(StepState stepState) => _stepState = stepState;

        public Task<Option> Run(PlanContext planContext, ScopeContext context)
        {
            _stepState.Value += 1;
            return new Option(StatusCode.OK).ToTaskResult();
        }
    }
}
