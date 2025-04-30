using Toolbox.Tools;
using Toolbox.Tools.Should;

namespace Toolbox.Test.Tools;

public class PathToolTests
{
    [Fact]
    public void ToExtensionTest()
    {
        string extension = null!;
        Verify.Throw<ArgumentNullException>(() => PathTool.ToExtension(extension));

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

        Verify.Throw<ArgumentNullException>(() => PathTool.SetExtension(null!, extension));
        Verify.Throw<ArgumentNullException>(() => PathTool.SetExtension("", extension));
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
