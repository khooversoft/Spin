//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class GraphCmd
//{
//    private readonly List<string> _cmds = new Sequence<string>();

//    public GraphCmd AddNode(string key, string? tags = null)
//    {
//        var seq = new Sequence<string?>();
//        seq += $"key={key.NotEmpty()}";
//        seq += OptionCmd("tags", tags);

//        string cmd = seq.Where(x => x != null).Join(",");

//        _cmds.Add($"add node {cmd};");
//        return this;
//    }

//    public GraphCmd AddEdge(string fromKey, string toKey, string? edgeType = null, string? tags = null)
//    {
//        var seq = new Sequence<string?>();
//        seq += $"fromKey={fromKey.NotEmpty()}";
//        seq += $"toKey={toKey.NotEmpty()}";
//        seq += OptionCmd("edgeType", edgeType);
//        seq += OptionCmd("tags", tags);

//        string cmd = seq.Where(x => x != null).Join(",");

//        _cmds.Add($"add edge {cmd};");
//        return this;
//    }

//    public GraphCmd Delete(GraphCmdSearch search)
//    {
//        search.NotNull();

//        _cmds.Add($"delete {search.Build()};");
//        return this;
//    }

//    public GraphCmd UpdateNode(GraphCmdSearch search, string tags)
//    {
//        search.NotNull();
//        tags.NotEmpty();

//        _cmds.Add($"update {search.Build()} set tags={tags};");
//        return this;
//    }

//    public GraphCmd UpdateEdge(GraphCmdSearch search, string? edgeType = null, string? tags = null)
//    {
//        search.NotNull();
//        (edgeType == null && tags == null).Assert(x => x == false, "edgeType and/or tags is required");

//        var seq = new Sequence<string?>();
//        seq += OptionCmd("edgeType", edgeType);
//        seq += OptionCmd("tags", tags);
//        string cmd = seq.Where(x => x != null).Join(",");

//        _cmds.Add($"update {search.Build()} set {cmd};");
//        return this;
//    }

//    public string Build() => _cmds.Assert(x => x.Count > 0, "Empty commands").Join(" ");

//    internal static string? OptionCmd(string label, string? cmd)
//    {
//        if (cmd.IsEmpty()) return null;
//        if (label != "tags" || cmd.IndexOf('=') < 0) return $"{label.NotEmpty()}={cmd}";
//        return $"{label.NotEmpty()}='{cmd}'";
//    }
//}
