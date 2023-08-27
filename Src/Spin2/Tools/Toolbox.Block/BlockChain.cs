using System.Text.Json.Serialization;
using Toolbox.Extensions;
using Toolbox.Security.Sign;
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

    [JsonConstructor]
    public BlockChain(IEnumerable<BlockNode> blocks) => _blocks = blocks.NotNull().ToList();

    public int Count => _blocks.Count;

    public IEnumerable<BlockNode> Blocks => _blocks;

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

                BlockNode latestBlock = _blocks[_blocks.Count - 1];

                var newBlock = new BlockNode(item, latestBlock.Index + 1, latestBlock.Digest);
                _blocks.Add(newBlock);
            }
        }

        return StatusCode.OK;
    }

    public bool IsValid()
    {
        lock (_lock)
        {
            if (_blocks.Any(x => !x.IsValid())) return false;

            for (int i = 1; i < _blocks.Count; i++)
            {
                BlockNode currentBlock = _blocks[i];
                BlockNode previousBlock = _blocks[i - 1];

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

    public GenesisBlock GetGenesisBlock() => _blocks
        .Where(x => x.DataBlock.BlockType == GenesisBlock.BlockType)
        .Select(x => x.DataBlock.ToObject<GenesisBlock>())
        .Last();

    public IReadOnlyList<PrincipalDigest> GetPrincipleDigests()
    {
        return _blocks
            .Select(x => new PrincipalDigest
            {
                Id = x.DataBlock.BlockId,
                PrincipleId = x.DataBlock.PrincipleId,
                MessageDigest = x.DataBlock.Digest,
                JwtSignature = x.DataBlock.JwtSignature,
            }).ToArray();
    }

    public Option<BlockAcl> GetAclBlock(string principalId)
    {
        var isOwner = HasAccess(principalId, BlockGrant.Owner);
        if (isOwner.IsOk()) return GetAclBlock();

        var aclOption = GetAclBlock();
        if (aclOption.IsNoContent()) return StatusCode.NotFound;
        if (aclOption.Return().HasAccess(principalId, BlockGrant.Owner).IsError()) return StatusCode.Forbidden;

        return aclOption;
    }

    public Option HasAccess(string principalId, BlockGrant grant)
    {
        var genesisBlock = GetGenesisBlock();
        if (genesisBlock.OwnerPrincipalId == principalId) return StatusCode.OK;

        var aclOption = GetAclBlock();
        if (aclOption.IsNoContent()) return StatusCode.Forbidden;

        var result = aclOption.Return().HasAccess(principalId, grant);
        return result;
    }

    public Option HasAccess(string principalId, BlockGrant grant, string blockType)
    {
        var genesisBlock = GetGenesisBlock();
        if (genesisBlock.OwnerPrincipalId == principalId) return StatusCode.OK;

        var aclOption = GetAclBlock();
        if (aclOption.IsNoContent()) return StatusCode.Forbidden;

        var result = aclOption.Return().HasAccess(principalId, grant, blockType);
        return result;
    }

    private Option CheckWriteAccess(IEnumerable<DataBlock> blocks)
    {
        blocks.NotNull();
        if (_blocks.Count == 0) return StatusCode.OK;

        return blocks.All(x => HasAccess(x.PrincipleId, BlockGrant.Write, x.BlockType).IsOk()) switch
        {
            true => StatusCode.OK,
            false => StatusCode.Unauthorized,
        };
    }

    private Option<BlockAcl> GetAclBlock() => _blocks
        .Where(x => x.DataBlock.BlockType == BlockAcl.BlockType)
        .Select(x => x.DataBlock.ToObject<BlockAcl>())
        .LastOrDefaultOption();

    public static BlockChain operator +(BlockChain self, DataBlock blockData)
    {
        self.Add(blockData);
        return self;
    }
}

