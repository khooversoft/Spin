using FluentAssertions;
using Toolbox.Extensions;

namespace Toolbox.Graph.test.Schema;

public class GraphSchemaTests
{
    [Fact]
    public void NoValues()
    {
        var d = new Data();

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Node).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Index).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Reference).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Select).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });
    }

    [Fact]
    public void KeyValue()
    {
        var d = new Data
        {
            Key = "key1",
        };

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Node).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x[0].Should().Be("data:key1");
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Index).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Reference).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Select).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x.First().Should().Be("select (key=key1) return entity;");
        });

        Data.Schema.SchemaValues.GetSelectCommand(d).Should().Be("select (key=key1) return entity;");
    }

    [Fact]
    public void IndexWithNoNodeKeyValue()
    {
        var d = new Data
        {
            Name = "name1",
        };

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Node).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Index).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x[0].Should().Be("index:name1");
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Reference).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Select).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);

            x.OrderBy(x => x).Action(y =>
            {
                x[0].Should().Be("select (key=index:name1) -> [uniqueIndex] -> (*) return entity;");
            });
        });

        var select = Data.Schema.SchemaValues.GetSelectCommand(d, "nameIndex");
        select.Should().Be("select (key=index:name1) -> [uniqueIndex] -> (*) return entity;");
    }

    [Fact]
    public void IndexAndReferenceValue()
    {
        var d = new Data
        {
            Name = "name1",
            Age = 19,
        };

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Node).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Index).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(2);
            x[0].Should().Be("index:name1");
            x[1].Should().Be("external:name1/19");
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Reference).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x[0].Should().Be("ageGroup`age:19");
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Select).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(2);

            x.OrderBy(x => x).Action(y =>
            {
                x[0].Should().Be("select (key=index:name1) -> [uniqueIndex] -> (*) return entity;");
                x[1].Should().Be("select (key=external:name1/19) -> [uniqueIndex] -> (*) return entity;");
            });
        });

        var select1 = Data.Schema.SchemaValues.GetSelectCommand(d, "nameIndex");
        select1.Should().Be("select (key=index:name1) -> [uniqueIndex] -> (*) return entity;");

        var select2 = Data.Schema.SchemaValues.GetSelectCommand(d, "externalIndex");
        select2.Should().Be("select (key=external:name1/19) -> [uniqueIndex] -> (*) return entity;");
    }

    [Fact]
    public void ReferenceOnly()
    {
        var d = new Data
        {
            Age = 19,
        };

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Node).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Index).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(0);
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Reference).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x[0].Should().Be("ageGroup`age:19");
        });
    }

    [Fact]
    public void GetSchemaValues()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Node).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x[0].Should().Be("data:key1");
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Index).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(2);
            x[0].Should().Be("index:name1");
            x[1].Should().Be("external:name1/11");
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Reference).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x[0].Should().Be("ageGroup`age:11");
        });
    }

    [Fact]
    public void GetNodeCommands()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d);

        var nodeCommands = graphCode.SetNodeCommand();
        nodeCommands.Should().NotBeNull();
        nodeCommands.Count.Should().Be(1);
        nodeCommands[0].Should().Be("upsert node key=data:key1, entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwiYWdlIjoxMX0=' };");
    }

    [Fact]
    public void GetIndexCommands()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d);

        IReadOnlyList<string> matchTo = [
            "upsert node key=index:name1, uniqueIndex;",
            "upsert edge fromKey=index:name1, toKey=data:key1, edgeType=uniqueIndex;",
            "upsert node key=external:name1/11, uniqueIndex;",
            "upsert edge fromKey=external:name1/11, toKey=data:key1, edgeType=uniqueIndex;",
            ];

        var nodeCommands = graphCode.SetIndexCommands();
        Validate(nodeCommands, matchTo);
    }

    [Fact]
    public void GetReferenceCommands()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            Age = 11,
        };

        var graphCode = Data.Schema.Code(d);

        IReadOnlyList<string> matchTo = [
            "upsert edge fromKey=data:key1, toKey=age:11, edgeType=ageGroup;",
            ];

        var nodeCommands = graphCode.SetReferenceCommands();
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
            .Select(x => x.Key, x => x.IsNotEmpty() ? $"select (key={x}) return entity;" : null)
            .Node(x => x.Key, x => x.IsNotEmpty() ? $"data:{x}" : null)
            .Index(x => x.Name, x => x.IsNotEmpty() ? $"index:{x}" : null)
            .Select(x => x.Name, x => x.IsNotEmpty() ? $"select (key=index:{x}) -> [{GraphConstants.UniqueIndexTag}] -> (*) return entity;" : null, "nameIndex")
            .Index(x => x.Name, x => x.Age.ToString(), (x, y) => (x, y) switch
            {
                (string v1, string v2) when v2 == "0" => null,
                (string v1, string v2) => $"external:{v1}/{v2}",
                _ => null,
            })
            .Select(
                x => x.Name,
                x => x.Age.ToString(),
                (x, y) => (x, y) switch
                {
                    (string v1, string v2) when v2 == "0" => null,
                    (string v1, string v2) => $"select (key=external:{v1}/{v2}) -> [{GraphConstants.UniqueIndexTag}] -> (*) return entity;",
                    _ => null,
                },
                "externalIndex"
            )
            .Reference(x => x.Age, x => x > 0 ? $"age:{x}" : null, "ageGroup")
            .Build();
    }
}
