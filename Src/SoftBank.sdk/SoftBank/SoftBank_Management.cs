﻿using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.abstraction;
using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;
using Toolbox.Types;

namespace SoftBank.sdk.SoftBank;

internal class SoftBank_Management
{
    private readonly SoftBankActor _parent;
    private readonly ILogger _logger;

    public SoftBank_Management(SoftBankActor parent, ILogger logger)
    {
        _parent = parent;
        _logger = logger;
    }

    public async Task<Option> Delete(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Deleting SoftBank - actorId={actorId}", _parent.GetPrimaryKeyString());

        var result = await _parent.GetContractActor().Delete(traceId);
        return result;
    }

    public async Task<Option> Exist(string traceId) => await _parent.GetContractActor().Exist(traceId);

    public async Task<Option> Create(SbAccountDetail detail, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Creating contract accountDetail={accountDetail}", detail);

        if (!detail.Validate(out var v)) return v;

        IContractActor contract = _parent.GetContractActor();

        var createContractRequest = new ContractCreateModel
        {
            DocumentId = _parent.GetSoftBankContractId(),
            PrincipalId = detail.OwnerId,
            BlockAccess = [.. detail.AccessRights,
                AccessBlock.Create<SbAccountDetail>(BlockGrant.Read, SoftBankConstants.SoftBankPrincipalId),
                AccessBlock.Create<SbLedgerItem>(BlockGrant.ReadWrite, SoftBankConstants.SoftBankPrincipalId),
            ],
            RoleRights = detail.RoleRights.ToArray(),
        };

        var createOption = await contract.Create(createContractRequest, context.TraceId);
        if (createOption.IsError()) return createOption;

        return await _parent.Append(detail, detail.OwnerId, context);
    }

    public async Task<Option> SetAcl(AclBlock blockAcl, string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Add BlockAcl={blockAcl}", blockAcl);

        if (!blockAcl.Validate(out var v)) return v;

        return await _parent.Append(blockAcl, principalId, context);
    }
}
