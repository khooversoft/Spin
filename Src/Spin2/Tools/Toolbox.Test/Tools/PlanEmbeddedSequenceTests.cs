using FluentAssertions;
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

        plan.IsOk().Should().BeTrue();
        value.Should().Be(1);
        plan.Return().Action(x =>
        {
            x.History.Count.Should().Be(2);
            x.History.All(x => x.StatusCode == StatusCode.OK);
            x.States.Count.Should().Be(0);
        });

    }
}
