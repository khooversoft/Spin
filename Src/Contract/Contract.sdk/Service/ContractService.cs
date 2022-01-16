using Artifact.sdk;
using Contract.sdk.Models;
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
    private readonly IArtifactClient _artifactClient;

    public ContractService(IArtifactClient artifactClient)
    {
        _artifactClient = artifactClient;
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

    public async Task Set(DocumentId documentId, BlkBase blkBase, CancellationToken token)
    {
        switch (blkBase)
        {
            case BlkHeader blkHeader:
                break;

            case BlkTransaction blkTransaction:
                break;

            case BlkCode blkCode:
                break;
        }
    }

    public async Task<BatchSet<string>> Search(QueryParameter queryParameter, CancellationToken token)
    {
        BatchSet<string> batch = await _artifactClient.Search(queryParameter).ReadNext(token);
        return batch;
    }

    //private async Task<bool> Create(DocumentId documentId, BlkHeader blkHeader, CancellationToken token)
    //{
    //    BlockChainModel? model = await Get(documentId, token);
    //    if (model != null) return false;


    //}

    //public async Task<bool> Append(DocumentId documentId, BlkTransaction blkTransaction, CancellationToken token)
    //{

    //}

    //public async Task<bool> Append(DocumentId documentId, BlkCode blkCode, CancellationToken token)
    //{

    //}
}
