using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Store;

// Hash index: {hash}/{hash}/{key}.{typeName}.json
// Key index: {typeName}/{key}.{typeName}.json
// List index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
// ListSecond index: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json


public enum FileSystemType
{
    None,
    Hash,
    Key,
    List
}

public interface IFileSystem
{
    public FileSystemType SystemType { get; init; }
    string PathBuilder<T>(string key);
    string PathBuilder(string key, string listType);
}

public interface IListFileSystem : IFileSystem
{
    string PathBuilder(string key, string listType, DateTime timeIndex);
    string SearchBuilder(string key, string pattern);
    DateTime ExtractTimeIndex(string path);
}

