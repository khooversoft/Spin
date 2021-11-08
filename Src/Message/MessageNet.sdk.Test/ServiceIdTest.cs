//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using FluentAssertions;
//using MessageNet.sdk.Protocol;
//using Toolbox.Tools;
//using Xunit;

//namespace MessageNet.sdk.Test
//{
//    public class ServiceIdTest
//    {
//        public class EndpointIdTests
//        {
//            [Theory]
//            [InlineData("a/b")]
//            [InlineData("a1/b")]
//            [InlineData("a1/b2")]
//            public void TestPositive(string id)
//            {
//                var endpointId = new ServiceId(id);
//                endpointId.ToString().Should().Be(id);

//                var e1 = (ServiceId)id;
//                ((string)e1).Should().Be(id);

//                var e2 = new ServiceId(id);
//            }

//            [Theory]
//            [InlineData(null)]
//            [InlineData("")]
//            [InlineData("a")]
//            [InlineData("1a/b")]
//            [InlineData("a/1b")]
//            [InlineData("a1/b2/3c3")]
//            [InlineData("a1/4b2/3c3")]
//            public void TestNegative(string id)
//            {
//                Action a1 = () => new ServiceId(id);
//                a1.Should().Throw<ArgumentException>();

//                Action a2 = () => { var b2 = (ServiceId)id; };
//                a2.Should().Throw<ArgumentException>();

//                Action a3 = () => new ServiceId(id);
//                a3.Should().Throw<ArgumentException>();
//            }

//            [Fact]
//            public void TestIdInit()
//            {
//                var id = new ServiceId("a/b");

//                id.Id.Should().Be("a/b");
//                id.Namespace.Should().Be("a");
//                id.Service.Should().Be("b");
//            }

//            [Fact]
//            public void TestIdInit2()
//            {
//                var id = new ServiceId("a/b");

//                id.Id.Should().Be("a/b");
//                id.Namespace.Should().Be("a");
//                id.Service.Should().Be("b");
//            }

//            [Fact]
//            public void TestEndpointInit()
//            {
//                var id = new ServiceId("a/b/c");

//                id.Id.Should().Be("a/b/c");
//                id.Namespace.Should().Be("a");
//                id.Service.Should().Be("b");
//            }

//            [Fact]
//            public void TestSerialization()
//            {
//                var id = new ServiceId("a/b");

//                string json = Json.Default.Serialize(id);
//                ServiceId resultId = Json.Default.Deserialize<ServiceId>(json)!;

//                (resultId == id).Should().BeTrue();
//            }

//            [Fact]
//            public void TestInClassSerialization()
//            {
//                var r = new RecordA
//                {
//                    Name = "hello",
//                    ServiceId = new ServiceId("a/b"),
//                };

//                string json = Json.Default.Serialize(r);

//                RecordA resultId = Json.Default.Deserialize<RecordA>(json)!;

//                (resultId == r).Should().BeTrue();
//                (resultId.Name == r.Name).Should().BeTrue();
//                (resultId.ServiceId == r.ServiceId).Should().BeTrue();
//            }

//            private record RecordA
//            {
//                public string Name { get; init; } = null!;

//                public ServiceId ServiceId { get; init; } = null!;
//            }
//        }
//    }
//}
