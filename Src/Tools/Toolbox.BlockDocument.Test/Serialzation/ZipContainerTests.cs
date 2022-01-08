using FluentAssertions;
using System;
using System.IO;
using System.IO.Compression;
using Toolbox.Extensions;
using Toolbox.Security;
using Toolbox.Security.Keys;
using Toolbox.Security.Services;
using Toolbox.Tools;
using Toolbox.Tools.Zip;
using Xunit;

namespace Toolbox.BlockDocument.Test
{
    public class ZipContainerTests
    {
        [Fact]
        public void GivenBlockChain_WhenContainerIsMemory_ShouldRoundTrip()
        {
            const string issuer = "user@domain.com";
            const string issuer2 = "user2@domain.com";
            const string zipPath = "$block";
            var date = DateTime.UtcNow;

            IKeyService keyService = new KeyServiceBuilder()
                .Add(issuer, new RsaPublicKey())
                .Add(issuer2, new RsaPublicKey())
                .Build();

            IPrincipleSignatureCollection signatureCollection = new PrincipleSignatureCollection()
                .Add(new PrincipleSignature(issuer, "userBusiness@domain.com", keyService))
                .Add(new PrincipleSignature(issuer2, "userBusiness2@domain.com", keyService));

            BlockChain blockChain = new BlockChainBuilder()
                .SetPrincipleSignature(signatureCollection[issuer])
                .Build();

            var payload = new Payload { Name = "Name1", Value = 2, Price = 10.5f };
            var payload2 = new Payload2 { Last = "Last", Current = date, Author = "test" };

            blockChain.Add(payload, signatureCollection[issuer]);
            blockChain.Add(payload2, signatureCollection[issuer2]);

            blockChain.Validate(signatureCollection);

            string blockChainHash = blockChain.ToMerkleTree().BuildTree().ToString();

            string json = blockChain.ToJson();

            using var writeBuffer = new MemoryStream();
            using (var writer = new ZipArchive(writeBuffer, ZipArchiveMode.Create, leaveOpen: true))
            {
                writer.Write(zipPath, json);
            }

            writeBuffer.Length.Should().BeGreaterThan(0);
            writeBuffer.Seek(0, SeekOrigin.Begin);
            string readJson;

            using (var reader = new ZipArchive(writeBuffer, ZipArchiveMode.Read, leaveOpen: true))
            {
                readJson = reader.ReadAsString(zipPath);
            }

            BlockChain result = readJson.ToObject<BlockChainModel>()
                .VerifyNotNull("Cannot deserialize")
                .ConvertTo();

            result.Validate(signatureCollection);
            string resultChainHash = result.ToMerkleTree().BuildTree().ToString();

            blockChainHash.Should().Be(resultChainHash);
        }

        private record Payload
        {
            public string? Name { get; set; }
            public int Value { get; set; }
            public float Price { get; set; }
        }

        private record Payload2
        {
            public string? Last { get; set; }
            public DateTime Current { get; set; }
            public string? Author { get; set; }
        }
    }
}