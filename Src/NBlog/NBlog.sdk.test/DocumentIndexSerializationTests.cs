using FluentAssertions;
using Toolbox.DocumentSearch;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace NBlog.sdk.test;

public class DocumentIndexSerializationTests
{
    [Fact]
    public void SimplePayload()
    {
        DocumentReference r1 = new DocumentReference("documentId", [new WordToken("word", 1)], ["tag1"]);

        var json = r1.ToJson();
        json.Should().NotBeNullOrWhiteSpace();

        DocumentReference r2 = json.ToObject<DocumentReference>().NotNull();
        CompareReference(r1, r2);
    }

    [Fact]
    public void SimpleDocumentIndexSerialization()
    {
        var list = new[]
        {
            new DocumentReference("documentId", [new WordToken("word", 1)], ["tag1"]),
            new DocumentReference("documentId2", [new WordToken("word2", 2), new WordToken("word3", 3)], ["tag1"]),
        };

        var document = new DocumentIndexSerialization
        {
            Items = list,
        };

        var json = document.ToJson();
        json.Should().NotBeNullOrWhiteSpace();

        DocumentIndexSerialization r2 = json.ToObject<DocumentIndexSerialization>().NotNull();
        r2.Should().NotBeNull();
        r2.Items.Should().NotBeNull();
        r2.Items.Count.Should().Be(list.Length);

        CompareReference(list[0], r2.Items[0]);
        CompareReference(list[1], r2.Items[1]);
    }

    private void CompareReference(DocumentReference r1, DocumentReference r2)
    {
        r1.DocumentId.Should().Be(r2.DocumentId);
        Enumerable.SequenceEqual(r1.Words, r2.Words).Should().BeTrue();
        Enumerable.SequenceEqual(r1.Tags, r2.Tags).Should().BeTrue();
    }
}
