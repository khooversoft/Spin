namespace SpinPortal.Application;

public static class PortalConstants
{
    public const string NormalText = "text-transform: none";
    public const string GrayBackgroundColor = "#F1F1F1";

    public static class Pages
    {
        public static string TenantPage() => "/tenant";
        public static string TenantEditPage(string? objectId = null) => objectId != null ? $"/tenantEdit/{objectId}" : "/tenantEdit";
        public static string UserPage() => "/user";
        public static string UserEditPage(string? objectId = null) => objectId != null ? $"/userEdit/{objectId}" : "/userEdit";
    }
}
