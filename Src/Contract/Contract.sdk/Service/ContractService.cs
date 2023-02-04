using Artifact.sdk;
using Contract.sdk.Models;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Microsoft.Extensions.Logging;
using Spin.Common.Sign;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block.Application;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Block.Signature;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Model;
using Toolbox.Monads;

namespace Contract.sdk.Service;

public class ContractService
{
    private readonly ArtifactClient _artifactClient;
    private readonly SigningClient _signingClient;
    private readonly ILogger<ContractService> _logger;
    private const string _container = "contract";

    public ContractService(ArtifactClient artifactClient, SigningClient signingClient, ILogger<ContractService> logger)
    {
        _artifactClient = artifactClient.NotNull();
        _signingClient = signingClient.NotNull();
        _logger = logger.NotNull();
    }

    // ////////////////////////////////////////////////////////////////////////////////////////////
    // CRUD methods

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token)
    {
        documentId.NotNull();
        var lc = _logger.LogEntryExit();

        bool status = await _artifactClient.Delete(documentId.WithContainer(_container), token: token);
        _logger.LogTrace("Delete documentId={documentId}", documentId);
        return status;
    }

    public async Task<Option<BlockChainModel>> Get(DocumentId documentId, CancellationToken token)
    {
        documentId.NotNull();
        var lc = _logger.LogEntryExit();

        Document? document = await _artifactClient.Get(documentId.WithContainer(_container), token);
        if (document == null) return Option<BlockChainModel>.None;

        BlockChainModel model = document.ToObject<BlockChainModel>();
        _logger.LogTrace("Get documentId={documentId}", documentId);
        return model;
    }

    public async Task<Option<IReadOnlyList<Document>>> Get(DocumentId documentId, string blockTypes, CancellationToken token)
    {
        documentId.NotNull();
        blockTypes.NotEmpty();
        var lc = _logger.LogEntryExit();

        var blockTypeRequests = BlockTypeRequest.Parse(blockTypes);

        var blockChainOption = (await Get(documentId, token)).Bind(x => x.ToBlockChain());

        if (!blockChainOption.HasValue) return Option<IReadOnlyList<Document>>.None;
        var blockChain = blockChainOption.Return();

        _logger.LogTrace("Getting blockTypes{blockTypes} for documentId={documentId}", blockTypes, documentId);

        var result = blockTypeRequests
            .SelectMany(x => x.All ? all(x.BlockType) : latest(x.BlockType))
            .Select(x => x.DataBlock.ToObject<Document>())
            .ToList();

        return result;


        IEnumerable<BlockNode> latest(string bt) => blockChain.Blocks
            .Where(x => x.DataBlock.BlockType == bt)
            .Reverse()
            .Take(1);

        IEnumerable<BlockNode> all(string bt) => blockChain.Blocks
            .Where(x => x.DataBlock.BlockType == bt);
    }

    public async Task<BatchQuerySet<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token)
    {
        queryParameter.NotNull();
        var lc = _logger.LogEntryExit();

        queryParameter = queryParameter with { Container = _container };
        BatchQuerySet<DatalakePathItem> batch = await _artifactClient.Search(queryParameter).ReadNext(token);
        return batch;
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // Contract support

    public async Task<bool> Create(ContractCreateModel contractCreate, CancellationToken token)
    {
        contractCreate.NotNull();
        var lc = _logger.LogEntryExit();

        var documentId = new DocumentId(contractCreate.DocumentId);

        var modelOption = await Get(documentId, token);
        if (modelOption.HasValue) return false;

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(contractCreate.PrincipleId)
            .Build();

        blockChain.Add(contractCreate, contractCreate.PrincipleId);

        blockChain = await Sign(blockChain, token);
        if (!blockChain.IsValid()) throw new InvalidOperationException("Blockchain is invalid");

        await Set(documentId, blockChain.ToBlockChainModel(), token);
        return true;
    }

    public async Task<AppendResult> Append(Batch<Document> batch, CancellationToken token)
    {
        batch.NotNull();
        batch.Items.ForEach(x => x.Verify(true));
        var lc = _logger.LogEntryExit();

        var tracking = new List<(bool Success, string documentId)>();

        var group = batch.Items
            .GroupBy(x => x.DocumentId)
            .ToList();

        foreach (var item in group)
        {
            DocumentId documentId = (DocumentId)item.Key;

            var blockChainOption = (await Get(documentId, token)).Bind(x => x.ToBlockChain());
            if (!blockChainOption.HasValue)
            {
                tracking.Add((false, item.Key));
                continue;
            }

            BlockChain blockChain = blockChainOption.Return();

            foreach (var doc in item)
            {
                blockChain.Add(doc, doc.PrincipleId.NotNull(), doc.ObjectClass);
            }

            blockChain = await Sign(blockChain, token);
            await Set(documentId, blockChain.ToBlockChainModel(), token);

            _logger.LogTrace("Appending count={count} blocks for blockchain documentId={documentId}", item.Count(), documentId);
            tracking.Add((true, item.Key));
        }

        return new AppendResult
        {
            ReferenceId = batch.Id,
            Items = tracking
                .Select(x => new AppendState(x.Success, x.documentId))
                .ToList(),
        };
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // Sign support

    public async Task<BlockChain> Sign(BlockChain blockChain, CancellationToken token)
    {
        blockChain.NotNull();

        SignRequest request = blockChain.GetPrincipleDigests().ToSignRequest();
        if (request.PrincipleDigests.Count == 0) return blockChain;

        SignRequestResponse signedDigests = await _signingClient.Sign(request, token);

        return blockChain.Sign(signedDigests.PrincipleDigests);
    }

    public async Task<bool> Validate(DocumentId documentId, CancellationToken token)
    {
        documentId.NotNull();

        var blockChainOption = (await Get(documentId, token)).Bind(x => x.ToBlockChain());
        if (!blockChainOption.HasValue) return false;

        BlockChain blockChain = blockChainOption.Return();

        ValidateRequest request = blockChain.GetPrincipleDigests(onlyUnsighed: false).ToValidateRequest();
        return await _signingClient.Validate(request, token);
    }

    public async Task<bool> Validate(BlockChain blockChain, CancellationToken token)
    {
        blockChain.NotNull();

        ValidateRequest request = blockChain.GetPrincipleDigests(onlyUnsighed: false).ToValidateRequest();
        return await _signingClient.Validate(request, token);
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    private async Task Set(DocumentId documentId, BlockChainModel blockChain, CancellationToken token)
    {
        documentId.NotNull();
        blockChain.NotNull();

        Document document = new DocumentBuilder()
            .SetDocumentId(documentId.WithContainer(_container))
            .SetData(blockChain)
            .Build();

        await _artifactClient.Set(document, token);
    }
}
