using Microsoft.AspNetCore.Components;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace RaceAliveWeb.Application;

public static class NavHelper
{
    public static string MarathonCalendarPath => "/marathonCalendar";
    public static string MarathonSearchPath => "/marathonSearch";
    public static string MarathonProfilePath => "/marathonProfile";
    public static string MarathonRankingPath => "/marathonRankings";
    public static string MarathonReviewPath => "/marathonReview";
    public static string MarathonOrganizersPath => "/marathonOrganizers";
    public static string UnderConstructionPath => "/underConstruction";

    public static void GotoCalendar(this NavigationManager nav) => nav.NotNull().NavigateTo(MarathonCalendarPath);

    public static void GotoMarathonProfile(this NavigationManager nav, string id, string? returnUrl = null)
    {
        var url = Build([MarathonProfilePath, id], BuildReturnUrl(returnUrl));
        nav.NotNull().NavigateTo(url);
    }

    public static void GotoMarathonReview(this NavigationManager nav, string id, string? returnUrl = null)
    {
        var url = Build([MarathonReviewPath, id], BuildReturnUrl(returnUrl));
        nav.NotNull().NavigateTo(url);
    }

    private static string? BuildReturnUrl(string? url) => url.IsEmpty() ? null : $"returnUrl={url}";

    private static string Build(IEnumerable<string> paths, string? query) => Build(paths, query.IsNotEmpty() ? [query] : null);

    private static string Build(IEnumerable<string> paths, IEnumerable<string>? queries = null)
    {
        var path = paths.NotNull().Where(x => x.IsNotEmpty()).Join("/").NotEmpty();

        string url = queries switch
        {
            IEnumerable<string> q when q.Any() => $"{path}?{q.Where(x => x.IsNotEmpty()).Join("&")}",
            _ => path,
        };

        return url;
    }
}
