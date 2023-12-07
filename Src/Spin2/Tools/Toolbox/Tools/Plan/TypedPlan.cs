using Microsoft.Extensions.DependencyInjection;
using Toolbox.Types;

namespace Toolbox.Tools;

public record TypedPlan<T> : IPlan where T : IPlan
{
    public Task<Option> Run(PlanContext planContext, ScopeContext context)
    {
        T step = planContext.NotNull().Service.GetRequiredService<T>().NotNull();
        return step.Run(planContext, context);
    }
}
