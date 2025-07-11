using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class PlanEmbeddedSequenceTests
{
    private static ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public async Task SimpleLinked()
    {
        var service = new ServiceCollection().BuildServiceProvider();
        int value = 0;

        Plan plan2 = new Plan(PlanMode.All)
            .Add((planContext, context) => StatusCode.OK.Action(x => value = 1));

        Option<PlanContext> plan = await new Plan(PlanMode.All)
            .Add(plan2)
            .Run(service, _context);

        plan.IsOk().BeTrue();
        value.Be(1);
        plan.Return().Action(x =>
        {
            x.History.Count.Be(2);
            x.History.All(x => x.StatusCode == StatusCode.OK);
            x.States.Count.Be(0);
        });

    }

    [Fact]
    public async Task TwoLinked()
    {
        var service = new ServiceCollection().BuildServiceProvider();
        int value = 0;

        Plan plan2 = new Plan(PlanMode.All)
            .Add((planContext, context) => StatusCode.OK.Action(x => value += 1));

        Plan plan3 = new Plan(PlanMode.All)
            .Add((planContext, context) => StatusCode.OK.Action(x => value += 1));

        Option<PlanContext> plan = await new Plan(PlanMode.All)
            .Add(plan2)
            .Add(plan3)
            .Run(service, _context);

        plan.IsOk().BeTrue();
        value.Be(2);
        plan.Return().Action(x =>
        {
            x.History.Count.Be(4);
            x.History.All(x => x.StatusCode == StatusCode.OK);
            x.States.Count.Be(0);
        });
    }

    [Fact]
    public async Task TwoLinkAsync()
    {
        var service = new ServiceCollection().BuildServiceProvider();
        int value = 0;

        Plan plan2 = new Plan(PlanMode.All)
            .AddAsync((planContext, context) => upValue());

        Plan plan3 = new Plan(PlanMode.All)
            .AddAsync((planContext, context) => upValue());

        Option<PlanContext> plan = await new Plan(PlanMode.All)
            .Add(plan2)
            .Add(plan3)
            .Run(service, _context);

        plan.IsOk().BeTrue();
        value.Be(2);
        plan.Return().Action(x =>
        {
            x.History.Count.Be(4);
            x.History.All(x => x.StatusCode == StatusCode.OK);
            x.States.Count.Be(0);
        });

        Task<Option> upValue()
        {
            value++;
            return new Option(StatusCode.OK).ToTaskResult();
        }
    }
}
