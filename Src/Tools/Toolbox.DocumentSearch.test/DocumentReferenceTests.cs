using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch.test;

public class DocumentReferenceTests
{
    [Fact]
    public void Serialize()
    {
        var words = new WordToken[]
        {
            new WordToken("essential", -1),
            new WordToken("Refactoring", 2),
            new WordToken("solid", 1),
            new WordToken("regressions", 1),
            new WordToken("dependencies", 1),
            new WordToken("behavior", 1),
            new WordToken("coverage", 0),
        };

        string[] tags = ["tag1", "tag2"];

        var docRef = new DocumentReference("dbName", "doc1", words, tags);

        string json = docRef.ToJson();
        json.Should().NotBeEmpty();

        DocumentReference newDocRef = json.ToObject<DocumentReference>().NotNull();
        newDocRef.DocumentId.Should().Be("doc1");
        Enumerable.SequenceEqual(words, newDocRef.Words).Should().BeTrue();
        Enumerable.SequenceEqual(tags, newDocRef.Tags).Should().BeTrue();
    }
}
