using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class CursorTests
{
    [Fact]
    public void TestCursor()
    {
        int[] list = [1, 2, 3, 4, 5, 7, 8];

        var cursor = list.ToCursor();

        for (int i = 0; i < list.Length; i++)
        {
            cursor.NextValue().IsOk().Should().BeTrue();
            cursor.Index.Should().Be(i);
            cursor.Current.Should().Be(list[i]);
        }

        cursor.NextValue().IsNoContent().Should().BeTrue();
    }

    [Fact]
    public void TestTryCursor()
    {
        int[] list = [1, 2, 3, 4, 5, 7, 8];

        var cursor = list.ToCursor();

        int i = 0;
        while (cursor.TryGetValue(out var value))
        {
            cursor.Index.Should().Be(i);
            cursor.Current.Should().Be(list[i]);

            i++;
        }

        cursor.NextValue().IsNoContent().Should().BeTrue();
    }

    [Fact]
    public void TestPushPop()
    {
        int[] list = [1, 2, 3, 4, 5, 7, 8];

        var cursor = list.ToCursor();
        Enumerable.Range(0, 3).ForEach(_ => cursor.NextValue().IsOk().Should().BeTrue());
        cursor.Index.Should().Be(2);

        using (var scope = cursor.IndexScope.PushWithScope())
        {
            cursor.Index.Should().Be(2);
            Enumerable.Range(0, 3).ForEach(_ => cursor.NextValue().IsOk().Should().BeTrue());
            cursor.Index.Should().Be(5);
        }

        cursor.Index.Should().Be(2);
    }
}
