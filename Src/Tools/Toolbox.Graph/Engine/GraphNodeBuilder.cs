//using System.Collections.Frozen;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Toolbox.Graph;

//public sealed class GraphNodeBuilder
//{
//    public string Key { get; }
//    public DateTime CreatedDate { get; set; }
//    public GrantCollection Grants { get; set; }

//    private readonly Dictionary<string, string?> _tags;
//    private readonly Dictionary<string, GraphLink> _dataMap;
//    private readonly HashSet<string> _indexes;
//    private readonly Dictionary<string, string?> _foreignKeys;

//    public GraphNodeBuilder(string key)
//    {
//        Key = key.NotNull();
//        CreatedDate = DateTime.UtcNow;
//        Grants = new GrantCollection();

//        _tags = new(StringComparer.OrdinalIgnoreCase);
//        _dataMap = new(StringComparer.OrdinalIgnoreCase);
//        _indexes = new(StringComparer.OrdinalIgnoreCase);
//        _foreignKeys = new(StringComparer.OrdinalIgnoreCase);
//    }

//    public GraphNodeBuilder(GraphNode node)
//    {
//        Key = node.Key;
//        CreatedDate = node.CreatedDate;
//        Grants = node.Grants;

//        _tags = new(node.Tags, StringComparer.OrdinalIgnoreCase);
//        _dataMap = new(node.DataMap, StringComparer.OrdinalIgnoreCase);
//        _indexes = new(node.Indexes, StringComparer.OrdinalIgnoreCase);
//        _foreignKeys = new(node.ForeignKeys, StringComparer.OrdinalIgnoreCase);
//    }

//    // Tag mutations
//    public GraphNodeBuilder SetTag(string key, string? value) { _tags[key] = value; return this; }
//    public GraphNodeBuilder RemoveTag(string key) { _tags.Remove(key); return this; }

//    // DataMap mutations
//    public GraphNodeBuilder SetLink(string name, GraphLink link) { _dataMap[name] = link; return this; }
//    public GraphNodeBuilder RemoveLink(string name) { _dataMap.Remove(name); return this; }
//    public GraphNodeBuilder ClearLinks() { _dataMap.Clear(); return this; }

//    // Index mutations
//    public GraphNodeBuilder AddIndex(string value) { if (!string.IsNullOrWhiteSpace(value)) _indexes.Add(value); return this; }
//    public GraphNodeBuilder RemoveIndex(string value) { _indexes.Remove(value); return this; }

//    // Foreign key mutations
//    public GraphNodeBuilder SetForeignKey(string key, string? value) { _foreignKeys[key] = value; return this; }
//    public GraphNodeBuilder RemoveForeignKey(string key) { _foreignKeys.Remove(key); return this; }

//    public GraphNode Build()
//    {
//        var node = new GraphNode(
//            key: Key,
//            tags: _tags.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
//            createdDate: CreatedDate,
//            dataMap: _dataMap.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
//            indexes: _indexes.ToFrozenSet(StringComparer.OrdinalIgnoreCase),
//            foreignKeys: _foreignKeys.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase)
//        )
//        {
//            Grants = Grants
//        };

//        return node;
//    }
//}