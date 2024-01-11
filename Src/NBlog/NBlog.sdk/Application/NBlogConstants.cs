using Toolbox.Tools;

namespace NBlog.sdk;

public static class NBlogConstants
{
    public const string DataLakeProviderName = "datalake";
    //public const string ConfigurationActorKey = "nblog-configuration.json";
    public const string DirectoryActorKey = "directory.json";
    public const string SearchActorKey = "searchindex.json";
    public const string CreatedDate = "createdDate";
    public const string ArticleTitle = "articleTitle";

    public const string PackageExtension = ".nblogPackage";
    public const string WordTokenExtension = ".wordTokens.json";
    public const string ConfigurationExtension = ".configuration.json";

    public const string SummaryAttribute = "summary";
    public const string MainAttribute = "main";

    //public const string ToolTag = "Tools";
    public const string NoSummaryTag = "noSummary";
    //public const string FrameworkDesignTag = "FrameworkDesign";

    public const string DbTag = "db";
    public const string AreaTag = "area";

    public static class Tool
    {
        public static string CreateConfigurationActorKey(string db) => $"{db.NotNull().ToLower()}{ConfigurationExtension}";

        public static bool IsConfigurationActorKey(string key) => key.NotNull().EndsWith(ConfigurationExtension);
    }
}
