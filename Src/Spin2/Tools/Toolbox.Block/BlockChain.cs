using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.MerkleTree;

namespace Toolbox.Block;

/// <summary>
/// Block chain container
/// </summary>
public sealed class BlockChain
{
    private readonly List<BlockNode> _blocks;
    private readonly object _lock = new object();

    public BlockChain() => _blocks = new List<BlockNode>();
    public BlockChain(IEnumerable<BlockNode> blockNodes) => _blocks = blockNodes.NotNull().ToList();

    public IReadOnlyList<BlockNode> Blocks => _blocks;

    /// <summary>
    /// Add data blocks
    /// </summary>
    /// <param name="dataBlocks"></param>
    public Option Add(params DataBlock[] dataBlocks)
    {
        var authorized = CheckWriteAccess(dataBlocks);
        if (authorized.StatusCode.IsError()) return authorized;

        lock (_lock)
        {
            foreach (var item in dataBlocks)
            {
                item.Verify();

                if (_blocks.Count == 0)
                {
                    _blocks.Add(new BlockNode(item));
                    continue;
                }

                BlockNode latestBlock = Blocks[_blocks.Count - 1];

                var newBlock = new BlockNode(item, latestBlock.Index + 1, latestBlock.Digest);
                _blocks.Add(newBlock);
            }
        }

        return new Option(StatusCode.OK);
    }

    public bool IsValid()
    {
        lock (_lock)
        {
            if (Blocks.Any(x => !x.IsValid())) return false;

            for (int i = 1; i < Blocks.Count; i++)
            {
                BlockNode currentBlock = Blocks[i];
                BlockNode previousBlock = Blocks[i - 1];

                if (currentBlock.Digest != currentBlock.Digest) return false;
                if (currentBlock.PreviousHash != previousBlock.Digest) return false;
            }

            return true;
        }
    }

    public string GetDigest()
    {
        lock (_lock)
        {
            return new MerkleTree()
                .Append(_blocks.Select(x => new MerkleHash(x.GetDigest())).ToArray())
                .BuildTree()
                .ToString();
        }
    }

    public IReadOnlyList<T> GetTypedBlocks<T>(string blockType) => GetTypedBlocks(blockType)
        .Select(x => x.ToObject<T>())
        .ToArray();

    public IReadOnlyList<DataBlock> GetTypedBlocks(string blockType) => _blocks
        .Where(x => x.DataBlock.BlockType == blockType)
        .Select(x => x.DataBlock)
        .ToList();

    public Option<GenesisBlock> GetGenesisBlock() => GetTypedBlocks<GenesisBlock>(GenesisBlock.BlockType)
        .FirstOrDefaultOption();

    public Option<BlockAcl> GetAclBlock() => GetTypedBlocks<BlockAcl>(BlockAcl.BlockType)
        .LastOrDefaultOption();

    public Option CheckWriteAccess(IEnumerable<DataBlock> blocks)
    {
        blocks.NotNull();

        if (_blocks.Count == 0) return new Option(StatusCode.OK);

        var dict = blocks.ToDictionary(x => x.PrincipleId, x => x);

        GenesisBlock genesisBlock = GetGenesisBlock().Return();
        dict.Remove(genesisBlock.OwnerPrincipalId);

        if (dict.Count == 0) return new Option(StatusCode.OK);

        Option<BlockAcl> aclOption = GetAclBlock();
        if (aclOption == Option<BlockAcl>.None) return new Option(StatusCode.Unauthorized);

        BlockAcl acl = aclOption.Return();

        foreach (var item in dict.ToArray())
        {
            if (acl.HasWriteAccess(new NameId(item.Value.BlockType), new PrincipalId(item.Value.PrincipleId)))
            {
                dict.Remove(item.Key);
            }
        }

        var errors = dict
            .Select(x => $"PrincipalId={x.Value.PrincipleId} is not authorized")
            .ToArray();

        return errors.Length switch
        {
            0 => new Option(StatusCode.OK),
            _ => new Option(StatusCode.Unauthorized, errors.Join(",")),
        };
    }

    public static BlockChain operator +(BlockChain self, DataBlock blockData)
    {
        self.Add(blockData);
        return self;
    }
}