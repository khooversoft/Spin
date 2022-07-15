//using Contract.sdk.Options;
//using ContractHost.sdk.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;

//namespace Smart_Installment.sdk;

//public static class DocumentContractClientExtensions
//{
//    public static async Task Create(this DocumentContractClient client, CreateContractOption option, CancellationToken token)
//    {
//        client.NotNull();
//        option.Verify();

//        var contract = new InstallmentContract
//        {
//            //Creator = option.Creator,
//            Description = option.Description,

//            NumPayments = option.NumPayments,
//            Principal = option.Principal,
//            Payment = option.Principal,
//            StartDate = option.StartDate,
//            CompletedDate = option.CompletedDate,

//            Parties = new[]
//            {
//                new PartyRecord
//                {
//                    UserId = option.SellerUserId,
//                    PartyType = "Seller",
//                    BankAccountId = option.SellerBankAccountId
//                },
//                new PartyRecord
//                {
//                    UserId = option.PurchaserUserId,
//                    PartyType = "Purchaser",
//                    BankAccountId = option.PurchaserBankAccountId
//                },
//            }
//        };

//        await client.Create(contract, token);
//    }


//}
