using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Types;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class CursorTests
    {
        [Fact]
        public void GivenCursor_EmptyList_ShouldPassTest()
        {
            var list = new List<int>();
            var cursor = new Cursor<int>(list);

            cursor.List.Count.Should().Be(0);
            cursor.Index.Should().Be(-1);
            cursor.Current.Should().Be(default);
            cursor.IsCursorAtEnd.Should().BeTrue();
            cursor.TryNextValue(out int value1).Should().BeFalse();
            cursor.TryPeekValue(out int value2).Should().BeFalse();
        }

        [Fact]
        public void GivenCursor_OneList_ShouldPassTest()
        {
            const int max = 1;
            var list = new List<int>(Enumerable.Range(0, max));
            var cursor = list.ToCursor();

            cursor.List.Count.Should().Be(max);
            cursor.Index.Should().Be(-1);
            cursor.Current.Should().Be(default);
            cursor.IsCursorAtEnd.Should().BeTrue();
            cursor.TryPeekValue(out int value2).Should().BeTrue();

            cursor.TryNextValue(out int value1).Should().BeTrue();
            value1.Should().Be(0);
            cursor.Index.Should().Be(0);
            cursor.Current.Should().Be(0);
            cursor.IsCursorAtEnd.Should().BeFalse();

            cursor.TryNextValue(out int value3).Should().BeFalse();
            cursor.IsCursorAtEnd.Should().BeTrue();
        }

        [Fact]
        public void GivenCursor_TwoList_ShouldPassTest()
        {
            const int max = 10;
            var list = new List<int>(Enumerable.Range(0, max));
            var cursor = list.ToCursor();

            cursor.List.Count.Should().Be(max);
            cursor.Index.Should().Be(-1);
            cursor.Current.Should().Be(default);
            cursor.IsCursorAtEnd.Should().BeTrue();

            int expectedValue = 0;
            while (cursor.TryNextValue(out int value))
            {
                value.Should().Be(expectedValue);

                cursor.Index.Should().Be(expectedValue);
                cursor.Current.Should().Be(expectedValue);

                expectedValue++;
            }

            cursor.IsCursorAtEnd.Should().BeTrue();

            cursor.Reset();
            expectedValue = 0;
            while (cursor.TryNextValue(out int value))
            {
                value.Should().Be(expectedValue);

                cursor.Index.Should().Be(expectedValue);
                cursor.Current.Should().Be(expectedValue);

                expectedValue++;
            }

            cursor.IsCursorAtEnd.Should().BeTrue();
        }
    }
}
