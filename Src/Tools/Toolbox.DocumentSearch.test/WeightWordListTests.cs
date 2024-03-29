using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.DocumentSearch.test;

public class WeightWordListTests
{
    [Fact]
    public void Build()
    {
        var words = new WordToken[]
        {
            new WordToken("first", 1),
            new WordToken("second", 2),
            new WordToken("third", 2),
            new WordToken("fourth", 2),
            new WordToken("fifth", 3),
            new WordToken("sixth", 4),
            new WordToken("seventh", 4),
        };

        var list = new WordTokenList(words);
        list.Dictionary.Count.Should().Be(words.Length);
        Enumerable.SequenceEqual(words.OrderBy(x => x.Word), list.Dictionary.Values.OrderBy(x => x.Word));
    }

    [Fact]
    public void CannotFind()
    {
        var list = GetList1();

        var result = list.Lookup(["canotFind"]);
        result.Count.Should().Be(0);
    }

    [Fact]
    public void FindOne()
    {
        var list = GetList1();

        var result = list.Lookup(["fifth"]);
        result.Count.Should().Be(1);
        result[0].Word.Should().Be("fifth");
        result[0].Weight.Should().Be(3);
    }

    [Fact]
    public void FindOneOutOfTwo()
    {
        var list = GetList1();

        var result = list.Lookup(["oops", "sixth"]);
        result.Count.Should().Be(1);
        result[0].Word.Should().Be("sixth");
        result[0].Weight.Should().Be(4);
    }

    [Fact]
    public void FindOTwo()
    {
        var list = GetList1();

        var result = list.Lookup(["second", "sixth"]);
        result.Count.Should().Be(2);

        result[0].Action(x =>
        {
            x.Word.Should().Be("second");
            x.Weight.Should().Be(2);
        });

        result[1].Action(x =>
        {
            x.Word.Should().Be("sixth");
            x.Weight.Should().Be(4);
        });
    }

    [Fact]
    public void SerializationTest()
    {
        var list = GetList1();

        string json = list.ToJson();
        json.Should().NotBeEmpty();

        WordTokenList newList = new WordTokenListBuilder().SetJson(json).Build();

        newList.Dictionary.Count.Should().Be(list.Dictionary.Count);

        Enumerable.SequenceEqual(
            list.Dictionary.Select(x => x.Value).OrderBy(x => x.Word),
            newList.Dictionary.Values.OrderBy(x => x.Word)
            ).Should().BeTrue();
    }

    [Fact]
    public void BuilderTest()
    {
        var list = GetList1();

        string json = list.ToJson();
        json.Should().NotBeEmpty();

        WordTokenList newList = new WordTokenListBuilder()
            .SetJson(json)
            .Add("8", 5)
            .Add("9", 5)
            .Build();

        newList.Dictionary.Count.Should().Be(list.Dictionary.Count + 2);

        var compareTo = list
            .Append(new WordToken("8", 5))
            .Append(new WordToken("9", 5))
            .ToArray();

        Enumerable.SequenceEqual(
            compareTo.OrderBy(x => x.Word),
            newList.Dictionary.Values.OrderBy(x => x.Word)
            ).Should().BeTrue();
    }

    [Fact]
    public void DuplicateValueTest()
    {
        var list = GetList1();

        string json = list.ToJson();
        json.Should().NotBeEmpty();

        WordTokenList newList = new WordTokenListBuilder()
            .SetJson(json)
            .Add("8", 5)
            .Add("8", 5)
            .Build();

        newList.Dictionary.Count.Should().Be(list.Dictionary.Count + 1);

        var compareTo = list
            .Append(new WordToken("8", 5))
            .ToArray();

        Enumerable.SequenceEqual(
            compareTo.OrderBy(x => x.Word),
            newList.Dictionary.Values.OrderBy(x => x.Word)
            ).Should().BeTrue();
    }

    private WordTokenList GetList1()
    {
        var words = new WordToken[]
        {
            new WordToken("first", 1),
            new WordToken("second", 2),
            new WordToken("third", 2),
            new WordToken("fourth", 2),
            new WordToken("fifth", 3),
            new WordToken("sixth", 4),
            new WordToken("seventh", 4),
        };

        return new WordTokenList(words);
    }
}