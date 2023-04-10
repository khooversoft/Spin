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

namespace MessageNet.sdk.Test
{
    public class MessageUrlTests
    {
        [Theory]
        [InlineData("a/b")]
        [InlineData("a/b/c")]
        [InlineData("a1/b")]
        [InlineData("a1/b2")]
        [InlineData("a1/b2/c3")]
        public void TestPositive(string id)
        {
            const string protocol = "message";

            var endpointId = new MessageUrl(id);
            endpointId.ToString().Should().Be($"{protocol}://{id}");

            var e1 = (MessageUrl)id;
            ((string)e1).Should().Be($"{protocol}://{id}");

            _ = new MessageUrl(id);
        }

        [Theory]
        [InlineData("protocol://a1/b2/c3")]
        public void TestPositive2(string id)
        {
            var endpointId = new MessageUrl(id);
            endpointId.ToString().Should().Be($"{id}");

            var e1 = (MessageUrl)id;
            ((string)e1).Should().Be($"{id}");

            _ = new MessageUrl(id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a@")]
        [InlineData("1a/b;")]
        [InlineData("protocol://")]
        [InlineData("protocol://dkd://")]
        public void TestNegative(string id)
        {
            Action a1 = () => new MessageUrl(id);
            a1.Should().Throw<ArgumentException>();

            Action a2 = () => { var b2 = (MessageUrl)id; };
            a2.Should().Throw<ArgumentException>();

            Action a3 = () => new MessageUrl(id);
            a3.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TestServiceInit()
        {
            var id = new MessageUrl("a");

            id.Protocol.Should().NotBeNullOrEmpty();
            id.Service.Should().Be("a");
            id.Endpoint.Should().BeNull();

            ((string)id).Should().Be("message://a");
        }

        [Fact]
        public void TestServiceInit2()
        {
            var id = new MessageUrl("protocol://a");

            id.Protocol.Should().Be("protocol");
            id.Service.Should().Be("a");
            id.Endpoint.Should().BeNull();

            ((string)id).Should().Be("protocol://a");
        }

        [Fact]
        public void TestServiceEndpointInit()
        {
            var id = new MessageUrl("a/b");

            id.Protocol.Should().NotBeNullOrEmpty();
            id.Service.Should().Be("a");
            id.Endpoint.Should().Be("b");

            ((string)id).Should().Be("message://a/b");
        }

        [Fact]
        public void TestServiceEndpointInit2()
        {
            var id = new MessageUrl("protocol://a/b");

            id.Protocol.Should().Be("protocol");
            id.Service.Should().Be("a");
            id.Endpoint.Should().Be("b");

            ((string)id).Should().Be("protocol://a/b");
        }

        [Fact]
        public void TestServiceEndpointInit3()
        {
            var id = new MessageUrl("protocol://a/b/3");

            id.Protocol.Should().Be("protocol");
            id.Service.Should().Be("a");
            id.Endpoint.Should().Be("b/3");

            ((string)id).Should().Be("protocol://a/b/3");
        }

        [Fact]
        public void TestSerialization()
        {
            var id = new MessageUrl("a/b");

            string json = Json.Default.Serialize(id);

            MessageUrl resultId = Json.Default.Deserialize<MessageUrl>(json)!;

            (resultId == id).Should().BeTrue();
        }


        [Fact]
        public void TestSerialization2()
        {
            var id = new MessageUrl("a", "b");

            string json = Json.Default.Serialize(id);

            MessageUrl resultId = Json.Default.Deserialize<MessageUrl>(json)!;

            (resultId == id).Should().BeTrue();
        }

        [Fact]
        public void TestInClassSerialization()
        {
            var r = new RecordA
            {
                Name = "hello",
                Endpoint = new MessageUrl("a/b/c"),
            };

            string json = Json.Default.Serialize(r);

            RecordA resultId = Json.Default.Deserialize<RecordA>(json)!;

            (resultId == r).Should().BeTrue();
            (resultId.Name == r.Name).Should().BeTrue();
            (resultId.Endpoint == r.Endpoint).Should().BeTrue();
        }

        [Theory]
        [InlineData("message://path1", "path2", "message://path1/path2")]
        [InlineData("path1", "path2", "message://path1/path2")]
        [InlineData("path1/path2", "path3", "message://path1/path2/path3")]
        [InlineData("path1/path2", "path3/path4", "message://path1/path2/path3/path4")]
        public void TestAppendPath(string url, string add, string result)
        {
            MessageUrl messageUrl = new MessageUrl(url);
            messageUrl += add;
            ((string)messageUrl).Should().Be(result);
        }


        private record RecordA
        {
            public string Name { get; init; } = null!;

            public MessageUrl Endpoint { get; init; } = null!;
        }
    }
}
