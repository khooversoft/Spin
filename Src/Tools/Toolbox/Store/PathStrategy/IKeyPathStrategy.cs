namespace Toolbox.Store;

public interface IKeyPathStrategy
{
    string BasePath { get; }
    bool UseCache { get; }

    string BuildKeySearch(string key);
    string BuildSearch(string pattern);
    string ExtractKey(string path);
    string BuildPath(string key);
    string BuildPath<T>(string key);
    string RemoveBasePath(string path);
}