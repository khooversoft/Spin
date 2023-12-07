using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public record LambdaPlan : IPlan
{
    private readonly Func<PlanContext, ScopeContext, Option> _func;
    public LambdaPlan(Func<PlanContext, ScopeContext, Option> func) => _func = func.NotNull();
    public Task<Option> Run(PlanContext planContext, ScopeContext context)
    {
        var option = _func(planContext, context);
        return option.ToTaskResult();
    }
}
