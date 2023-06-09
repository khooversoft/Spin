using FluentAssertions;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.DocumentContainer;

public class DocumentSerializationTests
{
    [Fact]
    public void GivenStringDocument_WillRoundTrip()
    {
        var documentId = (ObjectId)"test/pass";
        string payload = "this is the payload";

        Document document = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetContent(payload)
            .Build()
            .Validate().Verify();

        document.Should().NotBeNull();
        document.TypeName.Should().Be("String");
        document.Content.Should().Be(payload);

        byte[] bytes = document.ToBytes();

        Document resultDocument = bytes.ToDocument();

        (document == resultDocument).Should().BeTrue();
    }

    [Fact]
    public void GivenClassDocument_WillPass()
    {
        var documentId = (ObjectId)"test/pass";

        var payload = new Payload
        {
            Name = "Name1",
            IntValue = 5,
            DateValue = DateTime.Now,
            FloatValue = 1.5f,
            DecimalValue = 55.23m,
        };

        Document document = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetContent(payload)
            .Build()
            .Validate().Verify();

        document.Should().NotBeNull();
        document.TypeName.Should().Be(typeof(Payload).Name);

        byte[] bytes = document.ToBytes();

        Document resultDocument = bytes.ToDocument();

        (document == resultDocument).Should().BeTrue();
    }

    [Fact]
    public void GivenDifferentPayloadTypes_Invalid_ShouldFail()
    {
        var documentId = (ObjectId)"test/pass";

        Action act = () => new DocumentBuilder().SetContent("payload".ToBytes());
        act.Should().NotThrow<ArgumentException>();

        Action act2 = () => new DocumentBuilder().SetContent(new[] { 1, 2, 3, 4 });
        act2.Should().Throw<ArgumentException>();

        Action act3 = () => new DocumentBuilder().SetContent(new[] { 1, 2, 3, 4 }.ToList());
        act3.Should().Throw<ArgumentException>();
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
