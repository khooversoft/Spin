namespace TicketShare.sdk;

public class TsConstants
{
    public const string ConfigurationFilter = "TicketShare:*";
    public const string StorageOptionConfigPath = "TicketShare:Storage";
    public const string StorageAccountConnection = "TicketShare:Storage:AccountConnection";
    public const string StorageCredential = "TicketShare:Storage:Credentials";

    public static class Authentication
    {
        public const string ClientId = "TicketShare:Authentication:Microsoft:ClientId";
        public const string ClientSecret = "TicketShare:Authentication:Microsoft:ClientSecret";
    }
}
