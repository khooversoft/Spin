namespace NBlog.sdk;

public static class NavRoutes
{
    public static string GotoAboutMe() => "/about-me";
    public static string GotoArticle(string attribute, string id) => $"/article/{attribute}/{id}";
}
