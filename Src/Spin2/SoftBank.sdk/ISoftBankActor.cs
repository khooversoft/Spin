using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.SoftBank;

public interface ISoftBankActor : IGrainWithStringKey
{
    Task<Option> Create(AccountDetail detail, string traceId);
    Task<Option> Delete(string principalId, string traceId);
    Task<Option> Exist(string traceId);
    Task<Option<AccountDetail>> GetAccountDetail(string principalId, string traceId);
    Task<Option> SetAccountDetail(AccountDetail accountDetail, string traceId);
    Task<Option<AccountBalance>> GetBalance(string principalId, string traceId);
    Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string principalId, string traceId);
    Task<Option> AddLedgerItem(LedgerItem ledgerItem, string traceId);
    Task<Option> SetAcl(BlockAcl blockAcl, string principalId, string traceId);
    Task<Option> Validate(string traceId);
}