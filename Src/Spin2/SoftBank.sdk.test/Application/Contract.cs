//using FluentAssertions;
//using Orleans.TestingHost;
//using SoftBank.sdk.Models;
//using SpinCluster.sdk.Actors.SoftBank;
//using Toolbox.Block;
//using Toolbox.Types;

//namespace SoftBank.sdk.test.Application;

//public record Contract
//{
//    private readonly ISoftBankActor _softbank;

//    public Contract(TestCluster cluster, ObjectId objectId, string ownerId, params BlockAccess[] access)
//    {
//        ObjectId = objectId;
//        OwnerId = ownerId;
//        Access = access;

//        _softbank = cluster.GrainFactory.GetGrain<ISoftBankActor>(ObjectId);
//    }

//    public ObjectId ObjectId { get; }
//    public string OwnerId { get; }
//    public BlockAccess[] Access { get; }

//    public async Task CreateContract(ScopeContext context)
//    {
//        var request = new AccountDetail
//        {
//            DocumentId = ObjectId,
//            OwnerId = OwnerId,
//            Name = "name1",
//        };

//        await Delete(context);

//        Option createResult = await _softbank.Create(request, context.TraceId);
//        createResult.StatusCode.IsOk().Should().BeTrue(createResult.Error);
//    }

//    public async Task Delete(ScopeContext context) => await _softbank.Delete(context.TraceId);

//    public async Task AddLedgerItem(LedgerItem ledgerItem, ScopeContext context)
//    {
//        var addResponse = await _softbank.AddLedgerItem(ledgerItem, context.TraceId);
//        addResponse.StatusCode.IsOk().Should().BeTrue(addResponse.Error);
//    }
//}