using System.Collections.Generic;

namespace Directory.sdk
{
    public interface IDirectoryNameService
    {
        Database Default { get; }

        DirectoryNameService Load(string configStore, string environment, IEnumerable<KeyValuePair<string, string>>? properties = null, params string[] configFiles);
        Database Select(string environment);
        Database SelectDefault(string environment);
    }
}