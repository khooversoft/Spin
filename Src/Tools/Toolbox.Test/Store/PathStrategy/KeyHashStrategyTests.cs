using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Test.Store.PathStrategy;

public class KeyHashStrategyTests
{
    private record SampleType;

    [Theory]
    [InlineData("root", "root")]
    [InlineData("/root", "root")]
    [InlineData("root/", "root")]
    [InlineData("/root/", "root")]
    public void Constructor_ShouldNormalizeBasePath(string basePath, string expected)
    {
        var sut = new KeyHashStrategy(basePath, useCache: true);

        sut.BasePath.Be(expected);
        sut.UseCache.BeTrue();
    }

    [Fact]
    public void Constructor_ShouldRespectUseCacheFlag()
    {
        var sut = new KeyHashStrategy("root", useCache: false);
        sut.UseCache.BeFalse();
    }

    [Fact]
    public void Constructor_ShouldThrowWhenBasePathMissing()
    {
        Verify.Throws<ArgumentNullException>(() => new KeyHashStrategy(string.Empty, true));
    }

    [Fact]
    public void PathBuilder_ShouldBuildLowerCaseHashPath()
    {
        var sut = new KeyHashStrategy("Root/Folder", useCache: false);
        const string key = "My/Key";

        string hash = PathTool.CreateHashPath(key);
        string result = sut.BuildPath(key);

        result.Be($"root/folder/{hash}/{key.ToLowerInvariant()}");
    }

    [Fact]
    public void PathBuilder_ShouldThrowWhenKeyMissing()
    {
        var sut = new KeyHashStrategy("root", useCache: false);

        Verify.Throws<ArgumentNullException>(() => sut.BuildPath(null!));
    }

    [Fact]
    public void PathBuilder_Generic_ShouldIncludeTypeAndHashPath()
    {
        var sut = new KeyHashStrategy("Root", useCache: false);
        const string key = "My/Key";

        string hash = PathTool.CreateHashPath(key);
        string result = sut.BuildPath<SampleType>(key);

        result.Be($"root/{hash}/{key.ToLowerInvariant()}.sampletype.json");
    }

    [Fact]
    public void BuildSearch_ShouldCombineBaseWithPattern()
    {
        var sut = new KeyHashStrategy("ROOT", useCache: false);

        string result = sut.BuildSearch("PATTERN/**");

        result.Be("root/*/*/pattern/**");
    }

    [Fact]
    public void BuildKeySearch_ShouldAppendWildcard()
    {
        var sut = new KeyHashStrategy("ROOT", useCache: false);

        string result = sut.BuildKeySearch("KeyFolder");

        result.Be("root/*/*/keyfolder/**");
    }

    [Fact]
    public void ExtractKey_ShouldHandleTypedPathWithHash()
    {
        var sut = new KeyHashStrategy("root", useCache: false);
        const string key = "sub/path/item";

        string path = sut.BuildPath<SampleType>(key);
        string result = sut.ExtractKey(path);

        result.Be(key);
    }

    [Fact]
    public void ExtractKey_ShouldThrowOnInvalidFormat()
    {
        var sut = new KeyHashStrategy("root", useCache: false);

        Verify.Throws<ArgumentException>(() => sut.ExtractKey("root/invalid?path"));
    }

    [Fact]
    public void RemoveBasePath_ShouldStripBaseCaseInsensitive()
    {
        var sut = new KeyHashStrategy("root/path", useCache: false);

        string result = sut.RemoveBasePath("ROOT/PATH/item/sub");

        result.Be("item/sub");
    }

    [Fact]
    public void RemoveBasePath_ShouldReturnOriginalWhenBaseMissing()
    {
        var sut = new KeyHashStrategy("root/path", useCache: false);

        string result = sut.RemoveBasePath("other/item");

        result.Be("other/item");
    }

    [Fact]
    public void RemoveBasePath_ShouldThrowWhenNull()
    {
        var sut = new KeyHashStrategy("root/path", useCache: false);

        Verify.Throws<ArgumentNullException>(() => sut.RemoveBasePath(null!));
    }
}
