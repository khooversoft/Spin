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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjoxMX0=' };",
            "set node key=index:name1 set uniqueIndex;",
            "set edge from=index:name1, to=data:key1, type=uniqueIndex;",
            "set node key=external:name1/11 set uniqueIndex;",
            "set edge from=external:name1/11, to=data:key1, type=uniqueIndex;",
            "set edge from=data:key1, to=age:11, type=ageGroup;",
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
            "delete node ifexist key=data:key1;",
            "delete node ifexist key=index:name1;",
            "delete node ifexist key=external:name1/11;",
            "delete edge ifexist from=data:key1, to=age:11, type=ageGroup;",
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
