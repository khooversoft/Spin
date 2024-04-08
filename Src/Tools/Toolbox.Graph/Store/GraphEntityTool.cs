using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public interface IGraphEntityCommand
{
    string GetAddCommand();
    string GetDeleteCommand();
}

public static class GraphEntityTool
{
    public static Option<NodeCreateCommand> GetEntityNodeCommand(this IReadOnlyList<IGraphEntityCommand> commands) => commands
        .OfType<NodeCreateCommand>()
        .Where(x => x.IsEntityNode)
        .FirstOrDefaultOption(returnNotFound: true);

    public static Option<IReadOnlyList<IGraphEntityCommand>> GetGraphCommands<T>(this T value)
    {
        value.NotNull();

        var propertiesValues = value.GetType()
            .GetProperties()
            .SelectMany(x =>
            {
                var searchList = search(x);
                return searchList.Length switch
                {
                    0 => new PropertyValue(x.Name, null, x.GetValue(value)?.ToString()).ToEnumerable(),
                    _ => searchList.Select(y => new PropertyValue(x.Name, y, x.GetValue(value)?.ToString()))
                };
            })
            .ToArray();

        var nodeCommands = getKeyCommand(propertiesValues);
        if (nodeCommands.IsError()) return nodeCommands.ToOptionStatus<IReadOnlyList<IGraphEntityCommand>>();
        NodeCreateCommand setKeyNode = nodeCommands.Return();

        var indexCommands = getIndexCommands(propertiesValues, setKeyNode.NodeKey);

        IReadOnlyList<IGraphEntityCommand> set = [setKeyNode, .. indexCommands];
        return set.ToOption();

        Attribute[] search(MemberInfo x) => x.GetCustomAttributes(false).OfType<Attribute>().Where(y => isGraphAttribute(y)).ToArray();
        bool isGraphAttribute(Attribute x) => x is GraphKeyAttribute || x is GraphTagAttribute || x is GraphNodeIndexAttribute;
    }

    private static Option<NodeCreateCommand> getKeyCommand(PropertyValue[] propertiesValues)
    {
        bool allHasValue = propertiesValues
            .Where(x => x.Attribute is GraphTagAttribute)
            .All(x => x.Value.IsNotEmpty());

        if (!allHasValue) return (StatusCode.Conflict, "Required key property value is null");

        string tags = propertiesValues
            .Where(x => x.Attribute is GraphTagAttribute)
            .Select(x => getTag(x))
            .Join(',');

        var cmds = propertiesValues
            .Where(x => x.Attribute is GraphKeyAttribute && x.Value.IsNotEmpty())
            .Select(getNodeKey)
            .Select(x => new NodeCreateCommand(x, tags, isEntityNode: true))
            .Take(1)
            .ToArray();

        return cmds.Length == 1 ? cmds[0] : (StatusCode.Conflict, "Cannot find property with GraphKeyAttribute or there is more then 1");

        string getTag(PropertyValue pv) => $"{(((GraphTagAttribute)pv.Attribute!).TagName ?? pv.Name)}={pv.Value}";
        string getNodeKey(PropertyValue pv) => $"{(((GraphKeyAttribute)pv.Attribute!).IndexName)}:{pv.Value}";
    }

    private static IEnumerable<IGraphEntityCommand> getIndexCommands(PropertyValue[] propertiesValues, string rootNodeKey)
    {
        var cmds = propertiesValues
            .Where(x => x.Attribute is GraphNodeIndexAttribute)
            .Select(x => FormatWith(x, propertiesValues))
            .Where(x => x != null)
            .SelectMany(x => new IGraphEntityCommand[]
            {
                new NodeCreateCommand(x!, GraphConstants.UniqueIndexTag),
                new EdgeCreateCommand(x!, rootNodeKey)
            })
            .ToArray();

        return cmds;
    }

    private static string? FormatWith(PropertyValue pv, PropertyValue[] propertyValues)
    {
        var attr = pv.Attribute as GraphNodeIndexAttribute ?? throw new UnreachableException();
        if (pv.Value == null) return null;

        string? value = attr.Format switch
        {
            null => $"{attr.IndexName}:{pv.Value}",
            not null => FormatWith(attr, propertyValues) switch
            {
                null => null,
                string v => attr.IndexName + ":" + v
            }
        };

        return value;
    }

    private static string? FormatWith(GraphNodeIndexAttribute attr, PropertyValue[] propertyValues)
    {
        if (attr.Format == null) return null;

        var propertyNames = GetPropertyNames(attr.Format);

        var join = propertyNames
            .Join(propertyValues, x => x, x => x.Name, (o, i) => (pname: o, pvalue: i))
            .ToArray();

        (join.Length == propertyNames.Count).Assert(x => x == true, $"Cannot find all property names in format={attr.Format}");

        int propertyWithValueCount = join.Where(x => x.pvalue.Value.IsNotEmpty()).Count();
        if (propertyWithValueCount == 0) return null;
        if (propertyWithValueCount != propertyNames.Count) return null;

        string value = propertyValues.Aggregate(attr.Format, (a, x) => a.Replace($"{{{x.Name}}}", x.Value));
        return value;
    }

    private static IReadOnlyList<string> GetPropertyNames(string fmt)
    {
        var p = new StringTokenizer()
            .AddBlock('{', '}')
            .Parse(fmt)
            .OfType<BlockToken>()
            .ToArray();

        var result = p
            .Select(x => x.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return result;
    }

    [DebuggerDisplay("Name={Name}, Value={Value}, Attribute={Attribute}")]
    private readonly struct PropertyValue
    {
        public PropertyValue(string name, Attribute? attribute, string? value)
        {
            Name = name;
            Attribute = attribute;
            Value = value;
        }

        public string Name { get; } = null!;
        public Attribute? Attribute { get; }
        public string? Value { get; }
    }
}
