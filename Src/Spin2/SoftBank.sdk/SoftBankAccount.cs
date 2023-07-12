using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Types;

namespace SoftBank.sdk;

public class SoftBankAccount
{
    public SoftBankAccount(string ownerPrincipleId, ObjectId objectId)
    {
    }

    public SoftBankAccount(BlockChain blockChain, ObjectId objectId)
    {
    }



    public static Option<SoftBankAccount> Create(BlobPackage package, ScopeContext context)
    {
        Option<BlockChain> blockChain = package.ToBlockChain(context);
        if (blockChain.IsError()) return blockChain.ToOption<SoftBankAccount>();

        return new SoftBankAccount(blockChain.Return(), package.ObjectId.ToObjectId());
    }
}


