using Artifact.sdk;
using Provider.Abstractions;
using Provider.Abstractions.Models;
using Spin.Common.Client;
using SpinNet.sdk.Model;
using Toolbox.Abstractions.Tools;
using Toolbox.Extensions;

namespace InstallmentContract.Provider;

public class ContractService : IProvider
{
    private readonly IArtifactClient _client;
    private readonly ISigningClient _signingClient;

    public ContractService(IArtifactClient client, ISigningClient signingClient)
    {
        _client = client.NotNull();
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
    }
}
