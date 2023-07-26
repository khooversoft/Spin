using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftBank.sdk.Models;

public enum SoftBankAccess
{
    None = 0,
    Read = 1,
    Owner = 2,
    Withdraw = 3,
    Deposit = 4,
}
