using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SoftBank.sdk.Trx;
using SpinCluster.sdk.Actors.Contract;
using Toolbox.Types;

namespace SoftBank.sdk;

internal class SoftBankManagement
{
    private readonly SoftBankActor _parent;
    private readonly ILogger _logger;

    public SoftBankManagement(SoftBankActor parent, ILogger logger)
    {
        _parent = parent;
        _logger = logger;
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting SoftBank - actorId={actorId}", this._parent.GetPrimaryKeyString());

        var result = await _parent.GetContractActor().Delete(traceId);
        return result;
    }

    public async Task<Option> Exist(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var result = await _parent.GetContractActor().Exist(traceId);
        return result;
    }

    public async Task<Option> Create(AccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating contract accountDetail={accountDetail}", detail);

        var v = detail.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        IContractActor contract = _parent.GetContractActor();

        var createContractRequest = new ContractCreateModel
        {
            DocumentId = _parent.GetSoftBankContractId(),
            PrincipalId = detail.OwnerId,
            BlockAccess = detail.AccessRights.ToArray(),
        };

        var createOption = await contract.Create(createContractRequest, context.TraceId);
        if (createOption.IsError()) return createOption;

        return await _parent.Append(detail, detail.OwnerId, context);
    }
}
