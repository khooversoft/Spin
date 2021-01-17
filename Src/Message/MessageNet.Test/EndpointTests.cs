using FluentAssertions;
using MessageNet.sdk.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MessageNet.Test
{
    public class EndpointTests
    {
        [Theory]
        [InlineData("a/b")]
        [InlineData("a/b/c")]
        [InlineData("a1/b")]
        [InlineData("a1/b2")]
        [InlineData("a1/b2/c3")]
        public void TestPositive(string id)
        {
            var endpointId = new EndpointId(id);
            endpointId.ToString().Should().Be(id);

            var e1 = (EndpointId)id;
            ((string)e1).Should().Be(id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("1a/b")]
        [InlineData("a/1b")]
        [InlineData("a1/b2/3c3")]
        [InlineData("a1/4b2/3c3")]
        public void TestNegative(string id)
        {
            Action a1 = () => new EndpointId(id);
            a1.Should().Throw<ArgumentException>();

            Action a2 = () => { var b2 = (EndpointId)id; };
            a2.Should().Throw<ArgumentException>();
        }
    }
}
