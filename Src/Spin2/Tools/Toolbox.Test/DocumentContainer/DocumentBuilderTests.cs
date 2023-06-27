using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.DocumentContainer;

public class DocumentBuilderTests
{
    [Fact]
    public void Document_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain:service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload);

        builder.DocumentId!.Id.Should().Be(documentId);
        builder.Content.NotNull().BytesToString().Should().Be(payload);

        Document document = builder.Build();

        ((ObjectId)document.ObjectId).Id.Should().Be(documentId);
        document.Content.BytesToString().Should().Be(payload);
        document.ETag.Should().NotBeNull();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
    }

    [Fact]
    public void Document_WhenSerialized_ShouldRoundTrip()
    {
        const string documentId = "domain:service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload);

        Document sourceDocument = builder.Build();

        string json = Json.Default.Serialize(sourceDocument);

        Document readDocument = Json.Default.Deserialize<Document>(json)!;
        readDocument.Should().NotBeNull();

        ((ObjectId)readDocument.ObjectId).Id.Should().Be(documentId);
        readDocument.Content.BytesToString().Should().Be(payload);
        readDocument.ETag.Should().NotBeNull();

        readDocument.IsHashVerify().Should().BeTrue();
        readDocument.Verify();
    }

    [Fact]
    public void Document2_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain:service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();

        document.ToObject<string>().Should().Be(payload);


        Document doc2 = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload)
            .Build()
            .Verify();

        (document == doc2).Should().BeTrue();
    }

    [Fact]
    public void DocumentWithProperties_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain:service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
        document.ToObject<string>().Should().Be(payload);

        Document doc2 = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload)
            .Build()
            .Verify();

        (document == doc2).Should().BeTrue();
    }

    [Fact]
    public void Class_WhenSerialized_ShouldRoundTrip()
    {
        const string documentId = "domain:service/a1";

        var payload = new Payload
        {
            Name = "name",
            Description = "description",
        };

        var builder = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
        document.ToObject<Payload>().Should().Be(payload);

        Document doc2 = new DocumentBuilder()
            .SetDocumentId((ObjectId)documentId)
            .SetContent(payload)
            .Build()
            .Verify();

        (document == doc2).Should().BeTrue();
    }

    private record Payload
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    }
}