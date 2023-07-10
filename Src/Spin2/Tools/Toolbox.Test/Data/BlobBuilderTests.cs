using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Tools.Validation;

namespace Toolbox.Test.DocumentContainer;

public class BlobBuilderTests
{
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public void Blob_WhenConstructed_ShouldRoundTrip()
    {
        const string objectId = "schema/tenant/a1";
        const string payload = "This is the message";

        var builder = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload);

        builder.ObjectId!.Id.Should().Be(objectId);
        builder.Content.NotNull().BytesToString().Should().Be(payload);

        BlobPackage blob = builder.Build();

        blob.ObjectId.Should().Be(objectId);
        blob.Content.BytesToString().Should().Be(payload);
        blob.ETag.Should().NotBeNull();

        blob.IsHashVerify().Should().BeTrue();
        blob.Validate(_context.Location()).ThrowOnError();
    }

    [Fact]
    public void Blob_WhenSerialized_ShouldRoundTrip()
    {
        const string objectId = "schema/tenant/a1";
        const string payload = "This is the message";

        var builder = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload);

        BlobPackage sourceBlob = builder.Build();

        string json = Json.Default.Serialize(sourceBlob);

        BlobPackage readBlob = Json.Default.Deserialize<BlobPackage>(json)!;
        readBlob.Should().NotBeNull();

        readBlob.ObjectId.Should().Be(objectId);
        readBlob.Content.BytesToString().Should().Be(payload);
        readBlob.ETag.Should().NotBeNull();

        readBlob.IsHashVerify().Should().BeTrue();
        readBlob.Validate(_context.Location()).ThrowOnError();
    }

    [Fact]
    public void Blob_2_WhenConstructed_ShouldRoundTrip()
    {
        const string objectId = "schema/tenant/a1";
        const string payload = "This is the message";

        BlobPackage blob = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload)
            .Build();

        blob.IsHashVerify().Should().BeTrue();
        blob.Validate(new ScopeContext(NullLogger.Instance).Location()).ThrowOnError();

        blob.ToObject<string>().Should().Be(payload);

        BlobPackage blob2 = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload)
            .Build()
            .Action(x => x.IsValid(_context.Location()).Should().BeTrue());

        (blob == blob2).Should().BeTrue();
    }

    [Fact]
    public void Blob_WithProperties_WhenConstructed_ShouldRoundTrip()
    {
        const string objectId = "schema/tenant/a1";
        const string payload = "This is the message";

        BlobPackage blob = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload)
            .SetTag("key1", "value1")
            .SetTag("key2")
            .Build();

        blob.IsHashVerify().Should().BeTrue();
        blob.Validate(_context.Location()).IsValid.Should().BeTrue();
        blob.ToObject<string>().Should().Be(payload);

        BlobPackage blob2 = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload)
            .SetTag("key2;key1=value1")
            .Build()
            .Action(x => x.Validate(_context.Location()).ThrowOnError());

        (blob == blob2).Should().BeTrue();
    }

    [Fact]
    public void Class_ShouldRoundTrip()
    {
        const string objectId = "schema/tenant/a1";

        var payload = new Payload
        {
            Name = "name",
            Description = "description",
        };

        BlobPackage blob = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload)
            .Build();

        blob.IsHashVerify().Should().BeTrue();
        blob.IsValid(_context.Location()).Should().BeTrue();
        blob.ToObject<Payload>().Should().Be(payload);

        BlobPackage blob2 = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload)
            .Build()
            .Action(x => x.Validate(_context.Location()).ThrowOnError());

        (blob == blob2).Should().BeTrue();
    }

    [Fact]
    public void WhenSerialized_ShouldRoundTrip()
    {
        const string objectId = "schema/tenant/a1";

        var payload = new Payload
        {
            Name = "name",
            Description = "description",
        };

        BlobPackage blob = new BlobPackageBuilder()
            .SetObjectId(objectId.ToObjectId())
            .SetContent(payload)
            .SetTag("key2=value2;key1=value1")
            .Build();

        blob.IsHashVerify().Should().BeTrue();
        blob.IsValid(_context.Location()).Should().BeTrue();
        blob.ToObject<Payload>().Should().Be(payload);
        blob.Tags.Should().Be("key1=value1;key2=value2");

        string json = Json.Default.Serialize(blob);
        json.Should().NotBeNullOrEmpty();

        BlobPackage blob2 = Json.Default.Deserialize<BlobPackage>(json).NotNull();
        blob2.Should().NotBeNull();

        (blob == blob2).Should().BeTrue();
    }

    private record Payload
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    }
}