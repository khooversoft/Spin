namespace SpinCluster.sdk.Application;

public static class SpinConstants
{
    public const string SpinStateStore = "spinStateStore";
    public const string SignValidation = "system:signValidation";
    public const string SchedulerActoryKey = "system:scheduler";
    public const string DomainActorKey = "system:domain";
    public const string LeaseActorKey = "system:lease";
    public const string DirectoryActorKey = "system:directory";

    //public const string Folder = "folder";
    //public const string Open = "open";
    //public const string SystemTenant = "$system";

    public static class Schema
    {
        public const string Config = "spinconfig";
        public const string Domain = "domain";
        public const string Kid = "kid";
        public const string Subscription = "subscription";
        public const string Tenant = "tenant";
        public const string User = "user";
        public const string Lease = "lease";
        public const string PrincipalKey = "principal-key";
        public const string PrincipalPrivateKey = "principal-private-key";
        public const string Signature = "signature";
        public const string Storage = "storage";
        public const string Contract = "contract";
        public const string Directory = "directory";
        public const string Scheduler = "scheduler";
        public const string ScheduleWork = "schedulework";
        public const string Agent = "agent";
        public const string Smartc = "smartc";
        public const string Queue = "queue";
    }

    public static class ConfigKeys
    {
        public const string ValidDomainActorKey = "spinconfig:validDomain";
    }

    public static class Ext
    {
        public const string Json = ".json";
        public const string BlockStorage = ".block";
    }

    public static class Headers
    {
        public const string TraceId = "spin-trace-id";
        public const string PrincipalId = "spin-principal-id";
    }

    public static class Dir
    {
        public const string ScheduleWorkQueue = "queue:schedule-work";
    }
}
