using Toolbox.Tools;

namespace TicketShare.sdk;


public static class TicketShareTool
{
    public static string ToAccountKey(string id) => $"account:{id.NotEmpty().ToLower()}";
    public static string ToSeasonTicketKey(string id) => $"seasonTicket:{id.NotEmpty().ToLower()}";

    public static string SeasonTicketToIdentity = "seasonTicket-identity-to-identity";
}
