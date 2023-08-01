using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Models;

namespace SoftBank.sdk.Models;

public enum SoftBankGrant
{
    None = 0,
    Read = 1,
    Owner = 2,          // Owner
    Withdraw = 3,       // Write
    Deposit = 4,        // Write
}


//public static class SoftBankGrantTool
//{
//    public AccessRight CreateGrant(SoftBankGrant grant) => grant switch
//    {
//        SoftBankGrant.Read => new AccessRight { 
//    };
//}