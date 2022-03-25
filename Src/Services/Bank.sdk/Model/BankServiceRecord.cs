using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.sdk.Model;

public class BankServiceRecord
{
    public string? HostUrl { get; set; }

    public string? ApiKey { get; set; }

    public string? QueueId { get; set; }

    public string? Container { get; set; }
}
