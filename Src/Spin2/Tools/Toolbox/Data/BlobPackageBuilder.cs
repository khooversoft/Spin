using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class BlobPackageBuilder
{
    public ObjectId? ObjectId { get; set; } = null!;
    public string? TypeName { get; set; } = null!;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string? ETag { get; set; } = null!;
    public Tags Tags { get; } = new Tags();

    public BlobPackageBuilder SetObjectId(ObjectId? objectId) => this.Action(x => x.ObjectId = objectId);
    public BlobPackageBuilder SetTypeName(string? typeName) => this.Action(x => x.TypeName = typeName);

    public BlobPackageBuilder SetTag(string? tags) => this.Action(x => x.Tags.Set(tags));
    public BlobPackageBuilder SetTag(string key, string? value) => this.Action(x => x.Tags.Set(key, value));        
    
    public BlobPackageBuilder SetContent<T>(T value) where T : class
    {
        value.NotNull();

        Content = typeof(T) switch
        {
            Type type when type == typeof(string) => value.ToString().NotEmpty().ToBytes(),
            Type type when type == typeof(byte[]) => (byte[])(object)value,

            _ => Json.Default.SerializeDefault(value).ToBytes(),
        };

        TypeName = typeof(T).GetTypeName();
        return this;
    }

    public BlobPackage Build()
    {
        const string msg = "required";
        ObjectId.NotNull(name: msg);
        TypeName.NotEmpty(name: msg);
        Content.NotNull(name: msg);

        return new BlobPackage
        {
            ObjectId = ObjectId.ToString(),
            TypeName = TypeName,
            Content = Content,
            Tags = Tags.ToString(true).ToNullIfEmpty(),
        }.WithHash();
    }
}
