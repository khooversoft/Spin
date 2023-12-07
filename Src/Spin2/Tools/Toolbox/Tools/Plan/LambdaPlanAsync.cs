using Toolbox.Types;

namespace Toolbox.Tools;

public record LambdaPlanAsync : IPlan
{
    private readonly Func<PlanContext, ScopeContext, Task<Option>> _func;
    public LambdaPlanAsync(Func<PlanContext, ScopeContext, Task<Option>> func) => _func = func.NotNull();
    public Task<Option> Run(PlanContext planContext, ScopeContext context) => _func(planContext, context);
}
