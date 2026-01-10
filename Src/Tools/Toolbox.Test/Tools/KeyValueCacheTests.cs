using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Test.Tools;

public class KeyValueCacheTests
{
    [Fact]
    public void SimpleFlow()
    {
        var key = "key";

        var cache = new KeyValueCache<string>(TimeSpan.FromMilliseconds(100));
        cache.Count.Be(0);
        cache.TryGetValue(key).BeNotFound();

        cache.AddOrUpdate(key, "value1");
        cache.Count.Be(1);
        cache.TryGetValue(key).BeOk().Return().Be("value1");

        Thread.Sleep(TimeSpan.FromMilliseconds(200));
        cache.Count.Be(1);

        cache.TryGetValue(key).BeConflict().Return().Be("value1");
        cache.TryGetValue(key).BeNotFound();
        cache.Count.Be(0);
    }

    [Fact]
    public void AddOrUpdate_ShouldRefreshLifetime()
    {
        var key = "key";
        var cache = new KeyValueCache<string>(TimeSpan.FromMilliseconds(200));

        cache.AddOrUpdate(key, "value1");
        Thread.Sleep(TimeSpan.FromMilliseconds(50));

        cache.AddOrUpdate(key, "value2");
        Thread.Sleep(TimeSpan.FromMilliseconds(50));

        cache.TryGetValue(key).BeOk().Return().Be("value2");
        cache.Count.Be(1);
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries()
    {
        var cache = new KeyValueCache<string>(TimeSpan.FromMilliseconds(500));

        cache.AddOrUpdate("k1", "v1");
        cache.AddOrUpdate("k2", "v2");
        cache.Count.Be(2);

        cache.ClearAll();

        cache.Count.Be(0);
        cache.TryGetValue("k1").BeNotFound();
        cache.TryGetValue("k2").BeNotFound();
    }

    [Fact]
    public void ClearLeaseCache_ShouldRemoveOnlyRequestedKey()
    {
        var cache = new KeyValueCache<string>(TimeSpan.FromMilliseconds(500));

        cache.AddOrUpdate("k1", "v1");
        cache.AddOrUpdate("k2", "v2");

        cache.Clear("k1");

        cache.TryGetValue("k1").BeNotFound();
        cache.TryGetValue("k2").BeOk().Return().Be("v2");
        cache.Count.Be(1);
    }

    [Fact]
    public async Task ConcurrentAddOrUpdate_ShouldMaintainSingleEntry()
    {
        var cache = new KeyValueCache<string>(TimeSpan.FromSeconds(1));
        var key = "key";

        var tasks = Enumerable.Range(0, 50)
            .Select(i => Task.Run(() => cache.AddOrUpdate(key, $"value{i}")));

        await Task.WhenAll(tasks);

        cache.Count.Be(1);

        var value = cache.TryGetValue(key).BeOk().Return();
        value.StartsWith("value", StringComparison.Ordinal).BeTrue();
    }

    [Fact]
    public void EmptyKey_ShouldThrow()
    {
        var cache = new KeyValueCache<string>(TimeSpan.FromMilliseconds(50));

        Verify.Throws<ArgumentNullException>(() => cache.AddOrUpdate(string.Empty, "value"));
        Verify.Throws<ArgumentNullException>(() => cache.TryGetValue(string.Empty));
        Verify.Throws<ArgumentNullException>(() => cache.Clear(string.Empty));
    }
}
