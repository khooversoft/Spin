using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;


public class GraphSchemaBuilder<T>
{
    private readonly List<ISchemaValue<T>> _graphCodes = new List<ISchemaValue<T>>();

    public GraphSchemaBuilder<T> DataName<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format) =>
        AddExpression(expression, SchemaType.DataName, format);

    public GraphSchemaBuilder<T> DataName(string dataName) =>
        AddExpression(_ => dataName, SchemaType.DataName, x => x);

    public GraphSchemaBuilder<T> Node<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format) =>
        AddExpression(expression, SchemaType.Node, format);

    public GraphSchemaBuilder<T> Tag<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format) =>
        AddExpression(expression, SchemaType.Tags, format);

    public GraphSchemaBuilder<T> Index<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format) =>
        AddExpression(expression, SchemaType.Index, format);

    public GraphSchemaBuilder<T> Index<TProperty>(Expression<Func<T, TProperty>> expr1, Expression<Func<T, TProperty>> expr2, Func<TProperty, TProperty, string?> format)
    {
        return AddExpression(expr1, expr2, SchemaType.Index, x =>
        {
            x.Count.Assert(x => x == 2, x => $"count={x}");
            return format(x[0], x[1]);
        });
    }

    public GraphSchemaBuilder<T> Reference<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format, string attribute) =>
        AddExpression(expression, SchemaType.Reference, format, attribute);

    public GraphSchemaBuilder<T> Select<TProperty>(Expression<Func<T, TProperty>> expression, Func<TProperty, string?> format, string queryName = "default") =>
        AddExpression(expression, SchemaType.Select, format, queryName);

    public GraphSchemaBuilder<T> Select<TProperty>(
        Expression<Func<T, TProperty>> expr1, 
        Expression<Func<T, TProperty>> expr2, 
        Func<TProperty, TProperty, string?> format, 
        string queryName = "default"
        ) =>
        AddExpression(expr1, expr2, SchemaType.Select, x =>
        {
            x.Count.Assert(x => x == 2, x => $"count={x}");
            return format(x[0], x[1]);
        }, queryName);


    public IGraphSchema<T> Build() => new GraphSchema<T>(_graphCodes);


    private GraphSchemaBuilder<T> AddExpression<TProperty>(Expression<Func<T, TProperty>> expression, SchemaType valueType, Func<TProperty, string?> format, string? attribute = null)
    {
        Func<T, TProperty> propertyFunc = expression.NotNull().Compile();

        var value = new SchemaValue<T, TProperty>
        {
            Type = valueType,
            GetSourceValues = [propertyFunc],
            FormatValue = x => format.NotNull()(x[0]),
            Attribute = attribute,
        };

        _graphCodes.Add(value);
        return this;
    }

    private GraphSchemaBuilder<T> AddExpression<TProperty>(
        Expression<Func<T, TProperty>> expr1,
        Expression<Func<T, TProperty>> expr2,
        SchemaType valueType,
        Func<IReadOnlyList<TProperty>, string?> format,
        string? attribute = null)
    {
        Func<T, TProperty> propertyFunc1 = expr1.NotNull().Compile();
        Func<T, TProperty> propertyFunc2 = expr2.NotNull().Compile();

        var value = new SchemaValue<T, TProperty>
        {
            Type = valueType,
            GetSourceValues = [propertyFunc1, propertyFunc2],
            FormatValue = x => format.NotNull()(x),
            Attribute = attribute,
        };

        _graphCodes.Add(value);
        return this;
    }
}
