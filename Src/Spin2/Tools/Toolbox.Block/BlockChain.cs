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
    public BlockChain(IEnumerable<BlockNode> blockNodes) => _blocks = blockNodes.NotNull().ToList();

    public int Count => _blocks.Count;

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

        return new Option(StatusCode.OK);
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

    public Option<BlockReader<T>> GetReader<T>(BlockType blockType, PrincipalId principalId) where T : class
    {
        return IsAuthorized(BlockGrant.Read, blockType, principalId) switch
        {
            false => new Option<BlockReader<T>>(StatusCode.Forbidden),
            true => new BlockReader<T>(_blocks.Where(x => x.DataBlock.BlockType == blockType).Select(x => x.DataBlock.ToObject<T>())),
        };
    }

    public Option<BlockWriter<T>> GetWriter<T>(BlockType blockType, PrincipalId principalId) where T : class
    {
        return IsAuthorized(BlockGrant.Write, blockType, principalId) switch
        {
            false => new Option<BlockWriter<T>>(StatusCode.Forbidden),
            true => new BlockWriter<T>(this, blockType),
        };
    }

    public Option<BlockReader<BlockNode>> GetNodeReader(PrincipalId principalId)
    {
        principalId.NotNull();

        GenesisBlock genesisBlock = GetGenesisBlock();

        return (genesisBlock.OwnerPrincipalId == principalId) switch
        {
            true => new BlockReader<BlockNode>(_blocks),
            false => new Option<BlockReader<BlockNode>>(StatusCode.Forbidden),
        };
    }

    public GenesisBlock GetGenesisBlock() => _blocks
        .Where(x => x.DataBlock.BlockType == GenesisBlock.BlockType)
        .Select(x => x.DataBlock.ToObject<GenesisBlock>())
        .Last();

    public Option IsOwner(PrincipalId principalId) => GetGenesisBlock() switch
    {
        var v when v.OwnerPrincipalId == principalId => new Option(StatusCode.OK),
        _ => new Option(StatusCode.Forbidden),
    };

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

    public BlockChainModel ToBlockChainModel() => new BlockChainModel
    {
        Blocks = _blocks.ToArray(),
    };

    private Option CheckWriteAccess(IEnumerable<DataBlock> blocks)
    {
        blocks.NotNull();
        if (_blocks.Count == 0) return new Option(StatusCode.OK);

        return blocks.All(x => IsAuthorized(BlockGrant.Write, x.BlockType, x.PrincipleId)) switch
        {
            true => new Option(StatusCode.OK),
            false => new Option(StatusCode.Unauthorized),
        };
    }

    private bool IsAuthorized(BlockGrant grant, BlockType blockType, PrincipalId principalId)
    {
        grant.IsEnumValid<BlockGrant>();
        blockType.NotNull();
        principalId.NotNull();

        GenesisBlock genesisBlock = GetGenesisBlock();
        if (genesisBlock.OwnerPrincipalId == principalId) return true;

        Option<BlockAcl> aclOption = this.GetAclBlock();
        if (aclOption == Option<BlockAcl>.None) return false;

        bool hasAccess = aclOption.Return().HasAccess(grant, blockType, principalId);
        return hasAccess;
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

