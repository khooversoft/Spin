namespace Toolbox.Graph;

public static class GraphConstants
{
    public const string UniqueIndexTag = "uniqueIndex";
    public const string EntityFileIdPrefix = "entity:";
    public const string EntityFileIdSearch = "entity:*";
    public const string EntityName = "entity";

    public const string NodesDataBasePath = "nodes";
    public const string MapDatabasePath = "graphMap/graphMap.gdb.json";

    public static class Trx
    {
        public static string LogKey = "logKey";
        public static string Primarykey = "primaryKey";
        public static string FileId = "fileId";
        public static string ChangeType = "changeType";
        public static string NewNode = "new:node";
        public static string CurrentNode = "current:node";
        public static string NewEdge = "new:edge";
        public static string CurrentEdge = "current:edge";
        public static string CurrentData = "current:data";
        public static string NewData = "new:data";
    }

}
