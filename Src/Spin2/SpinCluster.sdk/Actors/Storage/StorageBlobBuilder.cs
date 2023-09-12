using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;

namespace SpinCluster.sdk.Actors.Storage;

public class StorageBlobBuilder
{
    public string? StorageId { get; set; }
    public string? Path { get; set; }
    public byte[]? Content { get; set; }
    public string? ETag { get; set; }

    public StorageBlobBuilder SetStorageId(string value) => this.Action(x => x.StorageId = value);
    public StorageBlobBuilder SetPath(string value) => this.Action(x => x.Path = value);
    public StorageBlobBuilder SetETag(string? value) => this.Action(x => x.ETag = value);

    public StorageBlobBuilder SetData(byte[] subject)
    {
        subject.NotNull();
        Content = subject.ToArray();
        return this;
    }

    public StorageBlobBuilder SetContent<T>(T value) where T : class
    {
        value.NotNull();

        Content = value.ToJson().ToBytes();
        return this;
    }

    public StorageBlob Build()
    {
        const string msg = "required";
        StorageId.NotEmpty(name: msg);
        Path.NotEmpty(name: msg);
        Content.NotNull(name: msg);

        return new StorageBlob
        {
            StorageId = StorageId,
            Path = Path,
            Content = Content,
            ETag = ETag,
        };
    }
}