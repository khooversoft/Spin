using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.DocumentContainer;

public class BlobSerializationTests
{
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public void GivenStringDocument_WillRoundTrip()
    {
        ObjectId objectId = "test/pass/path";
        string payload = "this is the payload";

        BlobPackage blob = new BlobPackageBuilder()
            .SetObjectId(objectId)
            .SetContent(payload)
            .Build();

        blob.Validate().IsOk().Should().BeTrue();
        blob.Should().NotBeNull();
        blob.TypeName.Should().Be("String");
        blob.ToObject<string>().Should().Be(payload);
    }

    [Fact]
    public void GivenClassDocument_WillPass()
    {
        ObjectId objectId = "test/pass/fail";

        var payload = new Payload
        {
            Name = "Name1",
            IntValue = 5,
            DateValue = DateTime.Now,
            FloatValue = 1.5f,
            DecimalValue = 55.23m,
        };

        BlobPackage blob = new BlobPackageBuilder()
            .SetObjectId(objectId)
            .SetContent(payload)
            .Build();

        blob.Validate().IsOk().Should().BeTrue();
        blob.Should().NotBeNull();
        blob.TypeName.Should().Be(typeof(Payload).Name);
        blob.ToObject<Payload>().Should().Be(payload);
    }

    [Fact]
    public void GivenClassDocumentBlob_WillPass()
    {
        ObjectId objectId = "test/pass/path";
        byte[] payload = "this is a test 123".ToBytes();

        BlobPackage blob = new BlobPackageBuilder()
            .SetObjectId(objectId)
            .SetContent(payload)
            .Build();

        blob.Validate().IsOk().Should().BeTrue();
        blob.Should().NotBeNull();
        blob.TypeName.Should().Be(typeof(byte[]).Name);
        blob.ToObject<byte[]>().NotNull().Func(x => Enumerable.SequenceEqual(x, payload)).Should().BeTrue();
    }

    [Fact]
    public void GivenDifferentPayloadTypes()
    {
        ObjectId documentId = "test/pass/failNot";

        Action act = () => new BlobPackageBuilder().SetContent("payload".ToBytes());
        act.Should().NotThrow<ArgumentException>();

        Action act2 = () => new BlobPackageBuilder().SetContent(new[] { 1, 2, 3, 4 });
        act2.Should().NotThrow<ArgumentException>();

        Action act3 = () => new BlobPackageBuilder().SetContent(new[] { 1, 2, 3, 4 }.ToList());
        act3.Should().NotThrow<ArgumentException>();
    }


    private record Payload
    {
        public string Name { get; init; } = null!;
        public int IntValue { get; init; }
        public DateTime DateValue { get; init; }
        public float FloatValue { get; init; }
        public decimal DecimalValue { get; init; }
    }
}
