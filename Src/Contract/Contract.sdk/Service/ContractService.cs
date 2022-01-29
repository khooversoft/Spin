using Artifact.sdk;
using Contract.sdk.Models;
using Directory.sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Document;
using Toolbox.Model;

namespace Contract.sdk.Service;

public class ContractService
{
    private readonly ArtifactClient _artifactClient;
    private readonly SigningClient _signingClient;

    public ContractService(ArtifactClient artifactClient, SigningClient signingClient)
    {
        _artifactClient = artifactClient;
        _signingClient = signingClient;
    }

    public async Task<bool> Append(DocumentId documentId, BlkBase blkBase, CancellationToken token)
    {
        BlockChain? blockChain = (await Get(documentId, token))?.ConvertTo();
        if (blockChain == null) return false;

        switch (blkBase)
        {
            case BlkTransaction blkTransaction:
                await blockChain.Add(blkTransaction, blkTransaction.PrincipalId, _signingClient, token);
                break;

            case BlkCode blkCode:
                await blockChain.Add(blkCode, blkCode.PrincipalId, _signingClient, token);
                break;

            default: throw new ArgumentException($"Unknown type={blkBase.GetType().Name}");
        }

        var document = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(blockChain.ConvertTo())
            .Build();

        await _artifactClient.Set(document, token);
        return true;
    }

    public async Task<bool> Create(BlkHeader blkHeader, CancellationToken token)
    {
        var documentId = new DocumentId(blkHeader.DocumentId);

        BlockChainModel? model = await Get(documentId, token);
        if (model != null) return false;

        BlockChain blockChain = await new BlockChainBuilder()
            .SetSign(blkHeader.PrincipalId, _signingClient, token)
            .Build();

        await blockChain.Add(blkHeader, blkHeader.PrincipalId, _signingClient, token);

        var document = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(blockChain.ConvertTo())
            .Build();

        await _artifactClient.Set(document, token);

        return true;
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token)
    {
        bool status = await _artifactClient.Delete(documentId, token: token);
        return status;
    }

    public async Task<BlockChainModel?> Get(DocumentId documentId, CancellationToken token)
    {
        Document? document = await _artifactClient.Get(documentId, token);
        if (document == null) return null;

        BlockChainModel model = document.GetData<BlockChainModel>();
        return model;
    }

    public async Task<BatchSet<string>> Search(QueryParameter queryParameter, CancellationToken token)
    {
        BatchSet<string> batch = await _artifactClient.Search(queryParameter).ReadNext(token);
        return batch;
    }

    public async Task<bool> Validate(DocumentId documentId, CancellationToken token)
    {

    }
}
