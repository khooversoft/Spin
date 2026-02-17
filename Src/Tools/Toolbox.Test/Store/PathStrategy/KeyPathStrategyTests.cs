using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Test.Store.PathStrategy;

public class KeyPathStrategyTests
{
    private record SampleType;

    [Theory]
    [InlineData("root", "root")]
    [InlineData("/root", "root")]
    [InlineData("root/", "root")]
    [InlineData("/root/", "root")]
    public void Constructor_ShouldNormalizeBasePath(string basePath, string expected)
    {
        var sut = new KeyPathStrategy(basePath, useCache: true);

        sut.BasePath.Be(expected);
        sut.UseCache.BeTrue();
    }

    [Fact]
    public void Constructor_ShouldRespectUseCacheFlag()
    {
        var sut = new KeyPathStrategy("root", useCache: false);
        sut.UseCache.BeFalse();
    }

    [Fact]
    public void Constructor_ShouldThrowWhenBasePathMissing()
    {
        Verify.Throws<ArgumentNullException>(() => new KeyPathStrategy(string.Empty, true));
    }

    [Fact]
    public void PathBuilder_ShouldBuildLowerCasePath()
    {
        var sut = new KeyPathStrategy("Root/Folder", useCache: false);

        string result = sut.BuildPath("My/Key");

        result.Be("root/folder/my/key");
    }

    [Fact]
    public void PathBuilder_ShouldThrowWhenKeyMissing()
    {
        var sut = new KeyPathStrategy("root", useCache: false);

        Verify.Throws<ArgumentNullException>(() => sut.BuildPath(null!));
    }

    [Fact]
    public void PathBuilder_Generic_ShouldIncludeType()
    {
        var sut = new KeyPathStrategy("Root", useCache: false);

        string result = sut.BuildPath<SampleType>("MyKey");

        result.Be("root/mykey.sampletype.json");
    }

    [Fact]
    public void BuildSearch_ShouldCombineBaseWithPattern()
    {
        var sut = new KeyPathStrategy("ROOT", useCache: false);

        string result = sut.BuildSearch("PATTERN/**");

        result.Be("root/pattern/**");
    }

    [Fact]
    public void BuildKeySearch_ShouldAppendWildcard()
    {
        var sut = new KeyPathStrategy("ROOT", useCache: false);

        string result = sut.BuildKeySearch("KeyFolder");

        result.Be("root/keyfolder/**/*");
    }

    [Fact]
    public void ExtractKey_ShouldHandleTypedPath()
    {
        var sut = new KeyPathStrategy("root", useCache: false);

        string result = sut.ExtractKey($"ROOT/key.{nameof(SampleType)}.json");

        result.Be("key");
    }

    [Fact]
    public void ExtractKey_ShouldHandleKeyOnlyPath()
    {
        var sut = new KeyPathStrategy("root", useCache: false);

        string result = sut.ExtractKey("root/section/key/value");

        result.Be("section/key/value");
    }

    [Fact]
    public void ExtractKey_ShouldThrowOnInvalidFormat()
    {
        var sut = new KeyPathStrategy("root", useCache: false);

        Verify.Throws<ArgumentException>(() => sut.ExtractKey("root/key?name/value"));
    }

    [Fact]
    public void RemoveBasePath_ShouldStripBaseCaseInsensitive()
    {
        var sut = new KeyPathStrategy("root/path", useCache: false);

        string result = sut.RemoveBasePath("ROOT/PATH/item/sub");

        result.Be("item/sub");
    }

    [Fact]
    public void RemoveBasePath_ShouldReturnOriginalWhenBaseMissing()
    {
        var sut = new KeyPathStrategy("root/path", useCache: false);

        string result = sut.RemoveBasePath("other/item");

        result.Be("other/item");
    }

    [Fact]
    public void RemoveBasePath_ShouldThrowWhenNull()
    {
        var sut = new KeyPathStrategy("root/path", useCache: false);

        Verify.Throws<ArgumentNullException>(() => sut.RemoveBasePath(null!));
    }

    [Fact]
    public void GetPathParts_ShouldReturnKeyOnly()
    {
        var sut = new KeyPathStrategy("root", useCache: false);

        var (key, typeName) = sut.GetPathParts("root/folder/sub/key");

        typeName.BeNull();
        key.Be("folder/sub/key");
    }

    [Fact]
    public void GetPathParts_ShouldReturnTypeAndKeyForTypedPath()
    {
        var sut = new KeyPathStrategy("root", useCache: false);

        var (key, typeName) = sut.GetPathParts($"root/folder/key.{nameof(SampleType)}.json");

        typeName.Be(nameof(SampleType).ToLowerInvariant());
        key.Be("folder/key");
    }
}
