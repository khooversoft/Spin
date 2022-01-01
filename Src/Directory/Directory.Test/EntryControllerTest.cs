using Azure;
using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Service;
using Directory.Test.Application;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;
using Xunit;

namespace Directory.Test
{
    public class EntryControllerTest
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

            var entry = new DirectoryEntryBuilder()
                .SetDirectoryId(documentId)
                .SetClassType("test")
                .AddProperty(new EntryProperty { Name = "property1", Value = "value1" })
                .Build();

            await client.Set(entry);

            search = (await client.Search(query).ReadNext()).Records;
            search.Any(x => x.Name == (string)documentId).Should().BeTrue();

            DirectoryEntry? readEntry = await client.Get(documentId);
            readEntry.Should().NotBeNull();

            readEntry!.DirectoryId.Should().Be(entry.DirectoryId);
            readEntry.ClassType.Should().Be(entry.ClassType);
            readEntry.ETag.Should().NotBeNull();
            readEntry.Properties.Count.Should().Be(1);

            readEntry.Properties.Values.First().Action(x =>
            {
                (x == entry.Properties.Values.First()).Should().BeTrue();
            });

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

            var entry = new DirectoryEntryBuilder()
                .SetDirectoryId(documentId)
                .SetClassType("test")
                .AddProperty(new EntryProperty { Name = "property1", Value = "value1" })
                .Build();

            await client.Set(entry);

            DirectoryEntry? readEntry = await client.Get(documentId);
            readEntry.Should().NotBeNull();
            readEntry!.ETag.Should().NotBeNull();
            readEntry.Properties.Count.Should().Be(1);

            var updateEntry = new DirectoryEntryBuilder(readEntry)
                .SetClassType("test-next")
                .AddProperty(new EntryProperty { Name = "property2", Value = "value2" })
                .Build();

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

            var documentId = new DocumentId("test/unit-tests/entry1");

            var query = new QueryParameter()
            {
                Filter = "test",
                Recursive = false,
            };

            await client.Delete(documentId);

            var entry = new DirectoryEntryBuilder()
                .SetDirectoryId(documentId)
                .SetClassType("test")
                .AddProperty(new EntryProperty { Name = "property1", Value = "value1" })
                .Build();

            await client.Set(entry);

            DirectoryEntry? readEntry = await client.Get(documentId);
            readEntry.Should().NotBeNull();
            readEntry!.ETag.Should().NotBeNull();
            readEntry.Properties.Count.Should().Be(1);

            var updateEntry = new DirectoryEntryBuilder(readEntry)
                .SetClassType("test-next")
                .SetETag(new ETag("0xFF9CA90CB9F5120"))
                .AddProperty(new EntryProperty { Name = "property2", Value = "value2" })
                .Build();

            bool failed;
            try
            {
                await client.Set(updateEntry);
                failed = false;
            }
            catch(Azure.RequestFailedException)
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
