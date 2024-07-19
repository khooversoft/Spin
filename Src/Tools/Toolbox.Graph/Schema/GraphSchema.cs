using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphSchema<T>
{
    IReadOnlyList<ISchemaValue> SchemaValues { get; }
    Option<T> GetSubject(GraphQueryResult graphQueryResult);
}

public record GraphSchema<T> : IGraphSchema<T>
{
    public GraphSchema(IEnumerable<ISchemaValue> graphValues) => SchemaValues = graphValues.NotNull().ToImmutableArray();
    public IReadOnlyList<ISchemaValue> SchemaValues { get; }

    public Option<T> GetSubject(GraphQueryResult graphQueryResult)
    {
        string dataName = SchemaValues.GetNodeDataName() ?? "entity";
        return graphQueryResult.DataLinks.DataLinkToObject<T>(dataName);
    }
}

