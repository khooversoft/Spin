using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk;

public class SoftBankAccount
{
    private readonly BlockChain _blockChain;

    public SoftBankAccount(BlockChain blockChain) => _blockChain = blockChain.NotNull();

    public BlockScalarStream<AccountDetail> GetAccountDetailStream() => _blockChain.GetScalarStream<AccountDetail>(nameof(AccountDetail));
    public BlockStream<LedgerItem> GetLedgerStream() => _blockChain.GetStream<LedgerItem>(nameof(LedgerItem));

    public async Task<Option> ValidateBlockChain(ISignValidate signValidate, ScopeContext context)
    {
        return await _blockChain.ValidateBlockChain(signValidate, context);
    }


    public static Option<SoftBankAccount> Create(BlobPackage package, ScopeContext context)
    {
        Option<BlockChain> blockChain = package.ToBlockChain(context);
        if (blockChain.IsError()) return blockChain.ToOption<SoftBankAccount>();

        return new SoftBankAccount(blockChain.Return());
    }

    public static async Task<Option<SoftBankAccount>> Create(string ownerPrincipleId, ObjectId objectId, ISign sign, ScopeContext context)
    {
        Option<BlockChain> blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId)
            .SetPrincipleId(ownerPrincipleId)
            .Build(sign, context);

        if (blockChain.IsError()) return blockChain.ToOption<SoftBankAccount>();

        return new SoftBankAccount(blockChain.Return());
    }
}
