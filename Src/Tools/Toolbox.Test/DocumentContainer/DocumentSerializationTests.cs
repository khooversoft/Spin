//using Toolbox.Tools.Should;
//using Toolbox.Data;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.DocumentContainer;

//public class DocumentSerializationTests
//{
//    [Fact]
//    public void GivenStringDocument_WillRoundTrip()
//    {
//        var documentId = "test:pass".ToObjectId();
//        string payload = "this is the payload";

//        Document document = new DocumentBuilder()
//            .SetDocumentId(documentId)
//            .SetContent(payload)
//            .Build()
//            .Verify();

//        document.Should().NotBeNull();
//        document.TypeName.Should().Be("String");
//        document.ToObject<string>().Should().Be(payload);
//    }

//    [Fact]
//    public void GivenClassDocument_WillPass()
//    {
//        var documentId = "test:pass".ToObjectId();

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

//        document.Should().NotBeNull();
//        document.TypeName.Should().Be(typeof(Payload).Name);
//        document.ToObject<Payload>().Should().Be(payload);
//    }

//    [Fact]
//    public void GivenClassDocumentBlob_WillPass()
//    {
//        var documentId = "test:pass".ToObjectId();
//        byte[] payload = "this is a test 123".ToBytes();

//        Document document = new DocumentBuilder()
//            .SetDocumentId(documentId)
//            .SetContent(payload)
//            .Build()
//            .Verify();

//        document.Should().NotBeNull();
//        document.TypeName.Should().Be(typeof(byte[]).Name);
//        document.ToObject<byte[]>().NotNull().Func(x => Enumerable.SequenceEqual(x, payload)).Should().BeTrue();
//    }

//    [Fact]
//    public void GivenDifferentPayloadTypes_Invalid_ShouldFail()
//    {
//        var documentId = "test:pass".ToObjectId();

//        Action act = () => new DocumentBuilder().SetContent("payload".ToBytes());
//        act.Should().NotThrow<ArgumentException>();

//        Action act2 = () => new DocumentBuilder().SetContent(new[] { 1, 2, 3, 4 });
//        act2.Should().Throw<ArgumentException>();

//        Action act3 = () => new DocumentBuilder().SetContent(new[] { 1, 2, 3, 4 }.ToList());
//        act3.Should().Throw<ArgumentException>();
//    }


//    private record Payload
//    {
//        public string Name { get; init; } = null!;
//        public int IntValue { get; init; }
//        public DateTime DateValue { get; init; }
//        public float FloatValue { get; init; }
//        public decimal DecimalValue { get; init; }
//    }
//}
