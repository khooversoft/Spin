//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Toolbox.Store;

//public interface IFilePartitionStrategy
//{
//    string PathBuilder<T>(string key);
//    string PathBuilder(string key, string typeName);
//    string SearchBuilder(string pattern);
//}

//public class FilePartitionStrategy : IFilePartitionStrategy
//{
//    public string PathBuilder<T>(string key) => PartitionSchemas.HashPath<T>(key);
//    public string PathBuilder(string key, string typeName) => PartitionSchemas.HashPath(key, typeName);
//    public string SearchBuilder(string pattern) => PartitionSchemas.ScalarSearch(pattern);
//}
