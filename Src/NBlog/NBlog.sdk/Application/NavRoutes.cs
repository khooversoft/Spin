namespace NBlog.sdk;

public static class NavRoutes
{
    public static string GotoArticle(string dbName, string attribute, string id) => $"/article/{dbName}/{attribute}/{id}";
    public static string GotoSummary(string dbName, string indexName) => $"/summary/{dbName}/{Uri.EscapeDataString(indexName)}";
    public static string GotoSearch(string dbName, string searchText) => $"/search/{dbName}/{Uri.EscapeDataString(searchText)}";
    public static string GotoContact(string dbName) => $"/contact/{dbName}";

    public static string GotoNotFound(string? msg = null) => msg switch
    {
        string v => $"/NotFound/404/{Uri.EscapeDataString(v)}",
        null => "/NotFound/404",
    };
}
