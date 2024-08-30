using FluentAssertions;
using Toolbox.Data;
using Toolbox.Extensions;

namespace Toolbox.Test.Data;

public class DictionaryListTests
{
    private record TestRecord(string Name, int Age);

    [Fact]
    public void TestEmpty()
    {
        var dictList = new DictionaryList<string, TestRecord>(x => x.Name);
        dictList.Count.Should().Be(0);
        dictList.TryGetValue("*", out var x).Should().BeFalse();
    }

    [Fact]
    public void AddSingle()
    {
        var dictList = new DictionaryList<string, TestRecord>(x => x.Name);
        dictList.Add(new TestRecord("A", 1));
        dictList.Count.Should().Be(1);

        dictList.TryGetValue("A", out var readRecord).Should().BeTrue();
        readRecord.Should().Be(new TestRecord("A", 1));

        dictList.Clear();
        dictList.Count.Should().Be(0);
        dictList.TryGetValue("A", out readRecord).Should().BeFalse();
    }

    [Fact]
    public void AddSingleRemove()
    {
        var dictList = new DictionaryList<string, TestRecord>(x => x.Name);
        dictList.Add(new TestRecord("A", 1));
        dictList.Count.Should().Be(1);

        dictList.TryGetValue("A", out var readRecord).Should().BeTrue();
        readRecord.Should().Be(new TestRecord("A", 1));

        dictList.Remove("A");
        dictList.Count.Should().Be(0);
        dictList.TryGetValue("A", out readRecord).Should().BeFalse();
    }

    [Fact]
    public void AddSingleRemoveIndex()
    {
        var dictList = new DictionaryList<string, TestRecord>(x => x.Name);
        dictList.Add(new TestRecord("A", 1));
        dictList.Count.Should().Be(1);

        dictList.TryGetValue("A", out var readRecord).Should().BeTrue();
        readRecord.Should().Be(new TestRecord("A", 1));

        dictList.Remove(0).Should().BeTrue();
        dictList.Count.Should().Be(0);
        dictList.TryGetValue("A", out readRecord).Should().BeFalse();
    }

    [Fact]
    public void IndexList()
    {
        var dictList = new DictionaryList<string, TestRecord>(x => x.Name)
        {
            new TestRecord("name1", 10),
            new TestRecord("name2", 20),
            new TestRecord("name3", 30),
            new TestRecord("name4", 40),
            new TestRecord("name5", 50),
            new TestRecord("name6", 60),
        };

        string[] nameList = ["name1", "name2", "name3", "name4", "name5", "name6"];

        dictList.Count.Should().Be(nameList.Length);

        for (int i = 0; i < nameList.Length; i++)
        {
            TestRecord expectedRecord = new TestRecord(nameList[i], (i + 1) * 10);

            dictList.TryGetValue(nameList[i], out var readRecord).Should().BeTrue();
            readRecord.Should().Be(expectedRecord);

            dictList.TryGetValue(i, out var readRecordIndex).Should().BeTrue();
            readRecordIndex.Should().Be(expectedRecord);

            dictList[i].Action(x => x.Should().Be(expectedRecord));
        }

        nameList.ForEach(x => dictList.Remove(x).Should().BeTrue());
        dictList.Count.Should().Be(0);
    }

    [Fact]
    public void ListFlows()
    {
        TestRecord[] list = [
            new TestRecord("name1", 10),
            new TestRecord("name2", 20),
            new TestRecord("name3", 30),
            new TestRecord("name4", 40),
            new TestRecord("name5", 50),
            new TestRecord("name6", 60),
        ];

        var dictList = new DictionaryList<string, TestRecord>(x => x.Name, list);

        string[] nameList = ["name1", "name2", "name3", "name4", "name5", "name6"];
        nameList = [.. nameList.Shuffle()];

        dictList.Count.Should().Be(nameList.Length);

        for (int i = 0; i < nameList.Length; i++)
        {
            dictList.TryGetValue(nameList[i], out var readRecord).Should().BeTrue();
            readRecord!.Name.Should().Be(nameList[i]);

            dictList[nameList[i]].Action(x => x.Name.Should().Be(nameList[i]));
        }

        nameList.ForEach(x => dictList.Remove(x).Should().BeTrue());
        dictList.Count.Should().Be(0);
    }
}
