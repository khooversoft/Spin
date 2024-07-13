using System.Collections.Immutable;
using Toolbox.Tools;

namespace Toolbox.Graph;

public interface IGraphSchema<T>
{
    public IReadOnlyList<ISchemaValue<T>> SchemaValues { get; }
}

public record GraphSchema<T> : IGraphSchema<T>
{
    public GraphSchema(IEnumerable<ISchemaValue<T>> graphValues) => SchemaValues = graphValues.NotNull().ToImmutableArray();
    public IReadOnlyList<ISchemaValue<T>> SchemaValues { get; }
}

