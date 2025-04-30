//using Toolbox.Tools.Should;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.DocumentContainer;

//public class DocumentBuilderTests
//{
//    [Fact]
//    public void Document_WhenConstructed_ShouldRoundTrip()
//    {
//        const string documentId = "domain:service/a1";
//        const string payload = "This is the message";

//        var builder = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload);

//        builder.DocumentId!.Id.Be(documentId);
//        builder.Content.NotNull().BytesToString().Be(payload);

//        Document document = builder.Build();

//        document.ObjectId.Be(documentId);
//        document.Content.BytesToString().Be(payload);
//        document.ETag.NotBeNull();

//        document.IsHashVerify().BeTrue();
//        document.Verify();
//    }

//    [Fact]
//    public void Document_WhenSerialized_ShouldRoundTrip()
//    {
//        const string documentId = "domain:service/a1";
//        const string payload = "This is the message";

//        var builder = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload);

//        Document sourceDocument = builder.Build();

//        string json = Json.Default.Serialize(sourceDocument);

//        Document readDocument = Json.Default.Deserialize<Document>(json)!;
//        readDocument.NotBeNull();

//        readDocument.ObjectId.Be(documentId);
//        readDocument.Content.BytesToString().Be(payload);
//        readDocument.ETag.NotBeNull();

//        readDocument.IsHashVerify().BeTrue();
//        readDocument.Verify();
//    }

//    [Fact]
//    public void Document2_WhenConstructed_ShouldRoundTrip()
//    {
//        const string documentId = "domain:service/a1";
//        const string payload = "This is the message";

//        var builder = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload);

//        Document document = builder.Build();

//        document.IsHashVerify().BeTrue();
//        document.Verify();

//        document.ToObject<string>().Be(payload);


//        Document doc2 = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload)
//            .Build()
//            .Verify();

//        (document == doc2).BeTrue();
//    }

//    [Fact]
//    public void DocumentWithProperties_WhenConstructed_ShouldRoundTrip()
//    {
//        const string documentId = "domain:service/a1";
//        const string payload = "This is the message";

//        var builder = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload);

//        Document document = builder.Build();

//        document.IsHashVerify().BeTrue();
//        document.Verify();
//        document.ToObject<string>().Be(payload);

//        Document doc2 = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload)
//            .Build()
//            .Verify();

//        (document == doc2).BeTrue();
//    }

//    [Fact]
//    public void Class_WhenSerialized_ShouldRoundTrip()
//    {
//        const string documentId = "domain:service/a1";

//        var payload = new Payload
//        {
//            Name = "name",
//            Description = "description",
//        };

//        var builder = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload);

//        Document document = builder.Build();

//        document.IsHashVerify().BeTrue();
//        document.Verify();
//        document.ToObject<Payload>().Be(payload);

//        Document doc2 = new DocumentBuilder()
//            .SetDocumentId(documentId.ToObjectId())
//            .SetContent(payload)
//            .Build()
//            .Verify();

//        (document == doc2).BeTrue();
//    }

//    private record Payload
//    {
//        public Guid Id { get; set; } = Guid.NewGuid();
//        public string Name { get; set; } = null!;
//        public string Description { get; set; } = null!;
//        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
//    }
//}