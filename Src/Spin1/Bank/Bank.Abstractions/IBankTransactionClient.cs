using Bank.Abstractions.Model;
using Toolbox.Abstractions;

namespace Bank.Abstractions;

public interface IBankTransactionClient
{
    Task<TrxBalance?> GetBalance(DocumentId documentId, CancellationToken token = default);
    Task<TrxBatch<TrxRequestResponse>> Set(TrxBatch<TrxRequest> batch, CancellationToken token = default);
}
