//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class GraphCmdSearch
//{
//    private readonly List<string> _cmds = new Sequence<string>();

//    public GraphCmdSearch Node(string? key = null, string? tags = null)
//    {
//        (key.IsEmpty() && tags.IsEmpty()).Assert(x => x == false, "Requres search criteria");

//        var seq = new Sequence<string?>();
//        seq += GraphCmd.OptionCmd("key", key);
//        seq += GraphCmd.OptionCmd("tags", tags);

//        string cmd = seq.Where(x => x != null).Join(';');

//        _cmds.Add($"({cmd})");
//        return this;
//    }

//    public GraphCmdSearch Edge(string? fromKey = null, string? toKey = null, string? edgeType = null, string? tags = null)
//    {
//        (fromKey.IsEmpty() && toKey.IsEmpty() && edgeType.IsEmpty() && tags.IsEmpty()).Assert(x => x == false, "Requres search criteria");

//        var seq = new Sequence<string?>();
//        seq += GraphCmd.OptionCmd("fromKey", fromKey);
//        seq += GraphCmd.OptionCmd("toKey", toKey);
//        seq += GraphCmd.OptionCmd("edgeType", edgeType);
//        seq += GraphCmd.OptionCmd("tags", tags);

//        string cmd = seq.Where(x => x != null).Join(';');
//        _cmds.Add($"[{cmd}]");
//        return this;
//    }

//    public string Build() => _cmds
//        .Assert(x => x.Count > 0, "Empty search criteria")
//        .Join("->");
//}