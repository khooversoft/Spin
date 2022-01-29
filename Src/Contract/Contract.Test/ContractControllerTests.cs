using Contract.sdk.Client;
using Contract.sdk.Models;
using Contract.Test.Application;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Block;
using Toolbox.Document;
using Toolbox.Model;
using Xunit;

namespace Artifact.Test
{
    public class ContractControllerTests
    {
        [Fact]
        public async Task GivenNoContract_WhenCreated_ShouldVerify()
        {
            ContractClient client = TestApplication.GetContractClient();

            var documentId = new DocumentId("test/unit-tests-smart/contract1");

            var query = new QueryParameter()
            {
                Filter = "test/unit-tests-smart",
                Recursive = false,
            };

            IReadOnlyList<string> search = (await client.Search(query).ReadNext()).Records;
            if (search.Any(x => x == (string)documentId)) await client.Delete(documentId);

            var blkHeader = new BlkHeader
            {
                PrincipalId = "dev/user/endUser1@default.com",
                DocumentId = (string)documentId,
                Creator = "test",
                Description = "test description",
            };

            await client.Create(blkHeader);

            BlockChainModel model = await client.Get(documentId);
            model.Should().NotBeNull();
            model.Blocks.Should().NotBeNull();
            model.Blocks.Count.Should().Be(2);

            model.Blocks[1].Should().NotBeNull();
            model.Blocks[1].IsValid().Should().BeTrue();
            model.Blocks[1].BlockData.Should().NotBeNull();
            model.Blocks[1].BlockData.BlockType.Should().Be(typeof(BlkHeader).Name);

            BlockChain blockChain = model.ConvertTo();




            BatchSet<string> searchList = await client.Search(query).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.EndsWith(documentId.Path)).Should().BeTrue();

            (await client.Delete(documentId)).Should().BeTrue();

            searchList = await client.Search(query).ReadNext();
            searchList.Should().NotBeNull();
            searchList.Records.Any(x => x.EndsWith(documentId.Path)).Should().BeFalse();
        }

        private record Payload(string Name, string Value);
    }
}