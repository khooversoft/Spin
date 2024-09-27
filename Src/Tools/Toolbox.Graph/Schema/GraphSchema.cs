using System.Collections.Immutable;
using Toolbox.Tools;

namespace Toolbox.Graph;

public interface IGraphSchema<T>
{
    IReadOnlyList<ISchemaValue> SchemaValues { get; }
}

public record GraphSchema<T> : IGraphSchema<T>
{
    public GraphSchema(IEnumerable<ISchemaValue> graphValues) => SchemaValues = graphValues.NotNull().ToImmutableArray();
    public IReadOnlyList<ISchemaValue> SchemaValues { get; }
}

