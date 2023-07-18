namespace SpinCluster.sdk.Application;

public static class SpinConstants
{
    public const string SpinStateStore = "spinStateStore";
    public const string SchemaSearch = "schemaSearch";
    public const string Folder = "folder";
    public const string Open = "open";
    public const string SystemTenant = "$system";

    public static class Schema
    {
        public const string System = "$system";
        public const string Tenant = "tenant";
        public const string User = "user";
        public const string Group = "group";
        public const string Lease = "lease";
        public const string PrincipalKey = "principalKey";
        public const string PrincipalPrivateKey = "principalPrivateKey";
        public const string Signature = "signature";
        public const string Storage = "storage";
        public const string Config = "storage";
        public const string SoftBank = "softbank";
    }

    public static class Extension
    {
        public const string Json = ".json";
        public const string PrivateKeyJson = ".pkjson";
    }

    public static class Protocol
    {
        public const string TraceId = "spin-trace-id";
    }

    public static class ApiPath
    {
        public const string PrincipalKey = "principalKey";
    }
}
