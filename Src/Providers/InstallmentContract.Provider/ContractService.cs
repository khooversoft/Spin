using System.Net;
using InstallmentContract.Provider.Models;
using Microsoft.Extensions.Logging;
using Provider.Abstractions;
using SpinNet.sdk.Model;
using Toolbox.Block.Application;
using Toolbox.Block.Container;
using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Protocol;
using Toolbox.Sign;
using Toolbox.Store;
using Toolbox.Tools;

namespace InstallmentContract.Provider;

public class ContractService : IProvider
{
    private readonly IBlockDocumentStore _documentStore;
    private readonly ILogger<ContractService> _logger;
    private readonly ISigningClient _signingClient;

    public ContractService(IBlockDocumentStore documentStore, ISigningClient signingClient, ILogger<ContractService> logger)
    {
        _documentStore = documentStore.NotNull();
        _logger = logger.NotNull();
        _signingClient = signingClient;
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

        (NetResponse? contractDetailNotFound, ContractDetails request) = message.FindSingle<ContractDetails>(x => x.IsValid());
        if (contractDetailNotFound != null) return contractDetailNotFound;
        _logger.LogInformation("Creating contract for documentId={documentId}", request.DocumentId);


        if (await _documentStore.Exists(request.DocumentId, token) == false) return new NetResponse
        {
            StatusCode = HttpStatusCode.Conflict,
            Message = $"{request.DocumentId} already exist",
        };


        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(request.PrincipleId)
            .Build()
            .Add(request, request.PrincipleId);

        blockChain = await _signingClient.Sign(blockChain, token);


        Document contract = new DocumentBuilder()
            .SetDocumentId(request.DocumentId)
            .SetPrincipleId(request.PrincipleId)
            .SetContent(blockChain)
            .Build();

        bool success = await _documentStore.Set(contract, token);

        _logger.LogInformation("Completed creating contract for documentId={documentId}, success={success}", request.DocumentId, success);

        return success switch
        {
            true => new NetResponse { StatusCode = HttpStatusCode.OK },
            false => new NetResponse { StatusCode = HttpStatusCode.UnprocessableEntity, Message = $"Could not create documentId={request.DocumentId}" },
        };
    }
}
