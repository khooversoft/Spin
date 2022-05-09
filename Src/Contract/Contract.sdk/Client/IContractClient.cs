using Contract.sdk.Models;
using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Model;
using Toolbox.Tools;

namespace Contract.sdk.Client;

public interface IContractClient
{
    Task Append(DocumentId documentId, BlkCode blkCode, CancellationToken token = default);
    Task Append(DocumentId documentId, BlkCollection blkTransaction, CancellationToken token = default);
    Task Create(BlkHeader blkHeader, CancellationToken token = default);
    Task<bool> Delete(DocumentId documentId, CancellationToken token = default);
    Task<BlockChainModel> Get(DocumentId documentId, CancellationToken token = default);
    BatchSetCursor<string> Search(QueryParameter queryParameter);
    Task Set(DocumentId documentId, BlockChainModel blockChainModel, CancellationToken token = default);
    Task<BlockChainModel> Sign(BlockChainModel blockChainModel, CancellationToken token = default);
    Task Sign(DocumentId documentId, BlockChainModel blockChainModel, CancellationToken token = default);
    Task<bool> Validate(BlockChainModel blockChainModel, CancellationToken token = default);
    Task<bool> Validate(DocumentId documentId, CancellationToken token = default);
}
