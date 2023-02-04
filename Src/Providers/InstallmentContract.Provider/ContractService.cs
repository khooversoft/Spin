using Provider.Abstractions;
using Provider.Abstractions.Models;
using SpinNet.sdk.Model;
using Toolbox.Sign;
using Toolbox.Tools;
using Toolbox.Store;
using System.Net;
using Toolbox.Block.Application;
using Toolbox.Block.Container;
using Toolbox.DocumentStore;
using Toolbox.Protocol;
using Microsoft.Extensions.Logging;
using Toolbox.Logging;

namespace InstallmentContract.Provider;

public class ContractService : IProvider
{
    private readonly IDocumentStore _documentStore;
    private readonly ISigningClient _signingClient;
    private readonly ILogger<ContractService> _logger;

    public ContractService(IDocumentStore documentStore, ISigningClient signingClient, ILogger<ContractService> logger)
    {
        _documentStore = documentStore.NotNull();
        _signingClient = signingClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<NetResponse> Post(NetMessage message, CancellationToken token)
    {
        message.NotNull();
        using var ls = _logger.LogEntryExit();
        _logger.LogInformation("Message - Command={command}", message.Command);

        return message.Command switch
        {
            CommandMethod.Create => await CreateContract(message, token),

            _ => throw new InvalidOperationException("Unknown command")
        };
    }

    private async Task<NetResponse> CreateContract(NetMessage message, CancellationToken token)
    {
        using var ls = _logger.LogEntryExit();

        CreateContractRequest request = message.GetTypedPayloadSingle<CreateContractRequest>();
        _logger.LogInformation("Create contract - DocumentId={documentId}", request.DocumentId);

        if (!request.IsValid()) return new NetResponse
        { 
            StatusCode = HttpStatusCode.BadRequest,
            Message = $"{nameof(CreateContractRequest)} message is invalid",
        };

        if( await _documentStore.Exists(request.DocumentId, token) == false) return new NetResponse
        {
            StatusCode = HttpStatusCode.Conflict,
            Message = $"{request.DocumentId} already exist",
        };

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(request.PrincipleId)
            .Build()
            .Add(request, request.PrincipleId);

        Document contract = new DocumentBuilder()
            .SetDocumentId(request.DocumentId)
            .SetPrincipleId(request.PrincipleId)
            .SetData(request)
            .Build();

        await _documentStore.Set(contract, token);

        return new NetResponse { StatusCode = HttpStatusCode.OK };
    }
}
