﻿//using FluentAssertions;
//using Microsoft.Extensions.Logging.Abstractions;
//using Toolbox.Data;
//using Toolbox.Types;

//namespace Toolbox.Test.DocumentContainer;

//public class DocumentStoreInMemoryTests
//{
//    [Fact]
//    public async Task SingleDocumentRoundtrip()
//    {
//        ObjectId documentId = (ObjectId)"test:document1";

//        var lease = new DocumentObjectLease(new TimeContext(), NullLogger<DocumentObjectLease>.Instance);
//        var store = new InMemoryStore(lease, NullLogger<InMemoryStore>.Instance);
//        ScopeContext context = new ScopeContext(NullLogger.Instance);

//        var payload = new Payload
//        {
//            Name = "Name1",
//            IntValue = 5,
//            DateValue = DateTime.Now,
//            FloatValue = 1.5f,
//            DecimalValue = 55.23m,
//        };

//        Document document = new DocumentBuilder()
//            .SetDocumentId(documentId)
//            .SetContent(payload)
//            .Build()
//            .Verify();

//        document.IsHashVerify().Should().BeTrue();

//        var lookupOption = await store.Get(document.ObjectId);
//        lookupOption.HasValue.Should().BeFalse();
//        lookupOption.StatusCode.Should().Be(StatusCode.NotFound);

//        StatusCode putResult = await store.Set(document, context);
//        putResult.Should().Be(StatusCode.OK);

//        var readOption = await store.Get(document.ObjectId, document.ETag);
//        readOption.HasValue.Should().BeTrue();
//        readOption.StatusCode.Should().Be(StatusCode.OK);

//        Document readDocument = readOption.Return();
//        readDocument.IsHashVerify().Should().BeTrue();

//        (document == readDocument).Should().BeTrue();
//        Payload readPayload = readOption.Return().ToObject<Payload>();

//        (payload == readPayload).Should().BeTrue();

//        var deleteResult = await store.Delete(document.ObjectId, context, eTag: document.ETag);
//        deleteResult.Should().Be(StatusCode.OK);
//    }

//    [Fact]
//    public async Task MultipeDocumentRoundTrip()
//    {
//        const int count = 10;

//        var lease = new DocumentObjectLease(new TimeContext(), NullLogger<DocumentObjectLease>.Instance);
//        var store = new InMemoryStore(lease, NullLogger<InMemoryStore>.Instance);
//        ScopeContext context = new ScopeContext(NullLogger.Instance);

//        var payloads = Enumerable.Range(0, count)
//            .Select(x => new Payload
//            {
//                DocumentId = $"test:document{x}",
//                IntValue = x,
//                DateValue = DateTime.Now.AddDays(x),
//                FloatValue = x,
//                DecimalValue = x,
//            }).ToArray();

//        var documents = payloads
//            .Select(x => new DocumentBuilder()
//                .SetDocumentId((ObjectId)x.DocumentId)
//                .SetContent(x)
//                .Build()
//                .Verify()
//            ).ToArray();

//        foreach (var doc in documents)
//        {
//            var status = await store.Set(doc, context);
//            status.Should().Be(StatusCode.OK);
//        }

//        foreach (var doc in documents)
//        {
//            Option<Document> result = await store.Get(doc.ObjectId, doc.ETag);
//            result.StatusCode.Should().Be(StatusCode.OK);
//            (result.Return() == doc).Should().BeTrue();
//        }
//    }

//    [Fact]
//    public async Task ETagFailDocument()
//    {
//        ObjectId documentId = (ObjectId)"test:document1";

//        var lease = new DocumentObjectLease(new TimeContext(), NullLogger<DocumentObjectLease>.Instance);
//        var store = new InMemoryStore(lease, NullLogger<InMemoryStore>.Instance);
//        ScopeContext context = new ScopeContext(NullLogger.Instance);

//        var payload = new Payload
//        {
//            Name = "Name1",
//            IntValue = 5,
//            DateValue = DateTime.Now,
//            FloatValue = 1.5f,
//            DecimalValue = 55.23m,
//        };

//        Document document = new DocumentBuilder()
//            .SetDocumentId(documentId)
//            .SetContent(payload)
//            .Build()
//            .Verify();

//        document.IsHashVerify().Should().BeTrue();

//        StatusCode putResult = await store.Set(document, context);
//        putResult.Should().Be(StatusCode.OK);

//        var readOption = await store.Get(document.ObjectId, "bad");
//        readOption.HasValue.Should().BeFalse();
//        readOption.StatusCode.Should().Be(StatusCode.Conflict);

//        StatusCode putResult2 = await store.Set(document, context, eTag: "bad");
//        putResult2.Should().Be(StatusCode.Conflict);

//        StatusCode deleteResult = await store.Delete(document.ObjectId, context, eTag: "bad");
//        deleteResult.Should().Be(StatusCode.Conflict);
//    }


//    private record Payload
//    {
//        public string DocumentId { get; init; } = null!;
//        public string Name { get; init; } = null!;
//        public int IntValue { get; init; }
//        public DateTime DateValue { get; init; }
//        public float FloatValue { get; init; }
//        public decimal DecimalValue { get; init; }
//    }
//}
