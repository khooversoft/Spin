using FluentAssertions;
using MessageNet.sdk.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
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

            var e2 = new EndpointId()
            {
                Id = id,
            };
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

            Action a3 = () => new EndpointId { Id = id };
            a3.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TestEmpty()
        {
            var id = new EndpointId();

            id.Id.Should().Be(string.Empty);
            id.Namespace.Should().Be(string.Empty);
            id.Node.Should().Be(string.Empty);
            id.Endpoint.Should().Be(string.Empty);
        }

        [Fact]
        public void TestIdInit()
        {
            var id = new EndpointId("a/b");

            id.Id.Should().Be("a/b");
            id.Namespace.Should().Be("a");
            id.Node.Should().Be("b");
            id.Endpoint.Should().Be(string.Empty);
        }

        [Fact]
        public void TestIdInit2()
        {
            var id = new EndpointId
            {
                Id = "a/b",
            };

            id.Id.Should().Be("a/b");
            id.Namespace.Should().Be("a");
            id.Node.Should().Be("b");
            id.Endpoint.Should().Be(string.Empty);
        }

        [Fact]
        public void TestEndpointInit()
        {
            var id = new EndpointId("a/b/c");

            id.Id.Should().Be("a/b/c");
            id.Namespace.Should().Be("a");
            id.Node.Should().Be("b");
            id.Endpoint.Should().Be("c");
        }

        [Fact]
        public void TestSerialization()
        {
            var id = new EndpointId("a/b");

            string json = Json.Default.Serialize(id);

            EndpointId resultId = Json.Default.Deserialize<EndpointId>(json)!;

            (resultId == id).Should().BeTrue();
        }

        [Fact]
        public void TestInClassSerialization()
        {
            var r = new RecordA
            {
                Name = "hello",
                Endpoint = new EndpointId("a/b/c"),
            };

            string json = Json.Default.Serialize(r);

            RecordA resultId = Json.Default.Deserialize<RecordA>(json)!;

            (resultId == r).Should().BeTrue();
            (resultId.Name == r.Name).Should().BeTrue();
            (resultId.Endpoint == r.Endpoint).Should().BeTrue();
        }

        private record RecordA
        {
            public string Name { get; init; } = null!;

            public EndpointId Endpoint { get; init; } = null!;
        }
    }
}
