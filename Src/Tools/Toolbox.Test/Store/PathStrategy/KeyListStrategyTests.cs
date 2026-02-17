using System.Text.RegularExpressions;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Test.Store.PathStrategy;

public class KeyListStrategyTests
{
    private record SampleType;

    [Theory]
    [InlineData("root", "root")]
    [InlineData("/root", "root")]
    [InlineData("root/", "root")]
    [InlineData("/root/", "root")]
    public void Constructor_ShouldNormalizeBasePath(string basePath, string expected)
    {
        var sut = new KeyListStrategy<SampleType>(basePath, new LogSequenceNumber());

        sut.BasePath.Be(expected);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenBasePathMissing()
    {
        Verify.Throws<ArgumentNullException>(() => new KeyListStrategy<SampleType>(string.Empty, new LogSequenceNumber()));
    }

    [Fact]
    public void PathBuilder_ShouldBuildLowerCasePathWithSeq()
    {
        var sut = new KeyListStrategy<SampleType>("Root/Folder", new LogSequenceNumber());

        string result = sut.BuildPath("My/Key");

        var regex = new Regex(@"^root/folder/my/key/my_key-\d{15}-\d{6}-[a-f0-9]{4}\.sampletype\.json$");
        regex.IsMatch(result).BeTrue();
    }

    [Fact]
    public void PathBuilder_ShouldThrowWhenKeyMissing()
    {
        var sut = new KeyListStrategy<SampleType>("root", new LogSequenceNumber());

        Verify.Throws<ArgumentNullException>(() => sut.BuildPath(null!));
    }

    [Fact]
    public void BuildSearch_ShouldCombineBaseWithPattern()
    {
        var sut = new KeyListStrategy<SampleType>("ROOT", new LogSequenceNumber());

        string result = sut.BuildSearch("PATTERN/**");

        result.Be("root/pattern/**");
    }

    [Fact]
    public void BuildKeySearch_ShouldAppendWildcard()
    {
        var sut = new KeyListStrategy<SampleType>("ROOT", new LogSequenceNumber());

        string result = sut.BuildKeySearch("KeyFolder");

        result.Be("root/keyfolder/keyfolder-*");
    }

    [Fact]
    public void BuildKeySearch2_ShouldAppendWildcard()
    {
        var sut = new KeyListStrategy<SampleType>("ROOT", new LogSequenceNumber());

        string result = sut.BuildKeySearch("KeyFolder/item");

        result.Be("root/keyfolder/item/keyfolder_item-*");
    }

    [Fact]
    public void ExtractKey_ShouldHandleListPath()
    {
        var sut = new KeyListStrategy<SampleType>("root", new LogSequenceNumber());

        string path = sut.BuildPath("Folder/Key").ToUpperInvariant();
        string result = sut.ExtractKey(path);

        result.Be("folder/key");
    }

    [Fact]
    public void ExtractKey_ShouldThrowWhenInvalidFormat()
    {
        var sut = new KeyListStrategy<SampleType>("root", new LogSequenceNumber());

        Verify.Throws<ArgumentException>(() => sut.ExtractKey("root/invalid?path"));
    }

    [Fact]
    public void ExtractKey_ShouldThrowOnInvalidFormat()
    {
        var sut = new KeyListStrategy<SampleType>("root", new LogSequenceNumber());

        Verify.Throws<ArgumentException>(() => sut.ExtractKey("root/key?name/list"));
    }

    [Fact]
    public void GetPathParts_ShouldReturnKeyAndSeq()
    {
        var sut = new KeyListStrategy<SampleType>("root", new LogSequenceNumber());

        string path = sut.BuildPath("folder/key");
        var (key, seq, typeName) = sut.GetPathParts(path);

        key.Be("folder/key");
        seq.NotEmpty();
        typeName.Be("sampletype");
    }

    [Fact]
    public void GetPathParts_ShouldThrowOnInvalidFormat()
    {
        var sut = new KeyListStrategy<SampleType>("root", new LogSequenceNumber());

        Verify.Throws<ArgumentException>(() => sut.GetPathParts("root/folder/key.sampletype.json"));
    }

    [Fact]
    public void IsValidPath_ShouldValidateCorrectly()
    {
        var sut = new KeyListStrategy<SampleType>("root", new LogSequenceNumber());
        string valid = sut.BuildPath("folder/key");

        sut.IsValidPath(valid).BeTrue();
        sut.IsValidPath("root/invalid?path").BeFalse();
    }

    [Fact]
    public void BuildDeleteFolder_ShouldReturnFolder()
    {
        var sut = new KeyListStrategy<SampleType>("Root", new LogSequenceNumber());

        string folder = sut.BuildDeleteFolder("Folder/Key");

        folder.Be("Root/Folder/Key".ToLowerInvariant());
    }

    [Fact]
    public void RemoveBasePath_ShouldStripBaseCaseInsensitive()
    {
        var sut = new KeyListStrategy<SampleType>("root/path", new LogSequenceNumber());

        string result = sut.RemoveBasePath("ROOT/PATH/item/sub");

        result.Be("item/sub");
    }

    [Fact]
    public void RemoveBasePath_ShouldReturnOriginalWhenBaseMissing()
    {
        var sut = new KeyListStrategy<SampleType>("root/path", new LogSequenceNumber());

        string result = sut.RemoveBasePath("other/item");

        result.Be("other/item");
    }

    [Fact]
    public void RemoveBasePath_ShouldThrowWhenNull()
    {
        var sut = new KeyListStrategy<SampleType>("root/path", new LogSequenceNumber());

        Verify.Throws<ArgumentNullException>(() => sut.RemoveBasePath(null!));
    }
}
