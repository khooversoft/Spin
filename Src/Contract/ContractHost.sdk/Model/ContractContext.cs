using ContractHost.sdk.Host;
using Toolbox.Abstractions.Tools;

namespace ContractHost.sdk.Model;

public record ContractContext
{
    public ContractContext(ContractHostOption contractHostOption, string[] args)
    {
        Option = contractHostOption.Verify();
        Args = args.NotNull();
    }

    public ContractHostOption Option { get; }
    public string[] Args { get; }
}
