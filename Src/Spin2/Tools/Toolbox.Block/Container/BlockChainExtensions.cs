﻿using System.IO.Compression;
using Toolbox.Block.Serialization;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Zip;
using Toolbox.Types.MerkleTree;

namespace Toolbox.Block.Container;

public static class BlockChainExtensions
{
    private const string _zipPath = "$block";

    public static BlockChain Add<T>(this BlockChain blockChain, T value, string principleId, string? blockType = null) where T : class
    {
        blockChain.NotNull();
        value.NotNull();
        principleId.NotEmpty();
        blockType.Assert(x => x == null | !x.IsEmpty(), $"{nameof(blockType)} cannot be empty, null is valid");

        blockChain += new DataBlockBuilder()
            .SetTimeStamp(DateTime.UtcNow)
            .SetBlockType(blockType ?? value.GetType().GetTypeName())
            .SetData(value)
            .SetPrincipleId(principleId)
            .Build();

        return blockChain;
    }

    public static IReadOnlyList<T> GetTypedBlocks<T>(this BlockChain blockChain) => blockChain.NotNull()
        .GetTypedBlocks(typeof(T).GetTypeName())
        .Select(x => x.ToObject<T>())
        .ToArray();

    public static IReadOnlyList<T> GetTypedBlocks<T>(this BlockChain blockChain, string blockType) => blockChain.NotNull()
        .GetTypedBlocks(blockType)
        .Select(x => x.ToObject<T>())
        .ToArray();

    public static IReadOnlyList<DataBlock> GetTypedBlocks(this BlockChain blockChain, string blockType) => blockChain.NotNull()
        .Blocks
        .Where(x => x.DataBlock.BlockType == blockType)
        .Select(x => x.DataBlock)
        .ToList();

    public static BlockChain ToBlockChain(this BlockChainModel blockChainModel)
    {
        blockChainModel.NotNull();

        return new BlockChain(blockChainModel.Blocks);
    }

    public static BlockChainModel ToBlockChainModel(this BlockChain blockChain) => new BlockChainModel
    {
        Blocks = blockChain.NotNull().Blocks.ToList()
    };

    public static MerkleTree ToMerkleTree(this BlockChain blockChain)
    {
        return new MerkleTree()
            .Append(blockChain.Blocks.Select(x => x.Digest).ToArray());
    }

    public static byte[] ToZip(this BlockChain blockChain)
    {
        string json = blockChain.ToBlockChainModel().ToJson();

        using var writeBuffer = new MemoryStream();
        using (var writer = new ZipArchive(writeBuffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            writer.Write(_zipPath, json);
        }

        return writeBuffer.ToArray();
    }

    public static BlockChain ToBlockChain(this byte[] data)
    {
        using var readBuffer = new MemoryStream(data);

        using var reader = new ZipArchive(readBuffer, ZipArchiveMode.Read);
        string readJson = reader.ReadAsString(_zipPath);

        BlockChain result = readJson.ToObject<BlockChainModel>()
            .NotNull(name: "Cannot deserialize")
            .ToBlockChain();

        return result;
    }
}