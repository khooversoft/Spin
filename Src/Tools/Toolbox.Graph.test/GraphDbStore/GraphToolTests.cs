using Toolbox.Tools;

namespace Toolbox.Graph.test.GraphDbStore;

public class GraphToolTests
{
    [Theory]
    [InlineData("n", "nodes/n/n___main.json")]
    [InlineData("node1", "nodes/node1/node1___main.json")]
    [InlineData("user:user001", "nodes/user/user001/user__user001___main.json")]
    [InlineData("data/node1", "nodes/data/node1/data___node1___main.json")]
    [InlineData("data:node1", "nodes/data/node1/data__node1___main.json")]
    [InlineData("data:folder1/node1", "nodes/data/folder1/node1/data__folder1___node1___main.json")]
    [InlineData("data/company.com", "nodes/data/company.com/data___company.com___main.json")]
    [InlineData("Data/User1@company.com/node1", "nodes/data/user1@company.com/node1/data___user1@company.com___node1___main.json")]
    public void CreateFileId(string source, string expected)
    {
        string result = GraphTool.CreateFileId(source, "main");
        result.Be(expected);
    }
}
