//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Metrics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Diagnostics.Metrics.Testing;
//using Toolbox.Extensions;
//using Toolbox.Graph;
//using Toolbox.Tools;

//namespace Toolbox.Test.Application;

//public class TestGraphMetricCollector
//{
//    public TestGraphMetricCollector(IMeterFactory meterFactory)
//    {
//        Nodes = new TestNodesCollector(meterFactory);
//        Edges = new TestEdgesCollector(meterFactory);
//        Leases = new TestLeasesCollector(meterFactory);
//    }

//    public TestNodesCollector Nodes { get; }
//    public TestEdgesCollector Edges { get; }
//    public TestLeasesCollector Leases { get; }
//}

//public class TestNodesCollector : TestBaseCollector
//{
//    private readonly MetricCollector<long> _foreignKeyAdded;
//    private readonly MetricCollector<long> _foreignKeyRemoved;

//    public TestNodesCollector(IMeterFactory meterFactory) : base(meterFactory, "node")
//    {
//        _foreignKeyAdded = new MetricCollector<long>(meterFactory, nameof(GraphMap), "node.foreignkey.added");
//        _foreignKeyRemoved = new MetricCollector<long>(meterFactory, nameof(GraphMap), "node.foreignkey.removed");
//    }

//    public long ForeignKeyAdded => _foreignKeyAdded.LastMeasurement?.Value ?? 0;
//    public long ForeignKeyRemoved => _foreignKeyRemoved.LastMeasurement?.Value ?? 0;
//}

//public class TestEdgesCollector : TestBaseCollector
//{
//    public TestEdgesCollector(IMeterFactory meterFactory) : base(meterFactory, "edge")
//    {
//    }
//}

//public class TestLeasesCollector
//{
//    private readonly MetricCollector<long> _activeAcquire;
//    private readonly MetricCollector<long> _activeExclusive;
//    private readonly MetricCollector<long> _acquire;
//    private readonly MetricCollector<long> _release;

//    public TestLeasesCollector(IMeterFactory meterFactory)
//    {
//        _activeAcquire = new MetricCollector<long>(meterFactory, nameof(GraphMap), "lease.acquire.count");
//        _activeExclusive = new MetricCollector<long>(meterFactory, nameof(GraphMap), "lease.exclusive.count");
//        _acquire = new MetricCollector<long>(meterFactory, nameof(GraphMap), "lease.acquire");
//        _release = new MetricCollector<long>(meterFactory, nameof(GraphMap), "lease.release");
//    }

//    public long Acquire => _acquire.LastMeasurement?.Value ?? 0;
//    public long Release => _release.LastMeasurement?.Value ?? 0;
//    public long ActiveAcquire => _activeAcquire.LastMeasurement?.Value ?? 0;
//    public long ActiveExclusive => _activeExclusive.LastMeasurement?.Value ?? 0;
//}

//public abstract class TestBaseCollector
//{
//    private readonly MetricCollector<long> _added;
//    private readonly MetricCollector<long> _deleted;
//    private readonly MetricCollector<long> _updated;
//    private readonly MetricCollector<long> _indexHit;
//    private readonly MetricCollector<long> _indexMissed;
//    private readonly MetricCollector<long> _indexScan;
//    private readonly MetricCollector<long> _count;

//    public TestBaseCollector(IMeterFactory meterFactory, string prefix)
//    {
//        _added = new MetricCollector<long>(meterFactory, nameof(GraphMap), $"{prefix}.added");
//        _deleted = new MetricCollector<long>(meterFactory, nameof(GraphMap), $"{prefix}.deleted");
//        _updated = new MetricCollector<long>(meterFactory, nameof(GraphMap), $"{prefix}.updated");
//        _indexHit = new MetricCollector<long>(meterFactory, nameof(GraphMap), $"{prefix}.index.hit");
//        _indexMissed = new MetricCollector<long>(meterFactory, nameof(GraphMap), $"{prefix}.index.missed");
//        _indexScan = new MetricCollector<long>(meterFactory, nameof(GraphMap), $"{prefix}.index.scan");
//        _count = new MetricCollector<long>(meterFactory, nameof(GraphMap), $"{prefix}.index.count");
//    }

//    public long Added => _added.LastMeasurement?.Value ?? 0;
//    public long Deleted => _deleted.LastMeasurement?.Value ?? 0;
//    public long Updated => _updated.LastMeasurement?.Value ?? 0;
//    public long IndexHit => _indexHit.LastMeasurement?.Value ?? 0;
//    public long IndexMissed => _indexMissed.LastMeasurement?.Value ?? 0;
//    public long IndexScan => _indexScan.LastMeasurement?.Value ?? 0;
//    public long Count => _count.LastMeasurement?.Value ?? 0;
//}