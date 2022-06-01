using ContractHost.sdk.Host;
using Toolbox.Tools;

namespace ContractHost.sdk.Model;

public record ContractContext
{
    public ContractContext(ContractHostOption contractHostOption, IReadOnlyList<EventClassRegistry> eventClassRegistries, string[] args)
    {
        Option = contractHostOption.Verify();
        EventClassRegistries = eventClassRegistries.NotNull();
        Args = args.NotNull();
    }

    public ContractHostOption Option { get; }
    public IReadOnlyList<EventClassRegistry> EventClassRegistries { get; }
    public string[] Args { get; }
}
