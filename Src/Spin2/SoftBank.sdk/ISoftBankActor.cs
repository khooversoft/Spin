using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Types;


namespace SoftBank.sdk;

public interface ISoftBankActor : IGrainWithStringKey
{
    Task<Option> Delete(string traceId);
    Task<Option> Exist(string traceId);
    Task<Option> Create(AccountDetail detail, string traceId);
    Task<Option> SetAccountDetail(AccountDetail detail, string traceId);
    Task<Option> SetAcl(BlockAcl blockAcl, string principalId, string traceId);
    Task<Option> AddLedgerItem(LedgerItem ledgerItem, string traceId);
    Task<Option<AccountDetail>> GetAccountDetail(string principalId, string traceId);
    Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string principalId, string traceId);
    Task<Option<AccountBalance>> GetBalance(string principalId, string traceId);
    Task<Option<AmountReserved>> Reserve(string principalId, decimal amount, string traceId);
    Task<Option> ReleaseReserve(string leaseKey, string traceId);
}
