using System.Collections.Frozen;
using Toolbox.Tools;

namespace NBlog.sdk;

public static class NBlogConstants
{
    public const string DataLakeProviderName = "datalake";
    public const string DirectoryActorKey = "directory.json";
    public const string SearchActorKey = "searchindex.json";
    public const string BadWordsActorKey = "bad-words.sysdata.json";
    public const string OrderBy = "orderBy";

    public const string PackageExtension = ".nblogPackage";
    public const string WordTokenExtension = ".wordTokens.json";
    public const string ConfigurationExtension = ".configuration.json";
    public const string ManifestExtension = ".manifest.json";
    public const string ContactMeExtension = ".contact-me.md";
    public const string AboutExtension = ".about.md";
    public const string SearchExtension = ".searchindex.json";
    public const string SysData = ".sysdata.json";

    public const string SummaryAttribute = "summary";
    public const string MainAttribute = "main";
    public const string ImageAttribute = "image";
    public const string IndexAttribute = "index";

    public const string DefaultDbName = "article";
    public const string ContactRequestFolder = "contact-request";

    public const string LeftMenuStateKey = "left-menu-state";

    public const string DbTag = "db";
    public const string AreaTag = "area";
    public const string HideStyle = "hide";
    public const string KeyHashTag = "keyhash";

    public static FrozenSet<string> ValidThemes = ((string[])["dark", "light"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static FrozenSet<string> RequiredTags = ((string[])["db", "area"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static FrozenSet<string> FilterTags = ((string[])["db"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static FrozenSet<string> FileAttributes = ((string[])["main", "summary", "image"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static FrozenSet<string> SearchAttributes = ((string[])["main", "summary"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    public static FrozenSet<string> IndexAttributes = ((string[])["index"]).ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static class Tool
    {
        public static string CreateConfigurationActorKey(string dbName) => $"{dbName.NotNull().ToLower()}{ConfigurationExtension}";
        public static bool IsConfigurationActorKey(string key) => key.NotNull().EndsWith(ConfigurationExtension);

        public static string CreateSearchIndexActorKey(string dbName) => $"{dbName.NotNull().ToLower()}{SearchExtension}";
        public static bool IsSearchActorKey(string key) => key.NotNull().EndsWith(SearchExtension);

        public static string CreateContactFileId(string dbName) => $"{dbName.NotNull().ToLower()}{ContactMeExtension}";
        public static string CreateAboutFileId(string dbName) => $"{dbName.NotNull().ToLower()}{AboutExtension}";

        public static string CreateContactRequestFileId(string randomTag) =>
            $"{ContactRequestFolder}/{DateTime.UtcNow.ToString("yyyyMM")}/message-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")}-{randomTag.NotEmpty()}.contactRequest.json";
    }
}
