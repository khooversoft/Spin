using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketShare.sdk;

public class TsConstants
{
    public const string DataLakeProviderName = "datalake";
    public const string DirectoryActorKey = "directory.json";

    public const string DataLakeOptionConfigPath = "TicketShare:Identity:Storage";

    public static class Authentication
    {
        public const string ClientId = "TicketShare:Authentication:Microsoft:ClientId";
        public const string ClientSecret = "TicketShare:Authentication:Microsoft:ClientSecret";
    }
}
