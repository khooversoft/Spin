using System.Linq.Expressions;
using Toolbox.Tools;

namespace Toolbox.Graph;


public class GraphSchemaBuilder<T>
{
    private readonly List<ISchemaValue> _graphCodes = new List<ISchemaValue>();

    public GraphSchemaBuilder<T> DataName(string dataName)
    {
        _graphCodes.Add(new SchemaConstant { Type = SchemaType.DataName, Attribute = dataName });
        return this;
    }

    public GraphSchemaBuilder<T> Node<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format) =>
        AddExpression(expression, SchemaType.Node, format);

    public GraphSchemaBuilder<T> Tag<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format) =>
        AddExpression(expression, SchemaType.Tags, format);

    public GraphSchemaBuilder<T> Index<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format) =>
        AddExpression(expression, SchemaType.Index, format);

    public GraphSchemaBuilder<T> Index<TProperty>(Expression<Func<T, TProperty>> expr1, Expression<Func<T, TProperty>> expr2, Func<TProperty, TProperty, string?> format) =>
        AddExpression(expr1, expr2, SchemaType.Index, format);

    public GraphSchemaBuilder<T> Reference<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format, string edgeType) =>
        AddExpression(expression, SchemaType.Reference, format, edgeType);

    public GraphSchemaBuilder<T> ReferenceCollection<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression, Func<TProperty, string?> format, string attribute) =>
        AddExpression(expression, SchemaType.Reference, format, attribute);

    public GraphSchemaBuilder<T> Select<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format, string queryName = "default") =>
        AddExpression(expression, SchemaType.Select, format, queryName);

    public GraphSchemaBuilder<T> Select<TProperty>(
        Expression<Func<T, TProperty>> expr1,
        Expression<Func<T, TProperty>> expr2,
        Func<TProperty, TProperty, string?> format,
        string queryName = "default"
        ) => AddExpression(expr1, expr2, SchemaType.Select, format, queryName);

    public IGraphSchema<T> Build() => new GraphSchema<T>(_graphCodes);


    private GraphSchemaBuilder<T> AddExpression<TProperty>(Expression<Func<T, TProperty>> expression, SchemaType valueType, Func<TProperty, string?> format, string? attribute = null)
    {
        Func<T, TProperty> propertyFunc = expression.NotNull().Compile();

        var value = new SchemaValue<T, TProperty>
        {
            Type = valueType,
            GetSourceValue = propertyFunc,
            FormatValue = format.NotNull(),
            Attribute = attribute,
        };

        _graphCodes.Add(value);
        return this;
    }

    private GraphSchemaBuilder<T> AddExpression<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression, SchemaType valueType, Func<TProperty, string?> format, string? attribute = null)
    {
        Func<T, IEnumerable<TProperty>> propertyFunc = expression.NotNull().Compile();

        var value = new SchemaValues<T, TProperty>
        {
            Type = valueType,
            GetSourceValues = propertyFunc,
            FormatValue = format.NotNull(),
            Attribute = attribute,
        };

        _graphCodes.Add(value);
        return this;
    }

    private GraphSchemaBuilder<T> AddExpression<TProperty>(
        Expression<Func<T, TProperty>> expr1,
        Expression<Func<T, TProperty>> expr2,
        SchemaType valueType,
        Func<TProperty, TProperty, string?> format,
        string? attribute = null)
    {
        Func<T, TProperty> propertyFunc1 = expr1.NotNull().Compile();
        Func<T, TProperty> propertyFunc2 = expr2.NotNull().Compile();

        var value = new SchemaTwoValues<T, TProperty>
        {
            Type = valueType,
            GetSourceValue1 = propertyFunc1,
            GetSourceValue2 = propertyFunc2,
            FormatValue = format.NotNull(),
            Attribute = attribute,
        };

        _graphCodes.Add(value);
        return this;
    }
}
