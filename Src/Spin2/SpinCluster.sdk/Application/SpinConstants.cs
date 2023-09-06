namespace SpinCluster.sdk.Application;

public static class SpinConstants
{
    public const string SpinStateStore = "spinStateStore";
    public const string SchemaSearch = "schemaSearch";
    public const string SignValidation = "signValidation";
    public const string Directory = "directory";
    public const string Scheduler = "system:scheduler";

    public const string Folder = "folder";
    public const string Open = "open";
    public const string SystemTenant = "$system";

    public static class Schema
    {
        public const string Kid = "kid";
        public const string Subscription = "subscription";
        public const string Tenant = "tenant";
        public const string User = "user";
        public const string Group = "group";
        public const string Lease = "lease";
        public const string PrincipalKey = "principal-key";
        public const string PrincipalPrivateKey = "principal-private-key";
        public const string Signature = "signature";
        public const string Storage = "storage";
        public const string Contract = "contract";
        public const string Directory = "directory";
        public const string Scheduler = "scheduler";
        public const string Agent = "agent";
        public const string Smartc = "smartc";
    }

    public static class Extension
    {
        public const string Json = ".json";
        public const string BlockStorage = ".block";
    }

    public static class Headers
    {
        public const string TraceId = "spin-trace-id";
        public const string PrincipalId = "spin-principal-id";
    }

    public static class ApiPath
    {
        public const string PrincipalKey = "principalKey";
    }
}
