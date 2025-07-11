//using Toolbox.Tools;
//using Toolbox.Tools.Should;

//namespace Toolbox.Test.Tools;

//public class CacheObjectTests
//{
//    [Fact]
//    public void GivenCacheObject_Initialize_ShouldBeFalseState()
//    {
//        var cache = new CacheObject<string>(TimeSpan.FromSeconds(10));

//        cache.IsValid().BeFalse();
//        cache.TryGetValue(out string value).BeFalse();
//        value.BeEmpty();
//    }

//    [Fact]
//    public void GivenNullValue_WhenCached_ShouldBeTrue()
//    {
//        string item = null!;
//        var cache = item.ToCacheObject(TimeSpan.FromSeconds(10));

//        cache.IsValid().BeFalse();

//        string item2 = "this is the item";
//        cache.Set(item2);
//        cache.TryGetValue(out string value2).BeTrue();
//        value2.Be(item2);
//        cache.IsValid().BeTrue();
//    }

//    [Fact]
//    public void GivenValue_WhenCachedAndCleard_ShouldBeFalseState()
//    {
//        const string valueToCache = "value to be cached";
//        string item = valueToCache;
//        var cache = item.ToCacheObject(TimeSpan.FromSeconds(10));

//        cache.IsValid().BeTrue();
//        cache.TryGetValue(out string value).BeTrue();
//        value.Be(valueToCache);

//        cache.Clear();
//        cache.TryGetValue(out string value2).BeFalse();
//        value2.BeNull();
//    }

//    [Fact]
//    public void ResetTest()
//    {
//        string item = "Item to be cached";
//        var cache = new CacheObject<string>(TimeSpan.FromMilliseconds(100)).Set(item);

//        cache.TryGetValue(out string value).BeTrue();
//        value.NotBeEmpty();
//        value.Be(item);
//        cache.IsValid().BeTrue();

//        cache.Clear();
//        cache.IsValid().BeFalse();
//    }

//    [Fact]
//    public void StoreTest()
//    {
//        string item = "Item to be cached";
//        var cache = new CacheObject<string>(TimeSpan.FromSeconds(100)).Set(item);

//        cache.IsValid().BeTrue();
//        cache.TryGetValue(out string value).BeTrue();
//        value.NotBeEmpty();
//        value.Be(item);
//    }

//    [Fact]
//    public void StoreExtensionTest()
//    {
//        string item = "Item to be cached";
//        CacheObject<string> cache = item.ToCacheObject(TimeSpan.FromSeconds(100));

//        cache.IsValid().BeTrue();
//        cache.TryGetValue(out string value).BeTrue();
//        value.NotBeEmpty();
//        value.Be(item);
//    }

//    [Fact]
//    public void ExpireTest()
//    {
//        string item = "Item to be cached";
//        var cache = new CacheObject<string>(TimeSpan.FromMilliseconds(100)).Set(item);

//        cache.TryGetValue(out string value).BeTrue();
//        value.NotBeEmpty();
//        value.Be(item);

//        Thread.Sleep(TimeSpan.FromMilliseconds(200));
//        cache.TryGetValue(out value).BeFalse();
//        value.BeEmpty();
//    }

//    [Fact]
//    public void TryTest()
//    {
//        string item = "Item to be cached";
//        var cache = new CacheObject<string>(TimeSpan.FromMilliseconds(100)).Set(item);

//        cache.TryGetValue(out string value).Be(true);
//        value.NotBeEmpty();
//        value.Be(item);

//        Thread.Sleep(TimeSpan.FromMilliseconds(200));
//        cache.TryGetValue(out value).BeFalse();
//        value.BeEmpty();
//    }

//    [Fact]
//    public void GivenInt_WhenSetToDefault_ShouldFails()
//    {
//        int item = default;
//        var cache = new CacheObject<int>(TimeSpan.FromMilliseconds(100)).Set(item);

//        cache.TryGetValue(out int value).Be(true);
//        value.Be(default);
//    }

//    [Fact]
//    public void GivenInt_WhenSetToValue_ShouldPass()
//    {
//        int item = 10;
//        var cache = new CacheObject<int>(TimeSpan.FromMilliseconds(100)).Set(item);

//        cache.TryGetValue(out int value).Be(true);
//        value.Be(item);

//        Thread.Sleep(TimeSpan.FromMilliseconds(200));
//        cache.TryGetValue(out value).BeFalse();
//        value.Be(default);
//    }
//}
