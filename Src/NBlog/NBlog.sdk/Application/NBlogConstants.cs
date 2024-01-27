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
    public const string ManifestExtension = ".manifest.json";
    public const string ContactMeExtension = ".contact-me.md";
    public const string SearchExtension = ".searchindex.json";

    public const string SummaryAttribute = "summary";
    public const string MainAttribute = "main";
    public const string ImageAttribute = "image";
    public const string DefaultDbName = "article";

    public const string NoSummaryTag = "noSummary";

    public const string DbTag = "db";
    public const string AreaTag = "area";

    public static FrozenSet<string> ValidThemes = ((string[])["dark", "light"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static FrozenSet<string> RequiredTags = ((string[])["db", "area"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static FrozenSet<string> FilterTags = ((string[])["db"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static FrozenSet<string> CanIndexFilesAttributes = ((string[])["main", "summary"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static class TargetName
    {
        public static FrozenSet<string> ValidNames = ((string[])["content", "index"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        public const string Content = "content";
        public const string Index = "index";
    }

    public static class Tool
    {
        public static string CreateConfigurationActorKey(string dbName) => $"{dbName.NotNull().ToLower()}{ConfigurationExtension}";
        public static bool IsConfigurationActorKey(string key) => key.NotNull().EndsWith(ConfigurationExtension);

        public static string CreateSearchIndexActorKey(string dbName) => $"{dbName.NotNull().ToLower()}{SearchExtension}";
        public static bool IsSearchActorKey(string key) => key.NotNull().EndsWith(SearchExtension);

        public static string CreateContactFileId(string dbName) => $"{dbName.NotNull().ToLower()}{ContactMeExtension}";
    }
}
