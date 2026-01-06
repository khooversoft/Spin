using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Test.Store;

public class StorePathToolTests
{
    [Theory]
    [InlineData("data", "data")]
    [InlineData("DATA", "data")]
    [InlineData("data/path/*", "data/path")]
    [InlineData("data/path/**", "data/path")]
    [InlineData("data/path*", "data")]
    [InlineData("data*", "")]
    [InlineData("data/path/*.file.txt", "data/path")]
    [InlineData("data/path/*.txt", "data/path")]
    [InlineData("data/path/file*.txt", "data/path")]
    [InlineData("data/path/fi*le*.txt", "data/path")]
    [InlineData("data/path/**/*.json", "data/path")]
    public void GetRootPath(string path, string expected)
    {
        string result = StorePathTool.GetRootPath(path);
        result.Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetRootPath_ShouldThrowOnEmptyOrWhitespace(string path)
    {
        Verify.Throws<ArgumentNullException>(() => StorePathTool.GetRootPath(path));
    }

    [Fact]
    public void GetRootPath_ShouldThrowOnNull()
    {
        Verify.Throws<ArgumentNullException>(() => StorePathTool.GetRootPath(null!));
    }
}
