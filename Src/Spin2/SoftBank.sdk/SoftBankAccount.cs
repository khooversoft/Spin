using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Block.Access;
using Toolbox.Block.Container;
using Toolbox.Block.Serialization;
using Toolbox.Data;
using Toolbox.Types;
using Toolbox.Tools.Validation;

namespace SoftBank.sdk;

public class SoftBankAccount : BlockDocument
{
    public SoftBankAccount(string ownerPrincipleId, ObjectId objectId)
        : base(ownerPrincipleId, objectId)
    {
    }

    public SoftBankAccount(BlockChain blockChain, ObjectId objectId)
        : base(blockChain, objectId)
    {
    }



    public static Option<SoftBankAccount> Create(BlobPackage package, ScopeContext context)
    {
        Option<BlockChain> blockChain = package.ToBlockChain(context.Location());
        if (blockChain.IsError()) return blockChain.ToOption<SoftBankAccount>();

        return new SoftBankAccount(blockChain.Return(), package.ObjectId.ToObjectId());
    }
}


