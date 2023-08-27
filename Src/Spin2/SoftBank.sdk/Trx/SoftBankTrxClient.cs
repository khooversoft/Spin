using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace SoftBank.sdk.Trx;

public class SoftBankTrxClient
{
    protected readonly HttpClient _client;
    public SoftBankTrxClient(HttpClient client) => _client = client.NotNull();

    public 
}
