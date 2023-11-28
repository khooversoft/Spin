using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SoftBank.sdk.SoftBank;

internal class SoftBank_AccountDetail
{
    private readonly SoftBankActor _parent;
    private readonly ILogger _logger;

    public SoftBank_AccountDetail(SoftBankActor parent, ILogger logger)
    {
        _parent = parent;
        _logger = logger;
    }

    public async Task<Option> SetAccountDetail(SbAccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Set account detail={accountDetail}", detail);

        if (!detail.Validate(out var v)) return v;

        return await _parent.Append(detail, detail.OwnerId, context);
    }

    public async Task<Option<SbAccountDetail>> GetAccountDetail(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting account detail, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        IContractActor contract = _parent.GetContractActor();

        var query = ContractQuery.CreateQuery<SbAccountDetail>(principalId, true);

        Option<ContractQueryResponse> queryOption = await contract.Query(query, context.TraceId);
        if (queryOption.IsError()) return queryOption.ToOptionStatus<SbAccountDetail>();

        Option<SbAccountDetail> response = queryOption.Return().GetSingle<SbAccountDetail>();
        if (response.IsNoContent()) return StatusCode.NotFound;

        return response.Return();
    }
}
