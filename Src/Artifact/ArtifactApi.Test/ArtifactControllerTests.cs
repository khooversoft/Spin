using Artifact.Application;
using Artifact.sdk.Model;
using Artifact.Test.Application;
using Directory.sdk;
using FluentAssertions;
using MessageNet.sdk.Protocol;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Model;
using Xunit;

namespace Artifact.Test
{
    public class ArtifactControllerTests
    {
        [Theory]
        [InlineData("customer/file1.txt")]
        [InlineData("smart-contract/customer/hash0xff3e4/file1.txt")]
        [InlineData("directory/file5.txt")]
        public async Task GivenData_WhenRoundTrip_ShouldMatch(string id)
        {
            ArtificatTestHost host = TestApplication.GetHost();

            //MessageUrl messageUrl = (MessageUrl)"message://artifact";

            const string payload = "This is a test";
            ArtifactId artifactId = new ArtifactId(id);

            ArtifactPayload articlePayload = new ArtifactPayloadBuilder()
                .SetId(artifactId)
                .SetPayload(payload)
                .Build();

            await host.ArtifactClient.Set(articlePayload);

            ArtifactPayload? readPayload = await host.ArtifactClient.Get(artifactId);
            readPayload.Should().NotBeNull();

            (articlePayload == readPayload).Should().BeTrue();

            string? payloadText = readPayload!.DeserializePayload<string>();
            payloadText.Should().Be(payload);

            var search = new QueryParameter();

            BatchSet<string> searchList = await host.ArtifactClient.List(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.StartsWith(artifactId.Path)).Should().BeTrue();

            (await host.ArtifactClient.Delete(artifactId)).Should().BeTrue();

            searchList = await host.ArtifactClient.List(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.StartsWith(artifactId.Path)).Should().BeFalse();
        }

        //[Theory]
        //[InlineData("customer/file1.txt")]
        //[InlineData("smart-contract/customer/hash0xff3e4/file1.txt")]
        //[InlineData("directory/file5.txt")]
        //public async Task GivenData_WhenRoundTrip_ShouldMatch(string id)
        //{
        //    ArtifactTestHost host = TestApplication.GetHost();

        //    MessageUrl messageUrl = (MessageUrl)"message://artifact";

        //    const string payload = "This is a test";
        //    ArtifactId artifactId = new ArtifactId(id);

        //    ArtifactPayload articlePayload = new ArtifactPayloadBuilder()
        //        .SetId(artifactId)
        //        .SetPayload(payload)
        //        .Build();

        //    await host.ArtifactClient.Set(articlePayload);

        //    ArtifactPayload? readPayload = await host.ArtifactClient.Get(artifactId);
        //    readPayload.Should().NotBeNull();

        //    (articlePayload == readPayload).Should().BeTrue();

        //    string? payloadText = readPayload!.DeserializePayload<string>();
        //    payloadText.Should().Be(payload);

        //    var search = new QueryParameter { Namespace = artifactId.Namespace };

        //    BatchSet<string> searchList = await host.ArtifactClient.List(search).ReadNext();
        //    searchList.Should().NotBeNull();
        //    searchList.Records.Any(x => x.StartsWith(artifactId.Path)).Should().BeTrue();

        //    (await host.ArtifactClient.Delete(artifactId)).Should().BeTrue();

        //    searchList = await host.ArtifactClient.List(search).ReadNext();
        //    searchList.Should().NotBeNull();
        //    searchList.Records.Any(x => x.StartsWith(artifactId.Path)).Should().BeFalse();
        //}


    }
}