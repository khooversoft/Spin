using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class GlobFilePatternTests
{
    [Theory]
    [InlineData("file.json", "file.json", true, false)]
    [InlineData("file2.json", "file.json", false, false)]

    [InlineData("file.json", "*", true, false)]
    [InlineData("file2.json", "*", true, false)]
    [InlineData("folder1/file.json", "*", false, false)]

    [InlineData("file.json", "*.json", true, false)]
    [InlineData("file2.json", "*.json", true, false)]
    [InlineData("file2.json", "*2.json", true, false)]
    [InlineData("file2.json", "*3.json", false, false)]
    [InlineData("nodes/path/path1/file.cpp", "nodes/path/path1/*.?pp", true, false)]
    [InlineData("nodes/path/path1/file.cpp", "nodes/path/path1/*.c?p", true, false)]

    [InlineData("file.md", "**/*.md", true, true)]
    [InlineData("nodes/path/file.md", "**/*.md", true, true)]
    [InlineData("nodes/path/file.cpp", "**/*.md", false, true)]
    [InlineData("nodes/path/file.md", "nodes/**/*.md", true, true)]
    [InlineData("nodes/path/file.cpp", "nodes/**/*.md", false, true)]
    [InlineData("nodes/path/path1/file.md", "nodes/**/path1/*.md", true, true)]
    [InlineData("nodes/path/path2/file.md", "nodes/**/path1/*.md", false, true)]
    [InlineData("nodes/path/path1/file.cpp", "nodes/**/path1/*.md", false, true)]
    [InlineData("/journal3/data/202506/20250622.journal3Key.json", "journal3/data/**/*", false, true)]

    [InlineData("nodes/path/path1/file.json", "**", true, true)]
    [InlineData("folder1/file.json", "folder1/*", true, false)]
    [InlineData("folder1/sub/file.json", "folder1/*", false, false)]
    [InlineData("folder1/abc123/file.txt", "folder1/abc*/**", true, true)]
    [InlineData("folder1/abc/file1.json", "folder1/**/file?.json", true, true)]
    [InlineData("folder1/abc/def/file1.json", "folder1/**/file?.json", true, true)]
    [InlineData("folder1/abc/file10.json", "folder1/**/file?.json", false, false)]
    [InlineData("folder1/x/y/abc123.json", "folder1/*/*/abc*.json", true, true)]
    [InlineData("folder1/x/y/z/abc123.json", "folder1/*/*/abc*.json", false, false)]
    [InlineData("folder1/deep/path/file.json", "**/*.json", true, true)]
    [InlineData("folder1\\deep\\path\\file.json", "**/*.json", true, true)]

    [InlineData("hashFolder1/a0//b5/path/file.json", "hashFolder1/*/*/path/*.json", true, true)]
    [InlineData("hashFolder1/a0/b5/path/file.json", "hashFolder1/*/*/path/*.json", true, true)]
    [InlineData("hashFolder1/a0/b5//a6/path/file.json", "hashFolder1/*/*/path/*.json", false, false)]
    [InlineData("hashfiles/a5/b2/hashfile6.json", "hashfiles/*/*/*.json", true, true)]
    [InlineData("hashfiles/a5/b2/hashfile6.json", "hashfiles/*/*/hashfile*.json", true, true)]
    [InlineData("hashfiles/a5/b2/hashfile6.json", "hashfiles/*/*/hashfile6.json", true, true)]
    public void MatchExact(string file, string pattern, bool expected, bool recursive)
    {
        var matcher = new GlobFileMatching(pattern);
        matcher.IsMatch(file).Be(expected);
        if (expected) matcher.IsRecursive.Be(recursive);
    }

    [Theory]
    [InlineData("folder1/path/file.md", "***/*.md", true, true)]
    [InlineData("folder1/path/file.md", "**/*.md", true, false)]
    [InlineData("folder1/path/file.json", "***/*.md", false, true)]
    public void IncludeFoldersFlag(string file, string pattern, bool expectedMatch, bool includeFolders)
    {
        var matcher = new GlobFileMatching(pattern);

        matcher.IsMatch(file).Be(expectedMatch);
        matcher.IncludeFolders.Be(includeFolders);
    }
}
