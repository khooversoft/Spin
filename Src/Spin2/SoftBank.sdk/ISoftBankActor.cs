using SoftBank.sdk.Models;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.SoftBank
{
    public interface ISoftBankActor : IGrainWithStringKey
    {
        Task<Option> AddLedgerItem(LedgerItem ledgerItem, string traceId);
        Task<Option> Create(AccountDetail detail, string traceId);
        Task<Option> Delete(string traceId);
        Task<Option> Exist(string traceId);
        Task<Option<decimal>> GetBalance(string principalId, string traceId);
        Task<Option<AccountDetail>> GetBankDetails(string principalId, string traceId);
        Task<Option<IReadOnlyList<LedgerItem>>> GetLedgerItems(string principalId, string traceId);
        Task<Option> SetAccountDetail(AccountDetail accountDetail, string traceId);
        Task<Option> Validate(string traceId);
    }
}