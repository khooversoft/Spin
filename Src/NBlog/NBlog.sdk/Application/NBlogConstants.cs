using System.Collections.Frozen;
using Toolbox.Tools;

namespace NBlog.sdk;

public static class NBlogConstants
{
    public const string DataLakeProviderName = "datalake";
    public const string DirectoryActorKey = "directory.json";
    public const string SearchActorKey = "searchindex.json";
    public const string Index = "orderBy";

    public const string PackageExtension = ".nblogPackage";
    public const string WordTokenExtension = ".wordTokens.json";
    public const string ConfigurationExtension = ".configuration.json";

    public const string SummaryAttribute = "summary";
    public const string MainAttribute = "main";
    public const string DefaultDbName = "article";

    public const string NoSummaryTag = "noSummary";

    public const string DbTag = "db";
    public const string AreaTag = "area";

    public static FrozenSet<string> ValidThemes = ((string[])["dark", "light"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static class Tool
    {
        public static string CreateConfigurationActorKey(string db) => $"{db.NotNull().ToLower()}{ConfigurationExtension}";

        public static bool IsConfigurationActorKey(string key) => key.NotNull().EndsWith(ConfigurationExtension);
    }
}
