using SoftBank.sdk.Models;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.SoftBank
{
    public interface ISoftBankActor : IGrainWithStringKey
    {
        Task<SpinResponse> AddLedgerItem(LedgerItem ledgerItem, string traceId);
        Task<SpinResponse> Create(AccountDetail detail, string traceId);
        Task<SpinResponse> Delete(string traceId);
        Task<SpinResponse> Exist(string traceId);
        Task<SpinResponse<decimal>> GetBalance(string traceId);
        Task<SpinResponse<AccountDetail>> GetBankDetails(string traceId);
        Task<SpinResponse<IReadOnlyList<LedgerItem>>> GetLedgerItems(string traceId);
        Task<SpinResponse> SetAccountDetail(AccountDetail accountDetail, string traceId);
        Task<SpinResponse> Validate(string traceId);
    }
}