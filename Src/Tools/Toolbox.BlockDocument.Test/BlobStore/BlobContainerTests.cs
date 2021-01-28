//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Toolbox.Test;
//using Xunit;

//namespace Toolbox.BlockDocument.Test.BlobStoreTest
//{
//    public class BlobContainerTests
//    {
//        private ILoggerFactory _loggerFactory = new TestLoggerFactory();
//        private readonly AzureTestOption _testOption;
//        private readonly BlobRepositoryOption _blobOption;

//        public BlobContainerTests()
//        {
//            _testOption = new TestOptionBuilder().Build();
//            _blobOption = _testOption.BlobOption.WithContainer("block-chain-test");
//        }

//        [Trait("Category", "LocalOnly")]
//        [Fact]
//        public async Task GivenBlockChain_WhenContainerIsBlob_ShouldRoundTrip()
//        {
//            const string _zipPath = "$block";
//            const string _blobPath = "Test.sa";

//            var container = new BlobRepository(_blobOption, _loggerFactory.CreateLogger<BlobRepository>());

//            var blockChain = new BlockChain()
//            {
//                new DataBlock<HeaderBlock>("header", "header_1", new HeaderBlock("Master Contract")),
//                new DataBlock<BlobBlock>("contract", "contract_1", new BlobBlock("contract.docx", "docx", "me", Encoding.UTF8.GetBytes("this is a contract between two people"))),
//                new DataBlock<TrxBlock>("ContractLedger", "Pmt", new TrxBlock("1", "cr", 100)),
//            };

//            blockChain.Blocks.Count.Should().Be(4);
//            blockChain.IsValid().Should().BeTrue();
//            string blockChainHash = blockChain.ToMerkleTree().BuildTree().ToString();

//            string json = blockChain.ToJson();

//            //var buffer = new byte[1000];
//            using var writeMemoryBuffer = new MemoryStream();
//            var writer = new ZipContainerWriter(new ZipArchive(writeMemoryBuffer, ZipArchiveMode.Create, leaveOpen: true));
//            writer.Write(_zipPath, json);
//            writer.Close();

//            writeMemoryBuffer.Length.Should().BeGreaterThan(0);
//            writeMemoryBuffer.Seek(0, SeekOrigin.Begin);

//            await container.Delete(_blobPath, CancellationToken.None);
//            await container.Upload(_blobPath, writeMemoryBuffer, CancellationToken.None);
//            writeMemoryBuffer.Close();

//            IReadOnlyList<byte> readBlob = await container.Download(_blobPath);
//            using var readMemoryBuffer = new MemoryStream(readBlob.ToArray());

//            var reader = new ZipContainerReader(new ZipArchive(readMemoryBuffer, ZipArchiveMode.Read));
//            string readJson = reader.Read(_zipPath);
//            reader.Close();

//            BlockChain result = readJson.ToBlockChain();
//            result.IsValid().Should().BeTrue();
//            string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

//            blockChainHash.Should().Be(resultChainHash);

//            await container.Delete(_blobPath, CancellationToken.None);
//        }

//        [Trait("Category", "LocalOnly")]
//        [Fact]
//        public async Task GivenBlockChain_WhenContainerIsBlobAndBuilder_ShouldRoundTrip()
//        {
//            const string _blobPath = "Test.sa";

//            var container = new BlobRepository(_blobOption, _loggerFactory.CreateLogger<BlobRepository>());

//            await container.CreateContainer(CancellationToken.None);

//            var blockChain = new BlockChain()
//            {
//                new DataBlock<HeaderBlock>("header", "header_1", new HeaderBlock("Master Contract")),
//                new DataBlock<BlobBlock>("contract", "contract_1", new BlobBlock("contract.docx", "docx", "me", Encoding.UTF8.GetBytes("this is a contract between two people"))),
//                new DataBlock<TrxBlock>("ContractLedger", "Pmt", new TrxBlock("1", "cr", 100)),
//            };

//            blockChain.Blocks.Count.Should().Be(4);
//            string blockChainHash = blockChain.ToMerkleTree().BuildTree().ToString();

//            using (var zipStream = blockChain.ToZipContainer())
//            {
//                await container.Delete(_blobPath, CancellationToken.None);
//                await container.Upload(_blobPath, zipStream, CancellationToken.None);
//            }

//            IReadOnlyList<byte> readBlob = await container.Download(_blobPath);
//            using var readMemoryBuffer = new MemoryStream(readBlob.ToArray());

//            BlockChain result = readMemoryBuffer.ToBlockChain();
//            result.IsValid().Should().BeTrue();
//            string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

//            blockChainHash.Should().Be(resultChainHash);

//            await container.Delete(_blobPath, CancellationToken.None);
//        }

//        [Trait("Category", "LocalOnly")]
//        [Fact]
//        public async Task GivenBlockChain_WhenUsingBuilder_ShouldValidate()
//        {
//            const string _blobPath = "Test.sa";

//            var container = new BlobRepository(_blobOption, _loggerFactory.CreateLogger<BlobRepository>());

//            await container.CreateContainer(CancellationToken.None);

//            var blockChain = new BlockChain()
//            {
//                new DataBlock<HeaderBlock>("header", "header_1", new HeaderBlock("Master Contract")),
//                new DataBlock<BlobBlock>("contract", "contract_1", new BlobBlock("contract.docx", "docx", "me", Encoding.UTF8.GetBytes("this is a contract between two people"))),
//                new DataBlock<TrxBlock>("ContractLedger", "Pmt", new TrxBlock("1", "cr", 100)),
//            };

//            blockChain.Blocks.Count.Should().Be(4);

//            string blockChainHash = blockChain.ToMerkleTree().BuildTree().ToString();

//            using var zipStream = blockChain.ToZipContainer();

//            BlockChain result = zipStream.ToBlockChain();
//            result.IsValid().Should().BeTrue();
//            string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

//            blockChainHash.Should().Be(resultChainHash);

//            await container.Delete(_blobPath, CancellationToken.None);
//        }
//    }
//}