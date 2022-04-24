using Artifact.sdk;
using Contract.sdk.Models;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.DocumentStore;
using Toolbox.Model;

namespace ContractApi.Application;

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

    public async Task<BlockChainModel?> Get(DocumentId documentId, CancellationToken token)
    {
        Document? document = await _artifactClient.Get(documentId.WithContainer(_container), token);
        if (document == null) return null;

        BlockChainModel model = document.DeserializeData<BlockChainModel>();
        return model;
    }

    public async Task Set(DocumentId documentId, BlockChainModel blockChain, CancellationToken token)
    {
        Document document = new DocumentBuilder()
            .SetDocumentId(documentId.WithContainer(_container))
            .SetData(blockChain)
            .Build();

        await _artifactClient.Set(document, token);
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token)
    {
        bool status = await _artifactClient.Delete(documentId.WithContainer(_container), token: token);
        return status;
    }

    public async Task<BatchSet<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token)
    {
        queryParameter = queryParameter with { Container = _container };
        BatchSet<DatalakePathItem> batch = await _artifactClient.Search(queryParameter).ReadNext(token);
        return batch;
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // Contract support

    public async Task<bool> Create(BlkHeader blkHeader, CancellationToken token)
    {
        var documentId = new DocumentId(blkHeader.DocumentId);

        BlockChainModel? model = await Get(documentId, token);
        if (model != null) return false;

        BlockChain blockChain = new BlockChainBuilder()
            .SetPrincipleId(blkHeader.PrincipalId)
            .Build();

        blockChain.Add(blkHeader, blkHeader.PrincipalId);

        blockChain = await Sign(blockChain, token);
        if (!blockChain.IsValid()) throw new InvalidOperationException("Blockchain is invalid");

        await Set(documentId, blockChain.ToBlockChainModel(), token);
        return true;
    }

    public async Task<bool> Append(DocumentId documentId, BlkBase blkBase, CancellationToken token)
    {
        BlockChain? blockChain = (await Get(documentId, token))?.ToBlockChain();
        if (blockChain == null) return false;

        switch (blkBase)
        {
            case BlkTransaction blkTransaction:
                blockChain.Add(blkTransaction, blkTransaction.PrincipalId);
                break;

            case BlkCode blkCode:
                blockChain.Add(blkCode, blkCode.PrincipalId);
                break;

            default: throw new ArgumentException($"Unknown type={blkBase.GetType().Name}");
        }

        blockChain = await Sign(blockChain, token);
        await Set(documentId, blockChain.ToBlockChainModel(), token);
        return true;
    }


    // ////////////////////////////////////////////////////////////////////////////////////////////
    // Sign support

    public async Task<BlockChain> Sign(BlockChain blockChain, CancellationToken token)
    {
        SignRequest request = blockChain.GetPrincipleDigests().ToSignRequest();
        if (request.PrincipleDigests.Count == 0) return blockChain;

        SignRequestResponse signedDigests = await _signingClient.Sign(request, token);

        return blockChain.Sign(signedDigests.PrincipleDigests);
    }

    public async Task<bool> Validate(DocumentId documentId, CancellationToken token)
    {
        BlockChain? blockChain = (await Get(documentId, token))?.ToBlockChain();
        if (blockChain == null) return false;

        ValidateRequest request = blockChain.GetPrincipleDigests(onlyUnsighed: false).ToValidateRequest();
        return await _signingClient.Validate(request, token);
    }

    public async Task<bool> Validate(BlockChain blockChain, CancellationToken token)
    {
        ValidateRequest request = blockChain.GetPrincipleDigests(onlyUnsighed: false).ToValidateRequest();
        return await _signingClient.Validate(request, token);
    }
}
