using Provider.Abstractions;
using Provider.Abstractions.Models;
using SpinNet.sdk.Model;
using Toolbox.Sign;
using Toolbox.Tools;
using Toolbox.Store;

namespace InstallmentContract.Provider;

public class ContractService : IProvider
{
    private readonly ISigningClient _signingClient;

    public ContractService(IDocumentStore documentStore, ISigningClient signingClient)
    {
        _signingClient = signingClient;
    }

    public async Task<NetResponse> Post(NetMessage message, CancellationToken token)
    {
        return message.Command switch
        {
            CommandMethod.Create => await CreateContract(message, token),

            _ => throw new InvalidOperationException("Unknown command")
        };
    }

    private async Task<NetResponse> CreateContract(NetMessage message, CancellationToken token)
    {
        CreateContractRequest request = message.GetTypedPayloadSingle<CreateContractRequest>();

        return null;
    }
}
