//using System;
//using System.Linq;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types.Id;

//namespace Toolbox.Types;

///// <summary>
/////
///// /schema/tenant[/path...]
/////
///// </summary>
//public sealed record ObjectUri : ObjectIdBase
//{
//    private ObjectUri(ObjectId id) : base(id) { }

//    public ObjectUri(string id)
//        : base(id, false, Syntax)
//    {
//    }

//    public void Deconstruct(out string Schema, out string? Path)
//    {
//        Schema = this.Schema;
//        Path = this.Path;
//    }

//    public const string Syntax = "{domain}[/{path}...] - domain & path must be alpha numeric or '-' except at the beginning or trailing '-'";

//    public override string ToString() => Id;
//    public bool Equals(ObjectId? other) => other is not null && Id == other.Id;
//    public override int GetHashCode() => HashCode.Combine(Id);

//    public static implicit operator ObjectUri(string documentId) => new ObjectUri(documentId);

//    public static Option<ObjectUri> CreateIfValid(string id)
//    {
//        Option<ObjectId> objectId = ObjectId.Parse(id, true);
//        if (objectId.IsError()) return objectId.ToOption<ObjectUri>();

//        return new ObjectUri(objectId.Return());
//    }
//}

//public static class ObjectUriExtensions
//{
//    public static ObjectUri ToObjectUri(this string? subject) => (ObjectUri)subject!;
//}