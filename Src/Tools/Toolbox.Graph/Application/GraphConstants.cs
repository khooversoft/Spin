namespace Toolbox.Graph;

public static class GraphConstants
{
    public const string EntityFileIdPrefix = "entity:";
    public const string EntityFileIdSearch = "entity:*";
    public const string EntityName = "entity";

    public const string NodesDataBasePath = "nodes";
    public const string MapDatabasePath = "graphMap/graphMap.gdb.json";
    public const string DbDatabaseSearchPath = "**/*;includeFolder=true";

    public static class TrxJournal
    {
        public const string ConnectionString = "tranLog=/journal/data";
        public const string DiKeyed = "graph-journal";
    }

    public static class Trx
    {
        public const string LogKey = "logKey";
        public const string FileId = "fileId";
        public const string CmType = "cmType";
        public const string NewNode = "new:node";
        public const string CurrentNode = "current:node";
        public const string NewEdge = "new:edge";
        public const string CurrentEdge = "current:edge";
        public const string CurrentData = "current:data";
        public const string NewData = "new:data";

        public const string GiType = "gi:type";
        public const string GiChangeType = "gi:changeType";
        public const string GiData = "gi:data";
    }
}
