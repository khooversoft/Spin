using System.Collections;
using System.Diagnostics;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;


public interface IPlan
{
    Task<Option> Run(PlanContext planContext, ScopeContext context);
}

public class Plan : IPlan, IEnumerable<IPlan>
{
    private List<IPlan> _list = new List<IPlan>();

    public Plan(PlanMode mode) => Mode = mode.Assert(x => x.IsEnumValid(), x => $"Invalid PlanMode={x}");

    public PlanMode Mode { get; init; }
    public int Count => _list.Count;
    public IList<IPlan> Items => _list;
    public Plan Add(IPlan plan) => this.Action(x => x._list.Add(plan.NotNull()));
    public Plan Add(Func<PlanContext, ScopeContext, Option> func) => Add(new LambdaPlan(func));
    public Plan AddAsync(Func<PlanContext, ScopeContext, Task<Option>> func) => Add(new LambdaPlanAsync(func));
    public Plan Add<T>() where T : IPlan => Add(new TypedPlan<T>());

    public async Task<Option> Run(PlanContext planContext, ScopeContext context)
    {
        foreach (var item in _list)
        {
            Option option = await item.Run(planContext, context);
            planContext.History.Add(option);

            switch (planContext.Mode)
            {
                case PlanMode.All:
                    if (option.IsError()) return option;
                    break;

                case PlanMode.IgnoreError: break;

                case PlanMode.First:
                    if (option.IsOk()) return StatusCode.OK;
                    break;

                default: throw new UnreachableException($"Unknown mode={planContext.Mode}");
            }
        }

        if (Mode == PlanMode.First) return (StatusCode.Conflict, "No OK for first");
        return StatusCode.OK;
    }

    public IEnumerator<IPlan> GetEnumerator() => ((IEnumerable<IPlan>)_list).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    public static Plan operator +(Plan subject, IPlan plan) => subject.Action(x => x.Add(plan));
}

public static class PlanExtensions
{
    public static async Task<Option<PlanContext>> Run(this Plan plan, IServiceProvider service, ScopeContext context)
    {
        var planContext = new PlanContext
        {
            Mode = plan.Mode,
            Service = service,
        };

        Option option = await plan.Run(planContext, context);
        return new Option<PlanContext>(planContext, option.StatusCode);
    }
}
