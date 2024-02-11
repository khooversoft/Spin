using NBlog.sdk;

namespace NBlogWeb3.Application;

public static class AppExtensions
{
    public static DbNameId GetDbNameId(this Microsoft.AspNetCore.Components.RouteData subject)
    {
        string dbName = subject.RouteValues.TryGetValue("dbName", out var value) && (value is string v) ? v : "*";
        return new DbNameId(dbName);
    }
}
