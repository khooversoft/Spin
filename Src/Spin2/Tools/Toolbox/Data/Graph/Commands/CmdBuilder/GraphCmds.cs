//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class GraphCmds
//{
//    private readonly List<string> _cmds = new Sequence<string>();
//    private readonly string _delimiter;

//    public GraphCmds(string delimiter = ";") => _delimiter = delimiter;

//    public GraphCmds Add(string cmd) => this.Action(_ => _cmds.Add(cmd.NotEmpty()));

//    public string Build()
//    {
//        _cmds.Assert(x => x.Count > 0, "Empty set");
//        return _cmds.Select(x => x[^1] == ';' ? x[0..^1] : x).Join(_delimiter) + ';';
//    }
//}
