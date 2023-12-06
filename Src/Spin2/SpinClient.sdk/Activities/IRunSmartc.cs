using SpinCluster.abstraction;
using Toolbox.Types;

namespace SpinClient.sdk;

public interface IRunSmartc
{
    Task<Option> Run(ScheduleAssigned scheduleAssigned, bool whatIf, ScopeContext context);
}
