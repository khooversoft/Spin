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
        const string classObject = "ClassObject";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetObjectClass(classObject)
            .SetData(payload);

        builder.DocumentId!.Id.Should().Be(documentId);
        builder.ObjectClass!.Should().Be(classObject);
        builder.Data.Should().Be(payload);

        Document document = builder.Build();

        ((DocumentId)document.DocumentId).Id.Should().Be(documentId);
        document.ObjectClass.Should().Be(classObject);
        document.Data.Should().Be(payload);
        document.Hash.Should().NotBeNull();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
    }
    
    [Fact]
    public void Document_WhenSerialized_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";
        const string classObject = "ClassObject";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetObjectClass(classObject)
            .SetData(payload);

        Document sourceDocument = builder.Build();

        string json = Json.Default.Serialize(sourceDocument);

        Document readDocument = Json.Default.Deserialize<Document>(json)!;
        readDocument.Should().NotBeNull();

        ((DocumentId)readDocument.DocumentId).Id.Should().Be(documentId);
        readDocument.ObjectClass.Should().Be(classObject);
        readDocument.Data.Should().Be(payload);
        readDocument.Hash.Should().NotBeNull();

        readDocument.IsHashVerify().Should().BeTrue();
        readDocument.Verify();
    }
    
    [Fact]
    public void Document2_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";
        const string classObject = "ClassObject";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetObjectClass(classObject)
            .SetData(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();

        document.ToObject<string>().Should().Be(payload);


        Document doc2 = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetObjectClass(classObject)
            .SetData(payload)
            .Build()
            .Verify();

        (document == doc2).Should().BeTrue();
    }

    [Fact]
    public void DocumentWithProperties_WhenConstructed_ShouldRoundTrip()
    {
        const string documentId = "domain/service/a1";
        const string classObject = "ClassObject";
        const string payload = "This is the message";

        var builder = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetObjectClass(classObject)
            .SetData(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
        document.ToObject<string>().Should().Be(payload);

        Document doc2 = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetObjectClass(classObject)
            .SetData(payload)
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
            .SetData(payload);

        Document document = builder.Build();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();
        document.ToObject<Payload>().Should().Be(payload);

        Document doc2 = new DocumentBuilder()
            .SetDocumentId((DocumentId)documentId)
            .SetData(payload)
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
