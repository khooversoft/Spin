using FluentAssertions;
using Toolbox.Types;

namespace Toolbox.Test.Types;

public class TagsTests
{
    [Fact]
    public void TagsEmpty()
    {
        var tags = new Tags();
        tags.Count.Should().Be(0);

        var tags2 = new Tags();
        tags2.Count.Should().Be(0);

        (tags == tags2).Should().BeTrue();
        (tags.ToString() == tags2.ToString()).Should().BeTrue();

        var tags3 = Tags.Parse("");
        (tags == tags3).Should().BeTrue();

        var tags4 = Tags.Parse(null!);
        (tags == tags4).Should().BeTrue();
    }

    [Fact]
    public void TagsSingle()
    {
        var tags = new Tags().Set("key1");
        tags.Count.Should().Be(1);
        tags.ContainsKey("key1").Should().BeTrue();
        tags["key1"].Should().BeNull();

        var tags2 = new Tags().Set("key1=value1");
        tags2.Count.Should().Be(1);
        tags2.ContainsKey("key1").Should().BeTrue();
        tags2["key1"].Should().Be("value1");
    }

    [Fact]
    public void TagsKeyValue()
    {
        var tags = new Tags();
        tags["key2"] = "value2";
        tags["key1"] = "value1";
        tags.Count.Should().Be(2);
        tags.ContainsKey("key1").Should().BeTrue();

        var tags2 = new Tags();
        tags2["key1"] = "value1";
        tags2["key2"] = "value2";
        tags2.Count.Should().Be(2);

        (tags == tags2).Should().BeTrue();
        (tags.ToString() == tags2.ToString()).Should().BeFalse();
        (tags.ToString(true) == tags2.ToString(true)).Should().BeTrue();

        var tags3 = Tags.Parse("key2=value2;key1=value1");
        (tags == tags3).Should().BeTrue();

        var tags4 = Tags.Parse("key2=value2");
        (tags == tags4).Should().BeFalse();
    }

    [Fact]
    public void TagsUsingSet()
    {
        var tags = new Tags();
        tags.Set("key2=value2");
        tags.Set("key1=value1");
        tags.Count.Should().Be(2);

        var tags2 = new Tags();
        tags2["key1"] = "value1";
        tags2["key2"] = "value2";
        tags2.Count.Should().Be(2);
        (tags == tags2).Should().BeTrue();
        (tags.ToString(true) == tags2.ToString(true)).Should().BeTrue();

        var tags3 = new Tags();
        tags3.Set("key2=value2;key1=value1");
        tags3.Count.Should().Be(2);
        (tags == tags3).Should().BeTrue();
        (tags.ToString(true) == tags3.ToString(true)).Should().BeTrue();

        var tags4 = Tags.Parse("key2=value2;key1=value1");
        tags4.Count.Should().Be(2);
        (tags == tags4).Should().BeTrue();
        (tags.ToString(true) == tags4.ToString(true)).Should().BeTrue();
    }

    [Fact]
    public void TagsStageTags()
    {
        var tags = new Tags();
        tags.Set("key2");
        tags.Set("key1=value1");
        tags.Count.Should().Be(2);
        tags.ContainsKey("key2").Should().BeTrue();

        var tags2 = new Tags();
        tags2["key1"] = "value1";
        tags2["key2"] = null;
        tags2.Count.Should().Be(2);
        (tags == tags2).Should().BeTrue();
        (tags.ToString(true) == tags2.ToString(true)).Should().BeTrue();

        var tags3 = new Tags();
        tags3.Set("key2;key1=value1");
        tags3.Count.Should().Be(2);
        (tags == tags3).Should().BeTrue();
        (tags.ToString(true) == tags3.ToString(true)).Should().BeTrue();

        var tags4 = Tags.Parse("key2;key1=value1");
        tags4.Count.Should().Be(2);
        (tags == tags4).Should().BeTrue();
        (tags.ToString(true) == tags4.ToString(true)).Should().BeTrue();
    }
}
