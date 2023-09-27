﻿using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Contract;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SoftBank.sdk.SoftBank;

internal class SoftBank_Ledger
{
    private readonly SoftBankActor _parent;
    private readonly ILogger _logger;

    public SoftBank_Ledger(SoftBankActor parent, ILogger logger)
    {
        _parent = parent;
        _logger = logger;
    }

    public async Task<Option> AddLedgerItem(SbLedgerItem ledgerItem, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Add Ledger item ledgerItem={ledgerItem}", ledgerItem);

        var v = ledgerItem.Validate().LogResult(context.Location());
        if (v.IsError()) return v;

        return await _parent.Append(ledgerItem, ledgerItem.OwnerId, context);
    }

    public async Task<Option<IReadOnlyList<SbLedgerItem>>> GetLedgerItems(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting leger items, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        IContractActor contract = _parent.GetContractActor();

        var query = new ContractQuery
        {
            PrincipalId = principalId,
            BlockType = typeof(SbLedgerItem).GetTypeName(),
        };

        Option<IReadOnlyList<DataBlock>> queryOption = await contract.Query(query, context.TraceId);
        if (queryOption.IsError()) return queryOption.ToOptionStatus<IReadOnlyList<SbLedgerItem>>();

        IReadOnlyList<SbLedgerItem> list = queryOption.Return()
            .Select(x => x.ToObject<SbLedgerItem>())
            .ToArray();

        return list.ToOption();
    }

    public async Task<Option<SbAccountBalance>> GetBalance(string principalId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        context.Location().LogInformation("Getting leger balance, principalId={principalId}", principalId);
        if (!IdPatterns.IsPrincipalId(principalId)) return StatusCode.BadRequest;

        var listOption = await GetLedgerItems(principalId, traceId);
        if (listOption.IsError()) return listOption.ToOptionStatus<SbAccountBalance>();

        var leaseDataOption = await _parent.GetReserveBalance(principalId, traceId);
        if (leaseDataOption.IsError()) return leaseDataOption;

        var response = new SbAccountBalance
        {
            DocumentId = _parent.GetPrimaryKeyString(),
            PrincipalBalance = listOption.Return().Sum(x => x.GetNaturalAmount()),
            ReserveBalance = leaseDataOption.Return().ReserveBalance,
        };

        return response;
    }
}
