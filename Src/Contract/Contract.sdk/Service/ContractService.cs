using Artifact.sdk;
using Contract.sdk.Models;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Contract.sdk.Service;

public class ContractService
{
    private readonly ArtifactClient _artifactClient;
    private readonly SigningClient _signingClient;
    private const string _container = "contract";

    public ContractService(ArtifactClient artifactClient, SigningClient signingClient)
    {
        _artifactClient = artifactClient;
        _signingClient = signingClient;
    }

    // ////////////////////////////////////////////////////////////////////////////////////////////
    // CRUD methods

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token)
    {
        documentId.NotNull();

        bool status = await _artifactClient.Delete(documentId.WithContainer(_container), token: token);
        return status;
    }

    public async Task<BlockChainModel?> Get(DocumentId documentId, CancellationToken token)
    {
        documentId.NotNull();

        Document? document = await _artifactClient.Get(documentId.WithContainer(_container), token);
        if (document == null) return null;

        BlockChainModel model = document.ToObject<BlockChainModel>();
        return model;
    }

    public async Task<Document?> GetLatest(DocumentId documentId, string blockType, CancellationToken token)
    {
        documentId.NotNull();
        blockType.NotEmpty();

        BlockChain? blockChain = (await Get(documentId, token))?.ToBlockChain();
        if (blockChain == null) return null;

        BlockNode? latest = blockChain.Blocks.Where(x => x.DataBlock.BlockType == blockType).LastOrDefault();
        return latest switch
        {
            null => null,
            _ => latest.DataBlock.ToObject<Document>(),
        };
    }

    public async Task<BatchSet<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token)
    {
        queryParameter.NotNull();

        queryParameter = queryParameter with { Container = _container };
        BatchSet<DatalakePathItem> batch = await _artifactClient.Search(queryParameter).ReadNext(token);
        return batch;
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // Contract support

    public async Task<bool> Create(ContractCreateModel contractCreate, CancellationToken token)
    {
        contractCreate.NotNull();

        var documentId = new DocumentId(contractCreate.DocumentId);

        BlockChainModel? model = await Get(documentId, token);
        if (model != null) return false;

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(contractCreate.PrincipleId)
            .Build();

        blockChain.Add(contractCreate, contractCreate.PrincipleId);

        blockChain = await Sign(blockChain, token);
        if (!blockChain.IsValid()) throw new InvalidOperationException("Blockchain is invalid");

        await Set(documentId, blockChain.ToBlockChainModel(), token);
        return true;
    }

    public async Task<bool> Append(Document document, CancellationToken token)
    {
        document.Verify();
        document.PrincipleId.NotEmpty(name: $"{nameof(document.PrincipleId)} is required");

        BlockChain? blockChain = (await Get(document.DocumentId, token))?.ToBlockChain();
        if (blockChain == null) return false;

        blockChain.Add(document, document.PrincipleId, document.ObjectClass);

        blockChain = await Sign(blockChain, token);
        await Set(document.DocumentId, blockChain.ToBlockChainModel(), token);
        return true;
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

        BlockChain? blockChain = (await Get(documentId, token))?.ToBlockChain();
        if (blockChain == null) return false;

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
