using Bank.Abstractions.Model;

namespace Bank.Abstractions;

public interface IBankClearingClient
{
    Task<TrxBatch<TrxRequestResponse>> Send(TrxBatch<TrxRequest> batch, CancellationToken token = default);
}
