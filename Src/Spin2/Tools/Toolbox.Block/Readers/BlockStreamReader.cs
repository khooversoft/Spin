//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Block;

//public class BlockStreamReader<T> where T : class
//{
//    protected readonly IEnumerable<T> _blockNodes;
//    internal BlockStreamReader(IEnumerable<T> blockNodes) => _blockNodes = blockNodes.NotNull();

//    public Option<T> GetLatest() => _blockNodes.LastOrDefaultOption();
//    public IReadOnlyList<T> List() => _blockNodes.ToArray();
//}
