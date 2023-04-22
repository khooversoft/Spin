using System.Collections.Concurrent;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Block.Signature;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;

namespace Toolbox.Block.Document;

public class BlockDocument
{
    private readonly ConcurrentDictionary<string, PrincipalSignature> _signatures = new(StringComparer.OrdinalIgnoreCase);
    private BlockChain _blockChain;

    public BlockDocument(BlockChain blockChain) => _blockChain = blockChain.NotNull();
    public BlockDocument(string ownerPrincipleId)
    {
        ownerPrincipleId.NotEmpty();

        _blockChain = new BlockChainBuilder()
            .SetPrincipleId(ownerPrincipleId)
            .Build();
    }

    public BlockDocument Add(PrincipalSignature signature) => this.Action(x => _signatures[signature.Kid] = signature.NotNull());

    public BlockCollectionStream GetCollection(string streamName) => new BlockCollectionStream(_blockChain, streamName);
    public BlockScalarStream GetScalar(string streamName) => new BlockScalarStream(_blockChain, streamName);

    public BlockDocument Sign() => this.Action(_ => _blockChain = _blockChain.Sign(x => _signatures[x]));
    public BlockDocument Validate() => this.Action(_ => _blockChain.Validate(x => _signatures[x]));

    public string GetMerkleTreeValue() => _blockChain.ToMerkleTree().BuildTree().ToString();

    public string ToJson() => _blockChain.ToBlockChainModel().ToJson();

    public byte[] ToZip() => _blockChain.ToZip();

    public static BlockDocument Create(string json) => json
        .ToObject<BlockChainModel>()
        .NotNull(name: "Serialization error")
        .ToBlockChain()
        .Func(x => new BlockDocument(x));

    public static BlockDocument Create(byte[] zipData) => zipData
        .ToBlockChain()
        .Func(x => new BlockDocument(x));
}
