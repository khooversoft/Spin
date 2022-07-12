using Azure;
using Directory.sdk.Client;
using Directory.sdk.Service;
using Directory.Test.Application;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Xunit;

namespace Directory.Test
{
    public class EntryControllerTests
    {
        [Fact]
        public async Task GivenDirectoryEntry_WhenRoundTrip_Success()
        {
            DirectoryClient client = TestApplication.GetDirectoryClient();

            var documentId = new DocumentId("test/unit-tests/entry1");

            var query = new QueryParameter()
            {
                Filter = "test/unit-tests",
                Recursive = false,
            };

            IReadOnlyList<DatalakePathItem> search = (await client.Search(query).ReadNext()).Records;
            if (search.Any(x => x.Name == (string)documentId)) await client.Delete(documentId);

            DirectoryEntry entry = new DirectoryEntry
            {
                DirectoryId = documentId.Path,
                ClassType = "test",
                Properties = new[] { "property1=value1" }
            };

            //var entry = new DirectoryEntryBuilder()
            //    .SetDirectoryId(documentId)
            //    .SetClassType("test")
            //    .AddProperty(new EntryProperty { Name = "property1", Value = "value1" })
            //    .Build();

            await client.Set(entry);

            search = (await client.Search(query).ReadNext()).Records;
            search.Any(x => x.Name == (string)documentId).Should().BeTrue();

            DirectoryEntry? readEntry = await client.Get(documentId);
            readEntry.Should().NotBeNull();

            readEntry!.DirectoryId.Should().Be(entry.DirectoryId);
            readEntry.ClassType.Should().Be(entry.ClassType);
            readEntry.ETag.Should().NotBeNull();
            readEntry.Properties.Count.Should().Be(1);

            readEntry.Properties
                .Zip(entry.Properties)
                .All(x => x.First == x.Second)
                .Should().BeTrue();

            //readEntry.Properties.Values.First().Action(x =>
            //{
            //    (x == entry.Properties.Values.First()).Should().BeTrue();
            //});

            await client.Delete(documentId);
            search = (await client.Search(query).ReadNext()).Records;
            search.Any(x => x.Name == (string)documentId).Should().BeFalse();
        }

        [Fact]
        public async Task GivenDirectoryEntry_WhenRoundTripWithETag_Success()
        {
            DirectoryClient client = TestApplication.GetDirectoryClient();

            var documentId = new DocumentId("test/unit-tests/entry1");

            var query = new QueryParameter()
            {
                Filter = "test",
                Recursive = false,
            };

            await client.Delete(documentId);

            DirectoryEntry entry = new DirectoryEntry
            {
                DirectoryId = documentId.Path,
                ClassType = "test",
                Properties = new[] { "property1=value1" }
            };

            await client.Set(entry);

            DirectoryEntry? readEntry = await client.Get(documentId);
            readEntry.Should().NotBeNull();
            readEntry!.ETag.Should().NotBeNull();
            readEntry.Properties.Count.Should().Be(1);

            DirectoryEntry updateEntry = new DirectoryEntry
            {
                DirectoryId = documentId.Path,
                ClassType = "test",
                Properties = new[] { "property1=value1", "property2=value2" },
                ETag = readEntry.ETag,
            };

            await client.Set(updateEntry);

            readEntry = await client.Get(documentId);
            readEntry.Should().NotBeNull();
            readEntry!.Properties.Count.Should().Be(2);

            await client.Delete(documentId);
            IReadOnlyList<DatalakePathItem> search = (await client.Search(query).ReadNext()).Records;
            search.Any(x => x.Name == (string)documentId).Should().BeFalse();
        }


        [Fact]
        public async Task GivenDirectoryEntry_WhenRoundTripWithETag_Fail()
        {
            DirectoryClient client = TestApplication.GetDirectoryClient();

            var documentId = new DocumentId("test/unit-tests/entry2");

            var query = new QueryParameter()
            {
                Filter = "test",
                Recursive = false,
            };

            await client.Delete(documentId);

            DirectoryEntry entry = new DirectoryEntry
            {
                DirectoryId = documentId.Path,
                ClassType = "test",
                Properties = new[] { "property1=value1" }
            };

            await client.Set(entry);

            DirectoryEntry? readEntry = await client.Get(documentId);
            readEntry.Should().NotBeNull();
            readEntry!.ETag.Should().NotBeNull();
            readEntry.Properties.Count.Should().Be(1);

            DirectoryEntry updateEntry = new DirectoryEntry
            {
                DirectoryId = documentId.Path,
                ClassType = "test-next",
                ETag = new ETag("0xFF9CA90CB9F5120"),
                Properties = new[] { "property2=value2" }
            };

            bool failed;
            try
            {
                await client.Set(updateEntry);
                failed = false;
            }
            catch (Azure.RequestFailedException)
            {
                failed = true;
            }

            failed.Should().BeTrue();

            await client.Delete(documentId);
            IReadOnlyList<DatalakePathItem> search = (await client.Search(query).ReadNext()).Records;
            search.Any(x => x.Name == (string)documentId).Should().BeFalse();
        }
    }
}
