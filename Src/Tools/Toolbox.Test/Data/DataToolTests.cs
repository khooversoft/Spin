using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Data;

namespace Toolbox.Test.Data;

public class DataToolTests
{
    [Fact]
    public void FilterAllInArray()
    {
        byte[] buffer = [1, 2, 3, 4, 5, 6];
        byte[] result = DataTool.Filter(buffer, x => false);

        byte[] matchTo = [];

        Enumerable.SequenceEqual(result, matchTo).Should().BeTrue();
        Enumerable.SequenceEqual(result, matchTo.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void NoneFilterArray()
    {
        byte[] buffer = [1, 2, 3, 4, 5, 6];
        byte[] result = DataTool.Filter(buffer, x => true);

        Enumerable.SequenceEqual(buffer, result).Should().BeTrue();
        Enumerable.SequenceEqual(buffer, result.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void FilteredArray()
    {
        byte[] buffer = [1, 2, 3, 4, 5, 6];
        byte[] result = DataTool.Filter(buffer, x => x != 3);

        byte[] matchTo = [1, 2, 4, 5, 6];

        Enumerable.SequenceEqual(result, matchTo).Should().BeTrue();
        Enumerable.SequenceEqual(result, matchTo.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void FilteredAndConvertArray()
    {
        byte[] buffer = [1, 2, 3, 4, 5, 6];
        byte[] result = DataTool.Filter(buffer, x => x != 3, x => x == 5 ? (byte)10 : x);

        byte[] matchTo = [1, 2, 4, 10, 6];

        Enumerable.SequenceEqual(result, matchTo).Should().BeTrue();
        Enumerable.SequenceEqual(result, matchTo.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void FilterAllInString()
    {
        string str = "this is a test";
        string result = DataTool.Filter(str, x => false);

        string matchTo = "";

        Enumerable.SequenceEqual(result, matchTo).Should().BeTrue();
        Enumerable.SequenceEqual(result, matchTo.ToArray()).Should().BeTrue();
    }
    [Fact]
    public void NoneFilterString()
    {
        string str = "this is a test";
        string result = DataTool.Filter(str, x => true);

        Enumerable.SequenceEqual(str, result).Should().BeTrue();
        Enumerable.SequenceEqual(str, result.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void FilteredString()
    {
        string str = "this is a test";
        string result = DataTool.Filter(str, x => x != 'i');

        string matchTo = "ths s a test";

        Enumerable.SequenceEqual(result, matchTo).Should().BeTrue();
        Enumerable.SequenceEqual(result, matchTo.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void FilteredAndConvertString()
    {
        string str = "this is a test";
        string result = DataTool.Filter(str, x => x != 'i', x => x == 'a' ? 'Z' : x);

        string matchTo = "ths s Z test";

        result.Should().Be(matchTo);
        Enumerable.SequenceEqual(result, matchTo.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void FilteredOutUnicodeString()
    {
        string str = "this \u00B3is a\u0060 test";
        string result = DataTool.Filter(str, DataTool.IsAsciiRange, x => x == 0x60 ? '`' : x);

        string matchTo = "this is a` test";

        Enumerable.SequenceEqual(result, matchTo).Should().BeTrue();
        Enumerable.SequenceEqual(result, matchTo.ToArray()).Should().BeTrue();
    }

    [Fact]
    public void FilterComplex()
    {
        string line = """
            ### LINQ
            This feature allows you to write declarative queries over collections of data using a set of extension methods and query expressions. LINQ supports functional operations such as mapping, filtering, sorting, grouping, and aggregating.

            ```csharp
            // Example of LINQ extension methods
            """;

        string matchTo = """
                LINQ  This feature allows you to write declarative queries over collections of data using a set of extension methods and query expressions  LINQ supports functional operations such as mapping  filtering  sorting  grouping  and aggregating        csharp     Example of LINQ extension methods
            """;

        string clean = Clean(line);
        clean.Should().Be(matchTo);
    }

    private static string Clean(string line)
    {
        return DataTool.Filter(line, _ => true, convert);

        char convert(char chr)
        {
            if (!char.IsAsciiLetterOrDigit(chr)) return ' ';
            return chr;
        }
    }
}
