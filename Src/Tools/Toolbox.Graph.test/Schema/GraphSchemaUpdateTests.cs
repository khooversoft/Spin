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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjoxMX0=' };",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjoxMX0=' };",
            "delete node ifexist key=index:name1;",
            "delete node ifexist key=external:name1/11;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjoxMjV9' };",
            "delete node ifexist key=external:name1/11;",
            "set node key=external:name1/125 set uniqueIndex;",
            "set edge from=external:name1/125, to=data:key1, type=uniqueIndex;",
            "delete edge ifexist from=data:key1, to=age:11, type=ageGroup;",
            "set edge from=data:key1, to=age:125, type=ageGroup;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUyIiwiYWdlIjoxMX0=' };",
            "delete node ifexist key=index:name1;",
            "delete node ifexist key=external:name1/11;",
            "set node key=index:name2 set uniqueIndex;",
            "set edge from=index:name2, to=data:key1, type=uniqueIndex;",
            "set node key=external:name2/11 set uniqueIndex;",
            "set edge from=external:name2/11, to=data:key1, type=uniqueIndex;",
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
