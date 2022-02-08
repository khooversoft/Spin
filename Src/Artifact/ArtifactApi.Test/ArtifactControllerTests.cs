using Artifact.sdk;
using Artifact.Test.Application;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Xunit;

namespace Artifact.Test
{
    public class ArtifactControllerTests
    {
        [Theory]
        [InlineData("contract:test/testing/customer/file1.txt")]
        [InlineData("contract:test/testing/smart-contract/customer/hash0xff3e4/file1.txt")]
        [InlineData("contract:test/testing/directory/file5.txt")]
        public async Task GivenData_WhenRoundTrip_ShouldMatch(string id)
        {
            ArtifactClient client = TestApplication.GetArtifactClient();

            const string payload = "This is a test";
            DocumentId documentId = new DocumentId(id);

            Document document = new DocumentBuilder()
                .SetDocumentId(documentId)
                .SetData(payload)
                .Build()
                .Verify();

            await client.Set(document);

            Document? readPayload = await client.Get(documentId);
            readPayload.Should().NotBeNull();
            readPayload!.Verify();

            (document == readPayload).Should().BeTrue();

            string? payloadText = readPayload!.DeserializeData<string>();
            payloadText.Should().Be(payload);

            var search = new QueryParameter { Container = "contract", Recursive = true };

            BatchSet<DatalakePathItem> searchList = await client.Search(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.Name.EndsWith(documentId.Path)).Should().BeTrue();

            (await client.Delete(documentId)).Should().BeTrue();

            searchList = await client.Search(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.Name.EndsWith(documentId.Path)).Should().BeFalse();
        }

        [Fact]
        public async Task GivenRecord_WhenRoundTrip_ShouldMatch()
        {
            ArtifactClient client = TestApplication.GetArtifactClient();

            var payload = new Payload("name1", "value1");
            DocumentId documentId = new DocumentId("contract:test/testing/payload.json");

            Document document = new DocumentBuilder()
                .SetDocumentId(documentId)
                .SetData(payload)
                .Build()
                .Verify();

            await client.Set(document);

            Document? readDocument = await client.Get(documentId);
            readDocument.Should().NotBeNull();
            readDocument!.Verify();

            (document == readDocument).Should().BeTrue();

            Payload? readPayload = readDocument!.DeserializeData<Payload>();
            readPayload.Should().NotBeNull();
            (payload == readPayload).Should().BeTrue();

            var search = new QueryParameter { Container = "contract", Filter = "test/testing" };

            BatchSet<DatalakePathItem> searchList = await client.Search(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.Name.EndsWith(documentId.Path)).Should().BeTrue();

            (await client.Delete(documentId)).Should().BeTrue();

            searchList = await client.Search(search).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.Name.EndsWith(documentId.Path)).Should().BeFalse();
        }

        private record Payload(string Name, string Value);
    }
}