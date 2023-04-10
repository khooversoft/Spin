using Bank.Abstractions.Model;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Toolbox.Tools;

namespace Bank.Abstractions;

public interface IBankAccountClient
{
    Task<bool> Delete(DocumentId documentId, CancellationToken token = default);
    Task<BankAccount?> Get(DocumentId documentId, CancellationToken token = default);
    BatchSetCursor<DatalakePathItem> Search(QueryParameter query);
    Task Set(BankAccount entry, CancellationToken token = default);
}
