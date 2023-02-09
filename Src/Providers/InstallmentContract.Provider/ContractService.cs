using System.Diagnostics.Contracts;
using System.Net;
using InstallmentContract.Provider.Models;
using Microsoft.Extensions.Logging;
using Provider.Abstractions;
using SpinNet.sdk.Application;
using SpinNet.sdk.Model;
using Toolbox.Block.Application;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Pattern;
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

    private const string getBalanceDocumentIdKey = "documentId";

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
            CommandMethod.Get when message.ResourceUri.EqualsIgnoreCase("balance") => await GetBalance(message, token),
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
            .SetBlockContent(blockChain)
            .Build();

        bool success = await _documentStore.Set(contract, token);

        _logger.LogInformation("Completed creating contract for documentId={documentId}, success={success}", request.DocumentId, success);

        return success switch
        {
            true => new NetResponse { StatusCode = HttpStatusCode.OK },
            false => new NetResponse { StatusCode = HttpStatusCode.UnprocessableEntity, Message = $"Could not create documentId={request.DocumentId}" },
        };
    }

    private async Task<NetResponse> GetBalance(NetMessage message, CancellationToken token)
    {
        const string documentIdText = "documentId";
        using var ls = _logger.LogEntryExit();

        string? documentId = message.Headers.FindTag(documentIdText);
        if (documentId == null) return new NetResponse { StatusCode = HttpStatusCode.BadRequest, Message = $"{documentIdText} not in headers" };

        (BlockChain blockChain, NetResponse? badResponse) = await GetBlockAndValidate(documentId, token);
        if (badResponse != null) return badResponse;

        _logger.LogInformation("Getting balance for documentId={documentId}", documentId);

        ContractDetails request = blockChain.GetTypedBlocks<ContractDetails>().Last();

        decimal balance = request.GetBalance();

        return new NetResponseBuilder()
            .SetStatusCode(HttpStatusCode.OK)
            .AddContent(new BalanceRecord { Amount = balance })
            .Build();
    }

    private async Task<(BlockChain blockChain, NetResponse? badResponse)> GetBlockAndValidate(string documentId, CancellationToken token)
    {
        Document? contract = await _documentStore.Get(documentId, token);
        if (contract == null)
        {
            _logger.LogError("DocumentId {documentId} not found", documentId);
            return (null!, new NetResponse { StatusCode = HttpStatusCode.NotFound, Message = $"DocumentId={documentId} not found" });
        };

        BlockChain blockChain = contract
            .ToObject<BlockChainModel>()
            .ToBlockChain();

        bool valid = await _signingClient.Validate(blockChain, token);
        if (!valid)
        {
            _logger.LogError("Block chain for DocumentId={documentId} faild validation", documentId);
            return (null!, new NetResponse { StatusCode = HttpStatusCode.UnprocessableEntity, Message = $"Block chain for DocumentId={documentId} faild validation" });
        }

        return (blockChain, null);
    }
}
