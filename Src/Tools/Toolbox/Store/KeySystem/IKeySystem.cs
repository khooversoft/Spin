namespace Toolbox.Store;

public enum KeySystemType
{
    None,

    Key,        // {basePath}/{key}
                // {basePath}/{typeName}/{key}.{typeName}.json

    Hash,       // {basePath}/{hash}/{hash}/{key}
                // {basePath}/{hash}/{hash}/{key}.{typeName}.json

    List,       // {basePath}/{listType}/{key}.{listType}.json
}

public interface IKeySystem
{
    string? BasePath { get; }
    KeySystemType SystemType { get; }
    string PathBuilder(string key);
    string PathBuilder<T>(string key);
    string BuildKeySearch(string key);
    string BuildSearch(string pattern);
    string RemovePathPrefix(string path);
    string BuildDeleteFolder(string path);
}
