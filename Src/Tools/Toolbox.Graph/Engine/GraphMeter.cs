using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class GraphMeter
{
    private readonly GraphMap _graphMap;

    public GraphMeter(GraphMap graphMap)
    {
        _graphMap = graphMap;
        Node = new Counter(() => _graphMap.Nodes.Count);
        Edge = new Counter(() => _graphMap.Edges.Count);
    }

    public Counter Node { get; }
    public Counter Edge { get; }

    private void IfTest(bool condition, ref long trueValue, ref long falseValue)
    {
        if (condition)
            Interlocked.Add(ref trueValue, 1);
        else
            Interlocked.Add(ref falseValue, 1);
    }

    public sealed class Counter
    {
        private long _added;
        private long _deleted;
        private long _updated;
        private long _indexHit;
        private long _indexMissed;
        private long _indexScan;
        private readonly Func<long> _getCount = null!;

        internal Counter(Func<long> getCount) => _getCount = getCount.NotNull();

        public long GetAdded() => _added;
        public long GetDeleted() => _deleted;
        public long GetUpdated() => _updated;
        public long GetIndexHit() => _indexHit;
        public long GetIndexMissed() => _indexMissed;
        public long GetIndexScan() => _indexScan;
        public long GetCount() => _getCount();

        internal void Added(long value = 1) => Interlocked.Add(ref _added, value);
        internal void Deleted(long value = 1) => Interlocked.Add(ref _deleted, value);
        internal void Updated(long value = 1) => Interlocked.Add(ref _updated, value);
        internal void IndexHit(long value = 1) => Interlocked.Add(ref _indexHit, value);
        internal void IndexMissed(long value = 1) => Interlocked.Add(ref _indexMissed, value);
        internal void IndexScan(long value = 1) => Interlocked.Add(ref _indexScan, value);
        internal void Index(bool value) => IfTest(value, ref _indexHit, ref _indexMissed);
        internal void Index(Option value) => IfTest(value.IsOk(), ref _indexHit, ref _indexMissed);
        internal void Index<T>(Option<T> value) => IfTest(value.IsOk(), ref _indexHit, ref _indexMissed);

        private void IfTest(bool condition, ref long trueValue, ref long falseValue)
        {
            if (condition)
                Interlocked.Add(ref trueValue, 1);
            else
                Interlocked.Add(ref falseValue, 1);
        }
    }
}
