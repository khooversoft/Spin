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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjpudWxsfQ==' };",
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
            "delete node ifexist key=data:key1;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjpudWxsfQ==' };",
            "set node key=index:name1 set uniqueIndex;",
            "set edge from=index:name1, to=data:key1, type=uniqueIndex;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUyIiwiYWdlIjpudWxsfQ==' };",
            "delete node ifexist key=index:name1;",
            "set node key=index:name2 set uniqueIndex;",
            "set edge from=index:name2, to=data:key1, type=uniqueIndex;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjo5OX0=' };",
            "set edge from=data:key1, to=age:99, type=ageGroup;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjoyMDV9' };",
            "delete edge ifexist from=data:key1, to=age:99, type=ageGroup;",
            "set edge from=data:key1, to=age:205, type=ageGroup;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6InZhbHVlTmFtZTEiLCJhZ2UiOjk5fQ==' };",
            "set node key=index:valueName1 set uniqueIndex;",
            "set edge from=index:valueName1, to=data:key1, type=uniqueIndex;",
            "set edge from=data:key1, to=age:99, type=ageGroup;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6InZhbHVlTmFtZTIiLCJhZ2UiOjMxMX0=' };",
            "delete node ifexist key=index:valueName1;",
            "set node key=index:valueName2 set uniqueIndex;",
            "set edge from=index:valueName2, to=data:key1, type=uniqueIndex;",
            "delete edge ifexist from=data:key1, to=age:99, type=ageGroup;",
            "set edge from=data:key1, to=age:311, type=ageGroup;",
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
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjoxMX0=' };",
            "delete node ifexist key=index:name1;",
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
            "set node key=index:name2 set uniqueIndex;",
            "set edge from=index:name2, to=data:key1, type=uniqueIndex;",
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
