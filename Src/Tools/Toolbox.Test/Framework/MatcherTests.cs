using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using FluentAssertions;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Toolbox.Test.Framework;

public class MatcherTests
{
    [Fact]
    public void SimpleMatcher()
    {
        Matcher matcher = new();
        matcher.AddInclude("**/*.md");

        PatternMatchingResult result = matcher.Match("file.md");
        result.Files.Count().Should().Be(1);
        result.HasMatches.Should().BeTrue();

        result = matcher.Match("nodes/path/file.md");
        result.Files.Count().Should().Be(1);
        result.HasMatches.Should().BeTrue();

        result = matcher.Match("nodes/path/file.mdx");
        result.Files.Count().Should().Be(0);
        result.HasMatches.Should().BeFalse();
    }
    
    [Fact]
    public void MulfipleFilesMatcher()
    {
        Matcher matcher = new();
        matcher.AddInclude("**/*.md");

        string[] files = [
            "file.md",
            "nodes/path/file.md",
            "nodes/path/file.cpp",
            ];

        PatternMatchingResult result = matcher.Match(files);
        result.Files.Count().Should().Be(2);
        result.HasMatches.Should().BeTrue();

        string[] shouldMatch = [
            "file.md",
            "nodes/path/file.md",
            ];

        result.Files.Select(x => x.Path).Should().BeEquivalentTo(shouldMatch);
    }

    [Theory]
    [InlineData("file.cpp", "*.md", 0, false, new string[0])]
    [InlineData("file.cpp", "*.cpp", 1, true, new string[] { "file.cpp" })]
    [InlineData("nodes/data/file.cpp", "**/*.cpp", 1, true, new string[] { "nodes/data/file.cpp" })]
    public void TestPatterns(string file, string pattern, int count, bool hasMatch, string[] files)
    {
        Matcher matcher = new();
        matcher.AddInclude(pattern);

        PatternMatchingResult result = matcher.Match(file);
        result.Files.Count().Should().Be(count);
        result.HasMatches.Should().Be(hasMatch);

        result.Files.Select(x => x.Path).Should().BeEquivalentTo(files);
    }

    [Theory]
    [InlineData("file.cpp", "*.md", 0, new string[0])]
    [InlineData("file.cpp", "*.cpp", 1, new string[] { "file.cpp" })]
    [InlineData("nodes/data/file.cpp", "**/*.cpp", 1, new string[] { "nodes/data/file.cpp" })]
    public void UseMatchPatterns(string file, string pattern, int count, string[] files)
    {
        var result = file.ToEnumerable().Match(pattern);
        result.Count.Should().Be(count);

        result.Should().BeEquivalentTo(files);
    }
}
