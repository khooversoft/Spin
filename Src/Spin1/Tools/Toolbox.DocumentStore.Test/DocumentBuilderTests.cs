using System;
using FluentAssertions;
using Toolbox.Protocol;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.DocumentStore.Test;

public class DocumentBuilderTests
{
    [Fact]
    public void Document_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetContent(payload);

        builder.DocumentId!.Id.Should().Be(documentId);
        builder.Content.Should().Be(payload);

        Document document = builder.Build();

        ((DocumentId)document.DocumentId).Id.Should().Be(documentId);
        document.Content.Should().Be(payload);
        document.HashBase64.Should().NotBeNull();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
    }
    
    [Fact]
    public void Document_WhenSerialized_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetContent(payload);

        Document sourceDocument = builder.Build();

        string json = Json.Default.Serialize(sourceDocument);

        Document readDocument = Json.Default.Deserialize<Document>(json)!;
        readDocument.Should().NotBeNull();

        ((DocumentId)readDocument.DocumentId).Id.Should().Be(documentId);
        readDocument.Content.Should().Be(payload);
        readDocument.HashBase64.Should().NotBeNull();

        readDocument.IsHashVerify().Should().BeTrue();
        readDocument.Verify();
    }
    
    [Fact]
    public void Document2_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetContent(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();

        document.ToObject<string>().Should().Be(payload);


        Document doc2 = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetContent(payload)
            .Build()
            .Verify();

        (document == doc2).Should().BeTrue();
    }

    [Fact]
    public void DocumentWithProperties_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetContent(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
        document.ToObject<string>().Should().Be(payload);

        Document doc2 = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetContent(payload)
            .Build()
            .Verify();

        (document == doc2).Should().BeTrue();
    }

    [Fact]
    public void Class_WhenSerialized_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";

        var payload = new Payload
        {
            Name = "name",
            Description = "description",
        };

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetContent(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
        document.ToObject<Payload>().Should().Be(payload);

        Document doc2 = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
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
