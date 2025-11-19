using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class PathToolTests
{
    [Fact]
    public void ToExtensionTest()
    {
        string extension = null!;
        Verify.Throws<ArgumentNullException>(() => PathTool.ToExtension(extension));

        extension = "ext";
        string path = PathTool.ToExtension(extension);
        path.Be(".ext");

        extension = ".ext";
        path = PathTool.ToExtension(extension);
        path.Be(".ext");
    }

    [Fact]
    public void SetSingleExtensionNullAndEmptyTest()
    {
        string extension = ".json";

        Verify.Throws<ArgumentNullException>(() => PathTool.SetExtension(null!, extension));
        Verify.Throws<ArgumentNullException>(() => PathTool.SetExtension("", extension));
    }

    [Fact]
    public void SetSingleExtensionTest()
    {
        string extension = ".json";

        string path = "file.json";
        string result = PathTool.SetExtension(path, extension);
        result.Be("file.json");

        path = "file.txt";
        result = PathTool.SetExtension(path, extension);
        result.Be("file.json");

        path = "c:\\temp\\file.json";
        result = PathTool.SetExtension(path, extension);
        result.Be("c:\\temp\\file.json");

        path = "c:\\temp\\file.txt";
        result = PathTool.SetExtension(path, extension);
        result.Be("c:\\temp\\file.json");
    }

    [Fact]
    public void SetDoubleExtensionTest()
    {
        string extension = ".project.json";

        string path = "file.json";
        string result = PathTool.SetExtension(path, extension);
        result.Be("file.project.json");

        path = "file.project.json";
        result = PathTool.SetExtension(path, extension);
        result.Be("file.project.json");

        path = "file.project.txt";
        result = PathTool.SetExtension(path, extension);
        result.Be("file.project.json");

        path = "c:\\temp\\file.project.json";
        result = PathTool.SetExtension(path, extension);
        result.Be("c:\\temp\\file.project.json");

        path = "c:\\temp\\file.project.txt";
        result = PathTool.SetExtension(path, extension);
        result.Be("c:\\temp\\file.project.json");
    }
}
