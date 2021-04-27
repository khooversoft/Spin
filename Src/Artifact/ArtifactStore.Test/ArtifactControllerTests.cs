using ArtifactStore.sdk.Model;
using ArtifactStore.Test.Application;
using FluentAssertions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Model;
using Xunit;

namespace ArtifactStore.Test
{
    public class ArtifactControllerTests
    {
        [Theory]
        [InlineData("customer/file1.txt")]
        [InlineData("smart-contract/customer/hash0xff3e4/file1.txt")]
        [InlineData("directory/file5.txt")]
        public async Task GivenData_WhenRoundTrip_ShouldMatch(string id)
        {
            ArtifactTestHost host = TestApplication.GetHost();

            const string payload = "This is a test";
            ArtifactId artifactId = new ArtifactId(id);

            byte[] bytes = Encoding.UTF8.GetBytes(payload);

            ArtifactPayload articlePayload = bytes.ToArtifactPayload(artifactId);

            await host.ArtifactClient.Set(articlePayload);

            ArtifactPayload? readPayload = await host.ArtifactClient.Get(artifactId);
            readPayload.Should().NotBeNull();

            (articlePayload == readPayload).Should().BeTrue();

            string payloadText = Encoding.UTF8.GetString(readPayload!.ToBytes());
            payloadText.Should().Be(payload);

            var search = new QueryParameter { Namespace = artifactId.Namespace };

            BatchSet<string> searchList = await host.ArtifactClient.List(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.StartsWith(artifactId.Path)).Should().BeTrue();

            (await host.ArtifactClient.Delete(artifactId)).Should().BeTrue();

            searchList = await host.ArtifactClient.List(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.StartsWith(artifactId.Path)).Should().BeFalse();
        }
    }
}