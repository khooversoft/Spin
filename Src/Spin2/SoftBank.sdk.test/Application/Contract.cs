using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestingHost;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.SoftBank;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SoftBank.sdk.test.Application;

public record Contract
{
    private readonly ISoftBankActor _softbank;

    public Contract(TestCluster cluster, string objectId, string ownerId)
    {
        ObjectId = objectId;
        OwnerId = ownerId;
        _softbank = cluster.GrainFactory.GetGrain<ISoftBankActor>(ObjectId);
    }

    public string ObjectId { get; } = null!;
    public string OwnerId { get; } = null!;

    public async Task CreateContract(ScopeContext context)
    {
        var request = new AccountDetail
        {
            ObjectId = ObjectId,
            OwnerId = OwnerId,
            Name = "name1",
        };

        await Delete(context);

        SpinResponse createResult = await _softbank.Create(request, context.TraceId);
        createResult.StatusCode.IsOk().Should().BeTrue(createResult.Error);
    }

    public async Task Delete(ScopeContext context) => await _softbank.Delete(context.TraceId);

    public async Task AddLedgerItem(LedgerItem ledgerItem, ScopeContext context)
    {
        var addResponse = await _softbank.AddLedgerItem(ledgerItem, context.TraceId);
        addResponse.StatusCode.IsOk().Should().BeTrue(addResponse.Error);
    }

    public async Task<decimal> GetBalance(ScopeContext context)
    {
        var balanceOption = await _softbank.GetBalance(context.TraceId);
        balanceOption.StatusCode.IsOk().Should().BeTrue();
        return balanceOption.Return();
    }
}