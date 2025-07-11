using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Types;

namespace NBlog.sdk.test;

public class DbNameIdTests
{
    [Theory]
    [InlineData(null, false, null, null)]
    [InlineData("", false, null, null)]
    [InlineData("dbName", true, "dbName", null)]
    [InlineData("dbName", true, "dbName", new string[0])]
    [InlineData("dbName;", true, "dbName", null)]
    [InlineData("dbName ;", true, "dbName", null)]
    [InlineData("dbName;contextId", true, "dbName", (string[])["contextId"])]
    [InlineData("dbName; contextId", true, "dbName", (string[])["contextId"])]
    [InlineData(" dbName; contextId", true, "dbName", (string[])["contextId"])]
    [InlineData("dbName;cont%;extId", false, null, (string[])["contextId"])]
    [InlineData("dbName;contextId#-error", false, null, (string[])["contextId"])]
    [InlineData("dbName;contextId;context2", true, "dbName", (string[])["contextId", "context2"])]
    public void TestParser(string? id, bool expected, string? dbName, string[]? contexts)
    {
        Option<(string dbName, IReadOnlyList<string> contexts)> result = DbNameId.Parse(id);
        result.IsOk().Should().Be(expected);

        contexts = contexts ?? Array.Empty<string>();

        if (result.HasValue)
        {
            result.Value.dbName.Should().Be(dbName);
            Enumerable.SequenceEqual(result.Value.contexts, contexts).Should().BeTrue();
        }

        if (result.IsOk())
        {
            var newId = new DbNameId(result.Value.dbName, result.Value.contexts);
            newId.DbName.Should().Be(dbName);
            Enumerable.SequenceEqual(newId.Values, contexts).Should().BeTrue();

            (string dbName2, IReadOnlyList<string> values) = newId;
            dbName2.Should().Be(dbName);
            Enumerable.SequenceEqual(values, contexts).Should().BeTrue();

        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("dbName;cont&extId")]
    public void TestFailedConstructor(string? dbName)
    {
        Action action = () => new DbNameId(dbName!);
        action.Should().Throw<ArgumentException>();
    }
}
