using Newtonsoft.Json.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types.Id;

namespace SpinCluster.sdk.Actors.Storage;

public class StorageBlobBuilder
{
    public ObjectId? ObjectId { get; set; }
    public string? TypeName { get; set; }
    public byte[]? Content { get; set; }
    public string? Tags { get; set; }

    public StorageBlobBuilder SetObjectId(ObjectId value) => this.Action(x => x.ObjectId = value);
    public StorageBlobBuilder SetTags(params string[] value) => this.Action(x => x.Tags = value.Join(";"));

    public StorageBlobBuilder SetContent(byte[] bytes)
    {
        TypeName = "bytes";
        Content = bytes.NotNull();
        return this;
    }

    public StorageBlobBuilder SetContent(byte[] bytes, string typeName)
    {
        TypeName = typeName.NotEmpty();
        Content = bytes.NotNull();
        return this;
    }

    public StorageBlobBuilder SetContent<T>(T value) where T : class
    {
        value.NotNull();

        TypeName = typeof(T).GetTypeName();
        Content = value.ToJson().ToBytes();
        return this;
    }

    public StorageBlob Build()
    {
        const string msg = "required";
        ObjectId.NotNull(name: msg);
        TypeName.NotEmpty(name: msg);
        Content.NotNull(name: msg);

        return new StorageBlob
        {
            ObjectId = ObjectId,
            TypeName = TypeName,
            Content = Content,
            Tags = Tags,
        }.WithHash();
    }
}