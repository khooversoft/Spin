using FluentAssertions;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class PathToolTests
{
    [Fact]
    public void ToExtensionTest()
    {
        string extension = null!;
        Action action = () => PathTool.ToExtension(extension);
        action.Should().Throw<ArgumentNullException>();

        extension = "ext";
        string path = PathTool.ToExtension(extension);
        path.Should().Be(".ext");

        extension = ".ext";
        path = PathTool.ToExtension(extension);
        path.Should().Be(".ext");
    }

    [Fact]
    public void SetSingleExtensionNullAndEmptyTest()
    {
        string extension = ".json";

        Action action = () => PathTool.SetExtension(null!, extension);
        action.Should().Throw<ArgumentNullException>();

        action = () => PathTool.SetExtension("", extension);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetSingleExtensionTest()
    {
        string extension = ".json";

        string path = "file.json";
        string result = PathTool.SetExtension(path, extension);
        result.Should().Be("file.json");

        path = "file.txt";
        result = PathTool.SetExtension(path, extension);
        result.Should().Be("file.json");

        path = "c:\\temp\\file.json";
        result = PathTool.SetExtension(path, extension);
        result.Should().Be("c:\\temp\\file.json");

        path = "c:\\temp\\file.txt";
        result = PathTool.SetExtension(path, extension);
        result.Should().Be("c:\\temp\\file.json");
    }

    [Fact]
    public void SetDoubleExtensionTest()
    {
        string extension = ".project.json";

        string path = "file.json";
        string result = PathTool.SetExtension(path, extension);
        result.Should().Be("file.project.json");

        path = "file.project.json";
        result = PathTool.SetExtension(path, extension);
        result.Should().Be("file.project.json");

        path = "file.project.txt";
        result = PathTool.SetExtension(path, extension);
        result.Should().Be("file.project.json");

        path = "c:\\temp\\file.project.json";
        result = PathTool.SetExtension(path, extension);
        result.Should().Be("c:\\temp\\file.project.json");

        path = "c:\\temp\\file.project.txt";
        result = PathTool.SetExtension(path, extension);
        result.Should().Be("c:\\temp\\file.project.json");
    }
}
