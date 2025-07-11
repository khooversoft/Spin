using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public class StorageBlobBuilder
{
    public string? StorageId { get; set; }
    public byte[]? Content { get; set; }
    public string? ETag { get; set; }

    public StorageBlobBuilder SetStorageId(string value) => this.Action(x => x.StorageId = value);
    public StorageBlobBuilder SetETag(string? value) => this.Action(x => x.ETag = value);

    public StorageBlobBuilder SetContent(ReadOnlySpan<byte> subject)
    {
        Content = subject.ToArray();
        return this;
    }

    public StorageBlobBuilder SetContent<T>(T value) where T : class
    {
        value.NotNull();

        Content = value.ToJson().ToBytes();
        return this;
    }

    public StorageBlobBuilder SetContentFromFile(string file)
    {
        file.NotEmpty();

        var bytes = File.ReadAllBytes(file);
        SetContent(bytes);

        return this;
    }

    public StorageBlob Build()
    {
        const string msg = "required";
        StorageId.NotEmpty(name: msg);
        Content.NotNull(name: msg);

        var blob = new StorageBlob
        {
            StorageId = StorageId,
            Content = Content,
            ETag = ETag,
        };

        return blob with { BlobHash = blob.CalculateHash() };
    }
}