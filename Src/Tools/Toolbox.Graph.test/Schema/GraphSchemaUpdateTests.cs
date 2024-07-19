using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Graph.test.Schema;

public class GraphSchemaUpdateTests
{
    [Fact]
    public void NoChange()
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
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d1).SetCurrent(d);

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjoxMX0=' };",
            ];

        var nodeCommands = graphCode.BuildSetCommands();
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
            "delete (key=external:name1/11);",
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
            "delete (key=external:name1/11);",
            "upsert node key=external:name1/125, uniqueIndex;",
            "upsert edge fromKey=external:name1/125, toKey=data:key1, edgeType=uniqueIndex;",
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
            "delete (key=external:name1/11);",
            "upsert node key=index:name2, uniqueIndex;",
            "upsert edge fromKey=index:name2, toKey=data:key1, edgeType=uniqueIndex;",
            "upsert node key=external:name2/11, uniqueIndex;",
            "upsert edge fromKey=external:name2/11, toKey=data:key1, edgeType=uniqueIndex;",
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
            .Node(x => x.Key, x => $"data:{x}")
            .Index(x => x.Name, x => x.IsNotEmpty() ? $"index:{x}" : null)
            .Index(x => x.Name, x => x.Age.ToString(), (x, y) => (x, y) switch
            {
                (string name, string age) => $"external:{name}/{age}",
                _ => null,
            })
            .Reference(x => x.Age, x => $"age:{x}", "ageGroup")
            .Build();
    }
}
