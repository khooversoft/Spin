using FluentAssertions;

namespace Toolbox.Graph.test.Schema;

public class GraphSchemaCommandTests
{

    [Fact]
    public void SetCommands()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d);

        IReadOnlyList<string> matchTo = [
            "upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjoxMX0=' };",
            "upsert node key=index:name1, uniqueIndex;",
            "upsert edge fromKey=index:name1, toKey=data:key1, edgeType=uniqueIndex;",
            "upsert node key=external:name1/11, uniqueIndex;",
            "upsert edge fromKey=external:name1/11, toKey=data:key1, edgeType=uniqueIndex;",
            "upsert edge fromKey=data:key1, toKey=age:11, edgeType=ageGroup;",
            ];

        var nodeCommands = graphCode.BuildSetCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void DeleteCommands()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d);

        IReadOnlyList<string> matchTo = [
            "delete (key=data:key1);",
            "delete (key=index:name1);",
            "delete (key=external:name1/11);",
            "delete [fromKey=data:key1, toKey=age:11, edgeType=ageGroup];",
            ];

        var nodeCommands = graphCode.BuildDeleteCommands();
        Validate(nodeCommands, matchTo);
    }

    private void Validate(IReadOnlyList<string> source, IReadOnlyList<string> matchTo)
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
        public string Name { get; init; } = null!;
        public int Age { get; init; }

        public static IGraphSchema<Data> Schema { get; } = new GraphSchemaBuilder<Data>()
            .Node(x => x.Key, x => $"data:{x}")
            .Index(x => x.Name, x => $"index:{x}")
            .Index(x => x.Name, x => x.Age.ToString(), (x, y) => $"external:{x}/{y}")
            .Reference(x => x.Age, x => $"age:{x}", "ageGroup")
            .Build();
    }
}
