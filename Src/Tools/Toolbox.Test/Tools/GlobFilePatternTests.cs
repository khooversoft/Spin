using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Tools;

public class GlobFilePatternTests
{
    [Theory]
    [InlineData("file.json", "file.json", true)]
    [InlineData("file2.json", "file.json", false)]

    [InlineData("file.json", "*", true)]
    [InlineData("file2.json", "*", true)]
    [InlineData("folder1/file.json", "*", false)]

    [InlineData("file.json", "*.json", true)]
    [InlineData("file2.json", "*.json", true)]
    [InlineData("file2.json", "*2.json", true)]
    [InlineData("file2.json", "*3.json", false)]
    [InlineData("nodes/path/path1/file.cpp", "nodes/path/path1/*.?pp", true)]
    [InlineData("nodes/path/path1/file.cpp", "nodes/path/path1/*.c?p", true)]

    [InlineData("file.md", "**/*.md", true)]
    [InlineData("nodes/path/file.md", "**/*.md", true)]
    [InlineData("nodes/path/file.cpp", "**/*.md", false)]
    [InlineData("nodes/path/file.md", "nodes/**/*.md", true)]
    [InlineData("nodes/path/file.cpp", "nodes/**/*.md", false)]
    [InlineData("nodes/path/path1/file.md", "nodes/**/path1/*.md", true)]
    [InlineData("nodes/path/path2/file.md", "nodes/**/path1/*.md", false)]
    [InlineData("nodes/path/path1/file.cpp", "nodes/**/path1/*.md", false)]
    public void MatchExact(string file, string pattern, bool expected)
    {
        var matcher = new GlobFileMatching(pattern);
        matcher.IsMatch(file).Should().Be(expected);
    }
}
