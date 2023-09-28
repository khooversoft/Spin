using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
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

        var query = new ContractQuery
        {
            PrincipalId = principalId,
            BlockType = typeof(SbAccountDetail).GetTypeName(),
            LatestOnly = true,
        };

        Option<IReadOnlyList<DataBlock>> queryOption = await contract.Query(query, context.TraceId);
        if (queryOption.IsError()) return queryOption.ToOptionStatus<SbAccountDetail>();

        IReadOnlyList<SbAccountDetail> list = queryOption.Return()
            .Select(x => x.ToObject<SbAccountDetail>())
            .ToArray();

        if (list.Count != 1) return StatusCode.NotFound;

        SbAccountDetail accountDetail = list.First();
        return accountDetail;
    }
}
