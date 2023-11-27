using Toolbox.Types;

namespace SpinAgent.sdk;

public interface IRunSmartc
{
    Task<Option> Run(WorkSession agentWorkClient, bool whatIf, ScopeContext context);
}
