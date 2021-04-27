using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Toolbox.Tools;
using Xunit;

namespace Toolbox.Test.Tools
{
    public class PathToolsTests
    {
        [Theory]
        [InlineData("single", "single.json")]
        [InlineData("single.json", "single.json")]
        [InlineData("single/second", "single/second.json")]
        [InlineData("single/second.json", "single/second.json")]
        [InlineData("single/second/third", "single/second/third.json")]
        [InlineData("single/second/third.json", "single/second/third.json")]
        [InlineData("single.txt", "single.txt")]
        public void PathSetExtension_ShouldPass(string source, string matchTo)
        {
            string result = PathTools.SetExtension(source, ".json");
            result.Should().Be(matchTo);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void InvalidPathSetExtension_ShouldFail(string source)
        {
            Action act = () => PathTools.SetExtension(source, ".json");
            act.Should().Throw<ArgumentException>();
        }
    }
}
