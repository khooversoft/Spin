using Toolbox.Tools;

namespace NBlog.sdk;

public static class PackagePaths
{
    public const string ManifestFilesFolder = "manifestfiles/";
    public const string DataFilesFolder = "datafiles/";
    public const string ArticleIndexZipFile = NBlogConstants.DirectoryActorKey;
    public const string ArticleSearchZipFile = NBlogConstants.SearchActorKey;

    public static string GetManifestZipPath(string file) => ManifestFilesFolder + file.NotEmpty();
    public static string GetDatafileZipPath(string file) => DataFilesFolder + file.NotEmpty();
    public static string GetPathArticleIndexZipPath() => ArticleIndexZipFile;
    public static string GetSearchFileZipPath() => ArticleSearchZipFile;

    public static string GetDatalakePath(string path) => path switch
    {
        string v when v.StartsWith(ManifestFilesFolder) => v[ManifestFilesFolder.Length..],
        string v when v.StartsWith(DataFilesFolder) => v[DataFilesFolder.Length..],
        string v when v == (ArticleIndexZipFile) => v,
        string v when v == (ArticleSearchZipFile) => v,

        _ => throw new ArgumentException($"Unknown path or path prefix: {path}")
    };
}
