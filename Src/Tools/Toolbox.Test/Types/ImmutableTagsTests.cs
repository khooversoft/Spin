//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Test.Types;

//public class ImmutableTagsTests
//{

//    [Fact]
//    public void ImplicitConversion()
//    {
//        ImmutableTags t = "hello";
//        t.Should().NotBeNull();
//        t.ToString().Should().Be("hello");
//    }

//    [Fact]
//    public void TagsEmpty()
//    {
//        var tags = new ImmutableTags();
//        tags.Count.Should().Be(0);

//        var tags2 = new ImmutableTags();
//        tags2.Count.Should().Be(0);

//        (tags == tags2).Should().BeTrue();
//        (tags.ToString() == tags2.ToString()).Should().BeTrue();

//        var tags3 = new ImmutableTags("");
//        (tags == tags3).Should().BeTrue();

//        var tags4 = new ImmutableTags((string)null!);
//        (tags == tags4).Should().BeTrue();
//    }


//    [Theory]
//    [InlineData(null)]
//    [InlineData("")]
//    [InlineData("k")]
//    [InlineData("key=node1")]
//    [InlineData("key=node1, t1, t2=v2")]
//    [InlineData("key=node1, t1, t2=v2, i:logonProvider={LoginProvider}/{ProviderKey}")]
//    [InlineData("key=node1, t1, t2=v2, unique:logonProvider={LoginProvider}/{ProviderKey}")]
//    [InlineData("key=node1, t1, t2=v2, unique:logonProvider=microsoft/user001-microsoft-id")]
//    [InlineData("key=node1, t1, t2=v2, unique:logonProvider='microsoft user001-microsoft-id'")]
//    public void EqualPatterns(string? line)
//    {
//        var t1 = new ImmutableTags(line);
//        var t2 = new ImmutableTags(line);

//        (t1 == t2).Should().BeTrue();
//        (t1 != t2).Should().BeFalse();
//    }


//    [Fact]
//    public void TagsSingleAndNotEqual()
//    {
//        var tags = new ImmutableTags("key1").Action(x =>
//        {
//            x.Count.Should().Be(1);
//            x.ContainsKey("key1").Should().BeTrue();
//            x["key1"].Should().BeNull();
//            x.Has("key1").Should().BeTrue();
//            x.Has("key1", "value").Should().BeFalse();
//            x.Has("fake").Should().BeFalse();
//            x.Has("key1", "fake1").Should().BeFalse();
//            x.Has(null).Should().BeFalse();
//        });

//        var tags2 = new ImmutableTags("key1=value1").Action(x =>
//        {
//            x.Count.Should().Be(1);
//            x.ContainsKey("key1").Should().BeTrue();
//            x["key1"].Should().Be("value1");
//            x.Has("key1").Should().BeTrue();
//            x.Has("key1", "value1").Should().BeTrue();
//            x.Has("fake").Should().BeFalse();
//            x.Has("key1", "fake1").Should().BeFalse();
//        });

//        (tags != tags2).Should().BeTrue();
//    }

//    [Fact]
//    public void TagsUsingTags()
//    {
//        var tags = new ImmutableTags("key2, key1=value1");
//        tags.Count.Should().Be(2);
//        tags.ContainsKey("key2").Should().BeTrue();

//        var tags2 = new ImmutableTags("key1=value1, key2");
//        tags2.Count.Should().Be(2);
//        (tags == tags2).Should().BeTrue();
//        (tags.ToString() == tags2.ToString()).Should().BeTrue();

//        var tags3 = new ImmutableTags("key2,key1=value1");
//        tags3.Count.Should().Be(2);
//        (tags == tags3).Should().BeTrue();
//        (tags.ToString() == tags3.ToString()).Should().BeTrue();
//    }

//    [Fact]
//    public void TagSerialization()
//    {
//        var tags = new ImmutableTags("key2,key1=value1");

//        string json = tags.ToJson();

//        ImmutableTags readTags = json.ToObject<ImmutableTags>().NotNull();
//        readTags.Should().NotBeNull();

//        (tags == readTags).Should().BeTrue();

//        readTags.Has("key2").Should().BeTrue();
//        readTags.Has("key1", "value1").Should().BeTrue();
//        readTags.Has("key1", "fake").Should().BeFalse();
//        readTags.Has("fake").Should().BeFalse();

//        IReadOnlyDictionary<string, string?>? r2 = json.ToObject<IReadOnlyDictionary<string, string?>>();
//        r2.Should().NotBeNull();
//        r2!.ContainsKey("key2").Should().BeTrue();
//        r2.ContainsKey("key1").Should().BeTrue();
//        r2["key1"].Should().Be("value1");
//    }
//}
