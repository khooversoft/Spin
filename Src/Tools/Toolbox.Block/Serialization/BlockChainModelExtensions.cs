using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Zip;

namespace Toolbox.Block.Serialization;

public static class BlockChainModelExtensions
{
    private const string _zipPath = "$block";

    public static BlockChainModel Verify(this BlockChainModel blockChainModel)
    {
        blockChainModel.NotNull();
        blockChainModel.Blocks.NotNull();
        blockChainModel.Blocks.ForEach(x => x.Verify());

        return blockChainModel;
    }

    public static byte[] ToPackage(this BlockChainModel blockChain)
    {
        blockChain.NotNull();

        string json = blockChain.ToJson();

        using var writeBuffer = new MemoryStream();
        using (var writer = new ZipArchive(writeBuffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            writer.Write(_zipPath, json);
        }

        return writeBuffer.ToArray();
    }

    public static BlockChainModel ToBlockChainModel(this byte[] bytes)
    {
        bytes.NotNull();

        using var writeBuffer = new MemoryStream(bytes);
        using var reader = new ZipArchive(writeBuffer, ZipArchiveMode.Read);
        string readJson = reader.ReadAsString(_zipPath);

        var result = readJson.ToObject<BlockChainModel>()
            .NotNull(name: "Cannot deserialize");

        return result;
    }
}