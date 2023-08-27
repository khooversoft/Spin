//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using SoftBank.sdk.Application;
//using SoftBank.sdk.Models;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SoftBank.sdk.Trx;

//internal class ProcessAsSource
//{
//    private readonly SoftBankTrxActor _parent;
//    private readonly IClusterClient _clusterClient;

//    public ProcessAsSource(SoftBankTrxActor parent, IClusterClient clusterClient)
//    {
//        _parent = parent.NotNull();
//        _clusterClient = clusterClient;
//    }

//    public async Task<Option<TrxResponse>> Process(TrxRequest request, ScopeContext context)
//    {
//        var v = request.Validate();
//        if (v.IsError()) return v.ToOptionStatus<TrxResponse>();

//        var response = request.Type switch
//        {
//            TrxType.Push => await Push(request, context),
//            TrxType.Pull => await Pull(request, context),
//            _ => throw new ArgumentException($"Invalid trx type: {request.Type}"),
//        };

//        return response;
//    }

//    private async Task<Option<TrxResponse>> Push(TrxRequest request, ScopeContext context)
//    {
//        var v = request.Validate();
//        if (v.IsError()) return v.LogResult(context.Location()).ToOptionStatus<TrxResponse>();
//        context.Location().LogInformation("Pushing trx, requestId={requestId}", request.Id);

//        // Reserve amount from source account
//        ISoftBankActor sourceAccount = _clusterClient.GetSoftBankActor(request.SourceAccountID);
//        Option<AmountReserved> reserveAmount = await sourceAccount.Reserve(request.PrincipalId, request.Amount, context.TraceId);
//        if (reserveAmount.IsError()) return reserveAmount.LogResult(context.Location()).ToOptionStatus<TrxResponse>();

//        // Pass of transaction to destination's trx actor
//        ISoftBankTrxActor destinationTrx = _parent.GetDesinationTrxActor(request.DestinationAccountId);
//        var trxResponseOption = await destinationTrx.Request(request, context.TraceId);
//        if (trxResponseOption.IsError()) return trxResponseOption.LogResult(context.Location());

//        // Add ledger item to source account for debit
//        var ledgerItem = new LedgerItem
//        {
//            OwnerId = request.PrincipalId,
//            Description = request.Description,
//            Type = LedgerType.Debit,
//            Amount = request.Amount,
//            Tags = new[]
//            {
//                "trx-push",
//                $"DestinationAccountId={request.DestinationAccountId}",
//                $"DestinationLedgerItemId={trxResponseOption.Return().DestinationLedgerItemId}",
//                $"AmountReserved.Id={reserveAmount.Return().Id}"        // Required to release reserve
//            }.Join(';'),
//        };

//        var ledgerAppended = await sourceAccount.AddLedgerItem(ledgerItem, context.TraceId);
//        if (ledgerAppended.IsError()) return ledgerAppended.LogResult(context.Location()).ToOptionStatus<TrxResponse>();

//        TrxResponse trxResponse = trxResponseOption.Return() with { SourceLedgerItemId = ledgerItem.Id };
//        context.Location().LogInformation("Completed push, requestId={requestId}", request.Id);
//        return trxResponse;
//    }

//    private async Task<Option<TrxResponse>> Pull(TrxRequest request, ScopeContext context)
//    {
//        var v = request.Validate();
//        if (v.IsError()) return v.LogResult(context.Location()).ToOptionStatus<TrxResponse>();
//        context.Location().LogInformation("Pulling trx, requestId={requestId}", request.Id);

//        ISoftBankActor sourceAccount = _clusterClient.GetSoftBankActor(request.SourceAccountID);

//        ISoftBankTrxActor destinationTrx = _parent.GetDesinationTrxActor(request.DestinationAccountId);
//        var trxResponseOption = await destinationTrx.Request(request, context.TraceId);
//        if (trxResponseOption.IsError()) return trxResponseOption;

//        var ledgerItem = new LedgerItem
//        {
//            OwnerId = request.PrincipalId,
//            Description = request.Description,
//            Type = LedgerType.Credit,
//            Amount = request.Amount,
//            Tags = new[]
//            {
//                "trx-pull",
//                $"DestinationAccountId={request.DestinationAccountId}",
//                $"DestinationLedgerItemId={trxResponseOption.Return().DestinationLedgerItemId}",
//            }.Join(';'),
//        };

//        var ledgerAppended = await sourceAccount.AddLedgerItem(ledgerItem, context.TraceId);
//        if (ledgerAppended.IsError()) return ledgerAppended.ToOptionStatus<TrxResponse>();

//        TrxResponse trxResponse = trxResponseOption.Return() with { SourceLedgerItemId = ledgerItem.Id };
//        context.Location().LogInformation("Completed push, requestId={requestId}", request.Id);
//        return trxResponse;
//    }
//}
