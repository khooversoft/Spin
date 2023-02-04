using Artifact.sdk;
using Artifact.Test.Application;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.DataLake.Model;
using Toolbox.DocumentStore;
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

            DocumentId documentId = new DocumentId(id);

            var payload = new Payload("Test1_name", "value1");

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

            var payloadText = readPayload!.ToObject<Payload>();
            payloadText.Should().Be(payload);

            var search = new QueryParameter { Container = "contract", Recursive = true };

            BatchQuerySet<DatalakePathItem> searchList = await client.Search(search).ReadNext();
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
                .SetObjectClass("test")
                .SetData(payload)
                .Build()
                .Verify();

            await client.Set(document);

            Document readDocument = (await client.Get(documentId)).NotNull();
            readDocument.Should().NotBeNull();
            readDocument.Verify();

            (document.DocumentId == readDocument.DocumentId).Should().BeTrue();
            (document.ObjectClass == readDocument.ObjectClass).Should().BeTrue();
            (document.TypeName == readDocument.TypeName).Should().BeTrue();
            (document.Data == readDocument.Data).Should().BeTrue();
            Enumerable.SequenceEqual(document.Hash, readDocument.Hash).Should().BeTrue();
            (document.PrincipleId == readDocument.PrincipleId).Should().BeTrue();

            (document == readDocument).Should().BeTrue();

            Payload? readPayload = readDocument!.ToObject<Payload>();
            readPayload.Should().NotBeNull();
            (payload == readPayload).Should().BeTrue();

            var search = new QueryParameter { Container = "contract", Filter = "test/testing" };

            BatchQuerySet<DatalakePathItem> searchList = await client.Search(search).ReadNext();
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