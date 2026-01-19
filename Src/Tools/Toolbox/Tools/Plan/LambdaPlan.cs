//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Tools;

//public record LambdaPlan : IPlan
//{
//    private readonly Func<PlanContext, ScopeContext, Option> _func;
//    public LambdaPlan(Func<PlanContext, ScopeContext, Option> func) => _func = func.NotNull();
//    public Task<Option> Run(PlanContext planContext, ScopeContext context)
//    {
//        var option = _func(planContext, context);
//        return option.ToTaskResult();
//    }
//}


//public record LambdaPlanAsync : IPlan
//{
//    private readonly Func<PlanContext, ScopeContext, Task<Option>> _func;
//    public LambdaPlanAsync(Func<PlanContext, ScopeContext, Task<Option>> func) => _func = func.NotNull();
//    public Task<Option> Run(PlanContext planContext, ScopeContext context) => _func(planContext, context);
//}

//public record TypedPlan<T> : IPlan where T : IPlan
//{
//    public Task<Option> Run(PlanContext planContext, ScopeContext context)
//    {
//        T step = planContext.NotNull().Service.GetRequiredService<T>().NotNull();
//        return step.Run(planContext, context);
//    }
//}
