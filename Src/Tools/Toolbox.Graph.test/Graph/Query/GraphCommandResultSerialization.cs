using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Query;

public class GraphCommandResultSerialization
{
    [Fact]
    public void SimpleSerialization()
    {
        QueryBatchResult source = new QueryBatchResult
        {
            Option = (StatusCode.Conflict, "error"),
            Items = [
                new QueryResult { Option = StatusCode.OK, QueryNumber = 1, Alias = "a1" },
                new QueryResult { Option = (StatusCode.InternalServerError, "oops"), QueryNumber = 2, Alias = "b1" },
                ],
        };

        string json = source.ToJson();

        QueryBatchResult result = json.ToObject<QueryBatchResult>().NotNull();

        result.Option.Assert(x => x == source.Option);
        result.Items.Count.Should().Be(2);

        var cursor = result.Items.ToCursor();
        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Action(x =>
        {
            x.Option.StatusCode.Should().Be(StatusCode.OK);
            x.Option.Error.BeNull();
            x.QueryNumber.Should().Be(1);
            x.Alias.Should().Be("a1");
        });

        cursor.MoveNext().Should().BeTrue();
        cursor.Current.Action(x =>
        {
            x.Option.StatusCode.Should().Be(StatusCode.InternalServerError);
            x.Option.Error.Should().Be("oops");
            x.QueryNumber.Should().Be(2);
            x.Alias.Should().Be("b1");
        });
    }
}
