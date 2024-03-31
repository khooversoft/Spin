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

public static class GraphEntityTool
{
    public static IReadOnlyList<string> GetGraphAddCommands<T>(this T value)
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

        var nodeCommands = getKeyCommands(propertiesValues);
        if (nodeCommands.Count == 0) return Array.Empty<string>();
        (string rootNodeKey, string nodeCommand) = nodeCommands.First();

        var indexCommands = getIndexCommands(propertiesValues, rootNodeKey);

        return [nodeCommand, .. indexCommands];

        Attribute[] search(MemberInfo x) => x.GetCustomAttributes(false).OfType<Attribute>().Where(y => isGraphAttribute(y)).ToArray();
        bool isGraphAttribute(Attribute x) => x is GraphKeyAttribute || x is GraphTagAttribute || x is GraphNodeIndexAttribute;
    }

    private static IReadOnlyList<(string key, string cmd)> getKeyCommands(PropertyValue[] propertiesValues)
    {
        string tags = propertiesValues
            .Where(x => x.Attribute is GraphTagAttribute)
            .Select(x => $"{((GraphTagAttribute)x.Attribute!).TagName}={x.Value ?? throw new ArgumentException("Required key property value is null")}")
            .Join(',');

        var cmds = propertiesValues
            .Select(x => x.Attribute as GraphKeyAttribute)
            .OfType<GraphKeyAttribute>()
            .Select(x => (attr: x, key: FormatWith(x.Format, propertiesValues)))
            .Where(x => x.key != null)
            .Select(x => (key: x.key!, cmd: $"upsert node key={x.key}" + (tags.IsNotEmpty() ? ", " + tags : string.Empty)))
            .Take(1)
            .ToArray();

        return cmds;
    }

    private static IEnumerable<string> getIndexCommands(PropertyValue[] propertiesValues, string rootNodeKey)
    {
        var cmds = propertiesValues
            .Select(x => x.Attribute as GraphNodeIndexAttribute)
            .OfType<GraphNodeIndexAttribute>()
            .Select(x => (attr: x, fromKey: FormatWith(x.Format, propertiesValues)))
            .Where(x => x.fromKey != null)
            .SelectMany(x => new string[]
            {
                $"upsert node key={x.fromKey}",
                $"add unique edge fromKey={x.fromKey}, toKey={rootNodeKey};"
            })
            .ToArray();

        return cmds;
    }

    private static string? FormatWith(string fmt, PropertyValue[] attrValueList)
    {
        var propertyNames = GetPropertyNames(fmt);

        var join = propertyNames
            .Join(attrValueList, x => x, x => x.Name, (o, i) => (pname: o, pvalue: i))
            .ToArray();

        (join.Length == propertyNames.Count).Assert(x => x == true, $"Cannot find all property names in format={fmt}");

        int propertyWithValueCount = join.Where(x => x.pvalue.Value.IsNotEmpty()).Count();
        if( propertyWithValueCount == 0) return null;
        (propertyWithValueCount == propertyNames.Count).Assert(x => x == true, $"Not all properties have values in format={fmt}");

        string value = attrValueList.Aggregate(fmt, (a, x) => a.Replace($"{{{x.Name}}}", x.Value));
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
