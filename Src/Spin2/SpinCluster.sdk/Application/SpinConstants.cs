﻿namespace SpinCluster.sdk.Application;

public static class SpinConstants
{
    public const string SpinStateStore = "spinStateStore";
    public const string SchemaSearch = "schemaSearch";
    public const string Folder = "folder";
    public const string Open = "open";
    public const string SystemTenant = "$system";

    public static class Schema
    {
        public const string Tenant = "tenant";
        public const string User = "user";
        public const string Group = "group";
        public const string Key = "principalKey";
        public const string Storage = "storage";
        public const string Config = "storage";
    }

    public static class Extension
    {
        public const string Tenant = Schema.Tenant + "v1";
        public const string User = Schema.User + "v1";
        public const string Group = Schema.Group + "v1";
        public const string Key = Schema.Key + "v1";
        public const string Storage = Schema.Storage + "v1";
        public const string Config = Schema.Config + "v1";
    }

    public static class Protocol
    {
        public const string TraceId = "spin-trace-id";
    }
}
