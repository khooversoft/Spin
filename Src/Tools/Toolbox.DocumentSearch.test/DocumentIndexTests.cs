using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentSearch.test;

public class DocumentIndexTests
{
    [Fact]
    public void CreateIndex()
    {
        var index = new DocumentIndexBuilder()
            .SetTokenizer(new DocumentTokenizer(CreateWorkWeight()))
            .Add("doc1", _text)
            .Build();

        index.Index.Count.Be(1);
        index.InvertedIndex.Index.Count.Be(43);
    }

    [Fact]
    public void SimpleQuery()
    {
        var index = new DocumentIndexBuilder()
            .SetTokenizer(new DocumentTokenizer(CreateWorkWeight()))
            .Add("doc1", _text)
            .Build();

        IReadOnlyList<DocumentReference> result = index.Search("Process");
        result.NotNull();
        result.Count.Be(1);
        result[0].DocumentId.Be("doc1");
    }


    [Fact]
    public void NoResultQuery()
    {
        var index = new DocumentIndexBuilder()
            .SetTokenizer(new DocumentTokenizer(CreateWorkWeight()))
            .Add("doc1", _text)
            .Build();

        IReadOnlyList<DocumentReference> result = index.Search("hoover");
        result.NotNull();
        result.Count.Be(0);
    }

    [Fact]
    public void MultipleDocsQuery()
    {
        var index = new DocumentIndexBuilder()
            .SetTokenizer(new DocumentTokenizer(CreateWorkWeight()))
            .Add("doc1", "This is document one")
            .Add("doc2", "This is document two", ["tag2"])
            .Add("doc3", "This is document two and three", ["tag1", "tag2"])
            .Build();

        index.Index.Count.Be(3);
        index.InvertedIndex.Index.Count.Be(6);

        IReadOnlyList<DocumentReference> result = index.Search("this");
        result.NotNull();
        result.Count.Be(0);

        result = index.Search("Document");
        result.Count.Be(3);
        result.Count(x => x.DocumentId == "doc1").Be(1);
        result.Count(x => x.DocumentId == "doc2").Be(1);
        result.Count(x => x.DocumentId == "doc3").Be(1);

        result = index.Search("one");
        result.Count.Be(1);
        result.Count(x => x.DocumentId == "doc1").Be(1);

        result = index.Search("two");
        result.Count.Be(2);
        result.Count(x => x.DocumentId == "doc2").Be(1);
        result.Count(x => x.DocumentId == "doc3").Be(1);

        result = index.Search("three two");
        result.Count.Be(2);
        result.Count(x => x.DocumentId == "doc2").Be(1);
        result.Count(x => x.DocumentId == "doc3").Be(1);

        result = index.Search("tag1");
        result.Count.Be(1);
        result.Count(x => x.DocumentId == "doc3").Be(1);

        result = index.Search("tag2");
        result.Count.Be(2);
        result.Count(x => x.DocumentId == "doc2").Be(1);
        result.Count(x => x.DocumentId == "doc3").Be(1);
    }

    [Fact]
    public void Serialization()
    {
        DocumentTokenizer tokenizer = new DocumentTokenizer(CreateWorkWeight());

        DocumentIndex index = new DocumentIndexBuilder()
            .SetTokenizer(tokenizer)
            .Add("doc1", "This is document one")
            .Add("doc2", "This is document two", ["tag1"])
            .Add("doc3", "This is document two and three", ["tag2", "tag1"])
            .Build();

        index.Index.Count.Be(3);
        index.InvertedIndex.Index.Count.Be(6);

        string json = index.ToJson();
        json.NotEmpty();

        DocumentIndex newIndex = json.ToObject<DocumentIndexSerialization>().NotNull().FromSerialization();
        newIndex.NotNull();
        newIndex.Index.Count.Be(3);
        newIndex.InvertedIndex.Index.Count.Be(6);
    }

    private WordTokenList CreateWorkWeight()
    {
        var words = new WordToken[]
        {
            new WordToken("essential", -1),

            new WordToken("Refactoring", 2),       // Tags
            new WordToken("solid", 1),
            new WordToken("regressions", 1),
            new WordToken("dependencies", 1),
            new WordToken("behavior", 1),
            new WordToken("coverage", 0),
        };

        return new WordTokenList(words);
    }

    private const string _text = """
        Refactoring is the process of improving the structure, readability, and maintainability of existing code without
        changing its external behavior. It's an essential practice in software development to keep codebases clean and efficient over time.

        - <span style="color:MediumSeaGreen">Understand the Code:</span> Before you start refactoring, make sure you have a solid understanding
          of the code you're working with. Analyze its behavior, dependencies, and any potential issues. Ensure you have proper test coverage
          to catch regressions during and after refactoring.
        """;
}
