//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Tools;

//namespace Toolbox.Types.Id;

//public abstract record ObjectIdBase
//{
//    private readonly ParsedObjectId _parsedObjectId;
//    private readonly string? _id = null;
//    private readonly string? _path = null;
//    private readonly bool _requirePath;

//    protected ObjectIdBase(ParsedObjectId id) => _parsedObjectId = id;

//    protected ObjectIdBase(string id, bool requirePath, string syntax)
//    {
//        Option<ParsedObjectId> option = ParsedObjectId.Parse(id, requirePath);
//        option.IsOk().Assert($"Syntax error, syntax={syntax}");

//        _parsedObjectId = option.Return();
//        _requirePath = requirePath;
//    }

//    public void Deconstruct(out string Schema, out string Tenant, out string Path)
//    {
//        Schema = this.Schema;
//        Tenant = this.Schema;
//        Path = this.Path;
//    }

//    public string Id => _id ?? _parsedObjectId.ToString();
//    public string Schema => _parsedObjectId.Schema;
//    public string Tenant => _parsedObjectId.Tentant;
//    public string Path => _path ?? _parsedObjectId.GetPath();
//    public IReadOnlyList<string> Paths => _parsedObjectId.Paths;

//    public bool IsValid(string objectId) => ParsedObjectId.Parse(objectId, _requirePath).IsOk();
//}
