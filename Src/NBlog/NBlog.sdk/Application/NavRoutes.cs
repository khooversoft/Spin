﻿namespace NBlog.sdk;

public static class NavRoutes
{
    public static string GotoAboutMe() => "/about-me";
    public static string GotoArticle(string dbName, string attribute, string id) => $"/article/{dbName}/{attribute}/{id}";
    public static string GotoSummary(string dbName, string indexName) => $"/summary/{dbName}/{Uri.EscapeDataString(indexName)}";
    public static string GotoSearch(string dbName, string searchText) => $"/search/{dbName}/{Uri.EscapeDataString(searchText)}";
}
