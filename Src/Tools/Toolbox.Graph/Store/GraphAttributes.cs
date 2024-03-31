using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Graph;

[AttributeUsage(AttributeTargets.Property)]
public class GraphKeyAttribute : Attribute
{
    public GraphKeyAttribute(string format) => Format = format;
    public string Format { get; }
}

[AttributeUsage(AttributeTargets.Property)]
public class GraphTagAttribute : Attribute
{
    public GraphTagAttribute(string tagName) => TagName = tagName;
    public string TagName { get; }
}

[AttributeUsage(AttributeTargets.Property)]
public class GraphNodeIndexAttribute : Attribute
{
    public GraphNodeIndexAttribute(string format) => Format = format;
    public string Format { get; }
}
