using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Tools;

public class DirectoryToolTests
{
    [Fact]
    public void TestDirectoryFind()
    {
        string[] roots = [@"d:\sources", @"c:\sources"];
        var stack = roots.Reverse().ToStack();

        while (stack.TryPop(out var rootPath))
        {
            var list = DirectoryTool.Find(rootPath, "NBlogArticles\\Src");
            if (list.Count == 0) continue;

            string matchTo = Path.Combine(rootPath, "NBlogArticles", "Src");
            list[0].Should().Be(matchTo);
            return;
        }

        throw new Exception("failed");
    }
}
