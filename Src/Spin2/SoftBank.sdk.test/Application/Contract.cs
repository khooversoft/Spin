using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Orleans.TestingHost;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.SoftBank;
using Toolbox.Block;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SoftBank.sdk.test.Application;

public record Contract
{
    private readonly ISoftBankActor _softbank;

    public Contract(TestCluster cluster, ObjectId objectId, PrincipalId ownerId, params BlockAccess[] access)
    {
        ObjectId = objectId;
        OwnerId = ownerId;
        Access = access;

        _softbank = cluster.GrainFactory.GetGrain<ISoftBankActor>(ObjectId);
    }

    public ObjectId ObjectId { get; }
    public PrincipalId OwnerId { get; }
    public BlockAccess[] Access { get; }

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
}