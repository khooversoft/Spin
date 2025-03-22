using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace Toolbox.Test.Store.Memory;

public class MemoryStoreLeaseTests
{
    [Fact]
    public void AddGetLeaseUpdateGetRemove()
    {
        var store = new MemoryStore();
        const string path = "path/data";
        const string dataValue = "data";
        const string dataValue2 = "data2";
        DataETag data = dataValue.ToDataETag();
        DataETag data2 = dataValue2.ToDataETag();

        store.Exist(path).Should().BeFalse();
        store.IsLeased(path).Should().BeFalse();

        store.Add(path, data).IsOk().Should().BeTrue();
        store.Exist(path).Should().BeTrue();
        store.IsLeased(path).Should().BeFalse();

        // Acquire lease
        var leaseId = store.AcquireLease(path, TimeSpan.FromMinutes(2));
        leaseId.IsOk().Should().BeTrue();

        store.Add(path, data).IsConflict().Should().BeTrue();
        store.Set(path, data2).IsConflict().Should().BeTrue();
        store.Remove(path).IsConflict().Should().BeTrue();

        store.Get(path).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Data.BytesToString().Should().Be(dataValue);
        });

        store.Search("*").Action(x =>
        {
            x.Count.Should().Be(1);
            x.First().Path.Should().Be(path);
        });

        // The only write/append that should work
        store.Set(path, data2, leaseId.Return()).IsOk().Should().BeTrue();

        store.Get(path).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Data.BytesToString().Should().Be(dataValue2);
        });

        store.Append(path, data2, leaseId.Return()).IsOk().Should().BeTrue();

        store.Set(path, data).Action(x => x.IsConflict().Should().BeTrue(x.ToString()));

        // Release lease
        store.ReleaseLease(leaseId.Return()).Action(x => x.IsOk().Should().BeTrue(x.ToString()));

        store.Set(path, data2, leaseId.Return()).IsOk().Should().BeTrue();
        store.Append(path, data2, leaseId.Return()).IsOk().Should().BeTrue();
        store.Set(path, data).IsOk().Should().BeTrue();

        store.Get(path).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Data.BytesToString().Should().Be(dataValue);
        });

        store.Remove(path).IsOk().Should().BeTrue();
        store.Exist(path).Should().BeFalse();
        store.Search("*").Count.Should().Be(0);

        store.Get(path).IsNotFound().Should().BeTrue();
    }
}
