using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Graph.test.Schema;

public class GraphSchemaSimpleTests
{
    [Fact]
    public void Value1Create()
    {
        var d = new Data
        {
            Key = "key1",
        };

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjpudWxsfQ==' };",
            ];

        var nodeCommands = Data.Schema.Code(d).BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void Value1Delete()
    {
        var d = new Data
        {
            Key = "key1",
        };

        IReadOnlyList<string> matchTo = [
            "delete (key=data:key1);",
            ];

        var nodeCommands = Data.Schema.Code(d).BuildDeleteCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void Value2Create()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
        };

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjpudWxsfQ==' };",
            "upsert node key=index:name1, uniqueIndex;",
            "upsert edge fromKey=data:key1, toKey=index:name1, edgeType=uniqueIndex;",
            ];

        var nodeCommands = Data.Schema.Code(d).BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void Value2Update()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
        };

        var d2 = new Data
        {
            Key = "key1",
            Name = "name2",
        };

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUyIiwiYWdlIjpudWxsfQ==' };",
            "delete (key=index:name1);",
            "upsert node key=index:name2, uniqueIndex;",
            "upsert edge fromKey=data:key1, toKey=index:name2, edgeType=uniqueIndex;",
            ];

        var nodeCommands = Data.Schema.Code(d2).SetCurrent(d).BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void Value3Create()
    {
        var d = new Data
        {
            Key = "key1",
            Age = 99,
        };

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjo5OX0=' };",
            "upsert edge fromKey=data:key1, toKey=age:99, edgeType=ageGroup;",
            ];

        var nodeCommands = Data.Schema.Code(d).BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void Value3Update()
    {
        var d = new Data
        {
            Key = "key1",
            Age = 99,
        };

        var d2 = new Data
        {
            Key = "key1",
            Age = 205,
        };

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjoyMDV9' };",
            "delete [fromKey=data:key1, toKey=age:99, edgeType=ageGroup];",
            "upsert edge fromKey=data:key1, toKey=age:205, edgeType=ageGroup;",
            ];

        var nodeCommands = Data.Schema.Code(d2).SetCurrent(d).BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void Value4Create()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "valueName1",
            Age = 99,
        };

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6InZhbHVlTmFtZTEiLCJhZ2UiOjk5fQ==' };",
            "upsert node key=index:valueName1, uniqueIndex;",
            "upsert edge fromKey=data:key1, toKey=index:valueName1, edgeType=uniqueIndex;",
            "upsert edge fromKey=data:key1, toKey=age:99, edgeType=ageGroup;",
            ];

        var nodeCommands = Data.Schema.Code(d).BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void Value4Update()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "valueName1",
            Age = 99,
        };

        var d2 = new Data
        {
            Key = "key1",
            Name = "valueName2",
            Age = 311,
        };

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6InZhbHVlTmFtZTIiLCJhZ2UiOjMxMX0=' };",
            "delete (key=index:valueName1);",
            "upsert node key=index:valueName2, uniqueIndex;",
            "upsert edge fromKey=data:key1, toKey=index:valueName2, edgeType=uniqueIndex;",
            "delete [fromKey=data:key1, toKey=age:99, edgeType=ageGroup];",
            "upsert edge fromKey=data:key1, toKey=age:311, edgeType=ageGroup;",
            ];

        var nodeCommands = Data.Schema.Code(d2).SetCurrent(d).BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void IndexRemoved()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var d1 = new Data
        {
            Key = "key1",
            Name = null,
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d1).SetCurrent(d);

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjoxMX0=' };",
            "delete (key=index:name1);",
            ];

        var nodeCommands = graphCode.BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void ReferenceChange()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var d1 = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 125,
        };

        var graphCode = Data.Schema.Code(d1).SetCurrent(d);

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjoxMjV9' };",
            "delete [fromKey=data:key1, toKey=age:11, edgeType=ageGroup];",
            "upsert edge fromKey=data:key1, toKey=age:125, edgeType=ageGroup;",
            ];

        var nodeCommands = graphCode.BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void IndexChanged()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var d1 = new Data
        {
            Key = "key1",
            Name = "name2",
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d1).SetCurrent(d);

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUyIiwiYWdlIjoxMX0=' };",
            "delete (key=index:name1);",
            "upsert node key=index:name2, uniqueIndex;",
            "upsert edge fromKey=data:key1, toKey=index:name2, edgeType=uniqueIndex;",
            ];

        var nodeCommands = graphCode.BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    private static void Validate(IReadOnlyList<string> source, IReadOnlyList<string> matchTo)
    {
        source.Should().NotBeNull();
        matchTo.Should().NotBeNull();

        source.Count.Should().Be(matchTo.Count);

        for (int i = 0; i < source.Count; i++)
        {
            source[i].Should().Be(matchTo[i]);
        }
    }

    private record Data
    {
        public string Key { get; init; } = null!;
        public string? Name { get; init; } = null!;
        public int? Age { get; init; }

        public static IGraphSchema<Data> Schema { get; } = new GraphSchemaBuilder<Data>()
            .Node(x => x.Key, x => x.IsNotEmpty() ? $"data:{x}" : null)
            .Index(x => x.Name, x => x.IsNotEmpty() ? $"index:{x}" : null)
            .Reference(x => x.Age, x => x > 0 ? $"age:{x}" : null, "ageGroup")
            .Build();
    }
}
