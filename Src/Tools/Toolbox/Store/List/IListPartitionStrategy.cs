//namespace Toolbox.Store;

//public interface IListPartitionStrategy
//{
//    string PathBuilder(string key, string listType);
//    string PathBuilder(string key, string listType, DateTime timeIndex);
//    string SearchBuilder(string key, string? pattern);
//    DateTime ExtractTimeIndex(string path);
//}

//public class DailyPartitionStrategy : IListPartitionStrategy
//{
//    public string PathBuilder(string key, string listType) => PartitionSchemas.ListPath(key, listType, DateTime.UtcNow);
//    public string PathBuilder(string key, string listType, DateTime timeIndex) => PartitionSchemas.ListPath(key, listType, timeIndex);
//    public string SearchBuilder(string key, string? pattern) => PartitionSchemas.ListSearch(key, pattern);
//    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractTimeIndex(path);
//}

//public class SecondPartitionStrategy : IListPartitionStrategy
//{
//    public string PathBuilder(string key, string listType) => PartitionSchemas.ListPathBySeconds(key, listType, DateTime.UtcNow);
//    public string PathBuilder(string key, string listType, DateTime timeIndex) => PartitionSchemas.ListPathBySeconds(key, listType, timeIndex);
//    public string SearchBuilder(string key, string? pattern) => PartitionSchemas.ListSearch(key, pattern);
//    public DateTime ExtractTimeIndex(string path) => PartitionSchemas.ExtractTimeIndex(path);
//}
