using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class QueryParameterTests
{
    [Fact]
    public void OnlyQuery()
    {
        QueryParameter.Parse("base/path.json").Action(result =>
        {
            result.Index.Should().Be(0);
            result.Count.Should().Be(1000);
            result.Filter.Should().Be("base/path.json");
            result.Recurse.Should().BeFalse();
            result.IncludeFile.Should().BeTrue();
            result.IncludeFolder.Should().BeFalse();
            result.BasePath.Should().Be("base/path.json");

            result.GetMatcher().IsMatch("base/Path.json", false).Should().BeTrue();
            result.GetMatcher().IsMatch("base/Path.json", true).Should().BeFalse();
            result.GetMatcher().IsMatch("no", false).Should().BeFalse();
            result.GetMatcher().IsMatch("base", false).Should().BeFalse();
            result.GetMatcher().IsMatch("base/Path", false).Should().BeFalse();
            result.GetMatcher().IsMatch("base/newfolder/Path.json", false).Should().BeFalse();
        });

        QueryParameter.Parse("filter=base/path.json").Action(result =>
        {
            result.Index.Should().Be(0);
            result.Count.Should().Be(1000);
            result.Filter.Should().Be("base/path.json");
            result.Recurse.Should().BeFalse();
            result.IncludeFile.Should().BeTrue();
            result.IncludeFolder.Should().BeFalse();
            result.BasePath.Should().Be("base/path.json");
        });
    }

    [Fact]
    public void RecursiveSearchQuery()
    {
        QueryParameter.Parse("data/**/*").Action(result =>
        {
            result.Index.Should().Be(0);
            result.Count.Should().Be(1000);
            result.Filter.Should().Be("data/**/*");
            result.Recurse.Should().BeTrue();
            result.IncludeFile.Should().BeTrue();
            result.IncludeFolder.Should().BeFalse();
            result.BasePath.Should().Be("data");

            result.GetMatcher().Action(result =>
            {
                result.IsMatch("data", true).Should().BeFalse();
                result.IsMatch("data/json", false).Should().BeTrue();
                result.IsMatch("data/file.json", false).Should().BeTrue();
                result.IsMatch("data/folder1/file1", false).Should().BeTrue();
                result.IsMatch("data/folder1/folder2/file2.txt", false).Should().BeTrue();

                result.IsMatch("system", false).Should().BeFalse();
            });
        });
    }

    [Fact]
    public void Recursive()
    {
        QueryParameter.Parse("base/path/**").Action(result =>
        {
            result.Index.Should().Be(0);
            result.Count.Should().Be(1000);
            result.Filter.Should().Be("base/path/**");
            result.Recurse.Should().BeTrue();
            result.IncludeFile.Should().BeTrue();
            result.IncludeFolder.Should().BeFalse();
            result.BasePath.Should().Be("base/path");

            result.GetMatcher().IsMatch("base/Path/Path.json", false).Should().BeTrue();
            result.GetMatcher().IsMatch("base/Path/Path.json", true).Should().BeFalse();
            result.GetMatcher().IsMatch("base/Path/newfolder/Path.json", false).Should().BeTrue();
            result.GetMatcher().IsMatch("no", false).Should().BeFalse();
            result.GetMatcher().IsMatch("base", false).Should().BeFalse();
            result.GetMatcher().IsMatch("base/Path", false).Should().BeFalse();
            result.GetMatcher().IsMatch("base/Path.json", false).Should().BeFalse();
            result.GetMatcher().IsMatch("base/Path.json", true).Should().BeFalse();
        });

        QueryParameter.Parse("base/path/**.json").Action(result =>
        {
            result.Index.Should().Be(0);
            result.Count.Should().Be(1000);
            result.Filter.Should().Be("base/path/**.json");
            result.Recurse.Should().BeTrue();
            result.IncludeFile.Should().BeTrue();
            result.IncludeFolder.Should().BeFalse();
            result.BasePath.Should().Be("base/path");

            result.GetMatcher().Action(result =>
            {
                result.IsMatch("base/Path/Path.json", false).Should().BeTrue();
                result.IsMatch("base/Path/Path.json", true).Should().BeFalse();
                result.IsMatch("base/Path/newfolder/Path.json", false).Should().BeTrue();
                result.IsMatch("no", false).Should().BeFalse();
                result.IsMatch("base", false).Should().BeFalse();
                result.IsMatch("base/Path", false).Should().BeFalse();
                result.IsMatch("base/Path.json", false).Should().BeFalse();
                result.IsMatch("base/Path.json", true).Should().BeFalse();
            });
        });

        QueryParameter.Parse("base/path/*.json").Action(result =>
        {
            result.Index.Should().Be(0);
            result.Count.Should().Be(1000);
            result.Filter.Should().Be("base/path/*.json");
            result.Recurse.Should().BeFalse();
            result.IncludeFile.Should().BeTrue();
            result.IncludeFolder.Should().BeFalse();
            result.BasePath.Should().Be("base/path");

            result.GetMatcher().Action(result =>
            {
                result.IsMatch("base/Path/file.json", false).Should().BeTrue();
                result.IsMatch("base/Path/file2.json", false).Should().BeTrue();
                result.IsMatch("base/Path/file2.txt", false).Should().BeFalse();
                result.IsMatch("base/Path/file.json", true).Should().BeFalse();

                result.IsMatch("base/Path/newfolder/Path.json", false).Should().BeFalse();
                result.IsMatch("no", false).Should().BeFalse();
                result.IsMatch("base", false).Should().BeFalse();
                result.IsMatch("base/Path", false).Should().BeFalse();
                result.IsMatch("base/Path.json", false).Should().BeFalse();
                result.IsMatch("base/Path.json", true).Should().BeFalse();
            });
        });
    }

    [Fact]
    public void FullPropertySet()
    {
        QueryParameter result = QueryParameter.Parse("filter=base/path.json;index=1;count=2;recurse=true;includeFile=true;includeFolder=true");

        result.Index.Should().Be(1);
        result.Count.Should().Be(2);
        result.Filter.Should().Be("base/path.json");
        result.Recurse.Should().BeTrue();
        result.IncludeFile.Should().BeTrue();
        result.IncludeFolder.Should().BeTrue();
        result.BasePath.Should().Be("base/path.json");
    }
}
