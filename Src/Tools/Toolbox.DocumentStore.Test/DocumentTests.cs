using FluentAssertions;
using System.Linq;
using System.Text;
using Toolbox.Abstractions;
using Toolbox.DocumentStore;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.DocumentStore.Test;

public class DocumentTests
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
            .SetData(Encoding.UTF8.GetBytes(payload));

        builder.DocumentId!.Id.Should().Be(documentId);
        builder.ObjectClass!.Should().Be(classObject);
        Encoding.UTF8.GetString(builder.Data!).Should().Be(payload);

        Document document = builder.Build();

        document.DocumentId.Id.Should().Be(documentId);
        document.ObjectClass.Should().Be(classObject);
        Encoding.UTF8.GetString(document.Data).Should().Be(payload);
        document.Hash.Should().NotBeNull();

        document.IsHashVerify().Should().BeTrue();
        document.Verify();

        string readPayload = Encoding.UTF8.GetString(document.Data!);
        readPayload.Should().Be(payload);
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
            .SetData(Encoding.UTF8.GetBytes(payload));

        Document sourceDocument = builder.Build();

        string json = Json.Default.Serialize(sourceDocument);

        Document readDocument = Json.Default.Deserialize<Document>(json)!;
        readDocument.Should().NotBeNull();

        readDocument.DocumentId.Id.Should().Be(documentId);
        readDocument.ObjectClass.Should().Be(classObject);
        Encoding.UTF8.GetString(readDocument.Data).Should().Be(payload);
        readDocument.Hash.Should().NotBeNull();

        readDocument.IsHashVerify().Should().BeTrue();
        readDocument.Verify();

        string readPayload = Encoding.UTF8.GetString(readDocument.Data!);
        readPayload.Should().Be(payload);
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
}
