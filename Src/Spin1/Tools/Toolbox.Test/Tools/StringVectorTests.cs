using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class StringVectorTests
    {
        [Fact]
        public void PathInitialization()
        {
            new StringVector().Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be("/");
                x.Path.Should().NotBeNull();
                x.Values.Count.Should().Be(0);
            });

            (new StringVector() + (string)null!).Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be("/");
                x.Path.Should().Be(string.Empty);
                x.Values.Count.Should().Be(0);
            });

            (new StringVector() + "").Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be("/");
                x.Path.Should().Be(string.Empty);
                x.Values.Count.Should().Be(0);
            });

            (new StringVector() + "path1").Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be("/");
                x.Path.Should().Be("path1");
                x.Values.Count.Should().Be(1);
                x.Values[0].Should().Be("path1");
            });

            (new StringVector() + "path1/path2").Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be("/");
                x.Path.Should().Be("path1/path2");
                x.Values.Count.Should().Be(2);
                x.Values[0].Should().Be("path1");
                x.Values[1].Should().Be("path2");
            });
        }

        [Fact]
        public void PathInitializationWithDelimiter()
        {
            (new StringVector(":") + "").Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be(":");
                x.Path.Should().Be(string.Empty);
                x.Values.Count.Should().Be(0);
            });

            (new StringVector(":") + "path1").Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be(":");
                x.Path.Should().Be("path1");
                x.Values.Count.Should().Be(1);
                x.Values[0].Should().Be("path1");
            });

            (new StringVector(":") + "path1:path2").Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be(":");
                x.Path.Should().Be("path1:path2");
                x.Values.Count.Should().Be(2);
                x.Values[0].Should().Be("path1");
                x.Values[1].Should().Be("path2");
            });

            (new StringVector(":d:") + "path1:d:path2").Action(x =>
            {
                x.Should().NotBeNull();
                x.Delimiter.Should().Be(":d:");
                x.Path.Should().Be("path1:d:path2");
                x.Values.Count.Should().Be(2);
                x.Values[0].Should().Be("path1");
                x.Values[1].Should().Be("path2");
            });
        }

        [Theory]
        [InlineData(null, null, "")]
        [InlineData("", "", "")]
        [InlineData("path1", "path2", "path1/path2")]
        [InlineData("path1/path2", "path3", "path1/path2/path3")]
        [InlineData("path1/path2", "path3/path4", "path1/path2/path3/path4")]
        public void PathAddition(string root, string add, string expected)
        {
            var r = new StringVector() + root;
            r.Path.Should().Be(root ?? string.Empty);

            r += add;
            r.Path.Should().Be(expected);

            var r2 = new StringVector() + root! + add;

            (r == r2).Should().BeTrue();
        }

        [Fact]
        public void PathAdditionOfTwoVectors()
        {
            var r1 = new StringVector() + "path1";
            var r2 = new StringVector() + "path2";
            var r3 = r1 + r2;

            r3.Path.Should().Be("path1/path2");
        }

        [Fact]
        public void PathAdditionWithArray()
        {
            var r1 = new StringVector() + "path1";
            var r2 = new string[] { "path2", "path3" };
            var r3 = r1 + r2;

            r3.Path.Should().Be("path1/path2/path3");
        }
    }
}
