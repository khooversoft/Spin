namespace SpinPortal.Application;


public static class NavTools
{
    public static string ToObjectStorePath(string? path) => path switch
    {
        null => "/objectStore",
        string v => $"/objectStore/{v}",
    };
}
