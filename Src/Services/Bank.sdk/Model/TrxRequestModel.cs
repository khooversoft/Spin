//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Document;
//using Toolbox.Extensions;

//namespace Bank.sdk.Model;

//public record TrxRequestModel
//{
//    public TrxRequest Reference { get; init; } = null!;

//    public BankAccountId FromId { get; init; } = null!;

//    public BankAccountId ToId { get; init; } = null!;
//}


//public static class TrxRequestModelExtensions
//{
//    public static TrxRequestModel? ConvertTo(this TrxRequest trxRequest)
//    {
//        if (trxRequest == null) return null;

//        BankAccountId? fromId = ((DocumentId)trxRequest.FromId).ToBankAccountId();
//        BankAccountId? toId = ((DocumentId)trxRequest.FromId).ToBankAccountId();

//        if (trxRequest.Id.IsEmpty() || fromId == null || toId == null) return null;

//        return new TrxRequestModel
//        {
//            Reference = trxRequest,
//            FromId = fromId,
//            ToId = toId,
//        };
//    }
//}

