using Toolbox.Extensions;

namespace SpinPortal.Application;

public static class PortalConstants
{
    public const string NormalText = "text-transform: none";
    public const string GrayBackgroundColor = "#F1F1F1";

    public static class Pages
    {
        public static string TenantPage() => "/data/tenant";
        public static string TenantEditPage(string? objectId = null) => "/tenantEdit" + objectId?.Func(x => $"/{x}");
        public static string UserPage() => "/data/user";
        public static string UserEditPage(string? objectId = null) => objectId != null ? $"/userEdit/{objectId}" : "/userEdit";
        public static string PrincipalKeyPage() => "/data/principalKey";
        public static string PrincipalKeyPage(string? keyId = null) => keyId != null ? $"/data/principalKey/{keyId}" : PrincipalKeyPage();
    }
}
