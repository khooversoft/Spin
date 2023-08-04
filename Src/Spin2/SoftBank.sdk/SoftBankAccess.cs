//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using Toolbox.Block;
//using Toolbox.Security.Principal;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SoftBank.sdk;

//public class SoftBankAccess
//{
//    private readonly BlockChain _blockChain;
//    private readonly ISign _sign;
//    private readonly ILogger _logger;

//    public SoftBankAccess(BlockChain blockChain, ISign sign, ILogger logger)
//    {
//        _blockChain = blockChain.NotNull();
//        _sign = sign.NotNull();
//        _logger = logger.NotNull()  ;
//    }

//    public async Task<Option> AddAccess(BlockAcl acl, string principalId, ScopeContext context)
//    {
//        context = context.With(_logger);
//        context.Location().LogInformation("Addng access to account for principalId={principalId}", principalId);

//        Option<DataBlock> aclBlock = await DataBlockBuilder
//                .CreateAclBlock(acl, principalId, context)
//                .Sign(_sign, context)
//                .LogResult(context.Location());

//        if (aclBlock.IsError()) return aclBlock.ToOptionStatus();

//        return _blockChain.Add(aclBlock.Return()).LogResult(context.Location());
//    }

//    //public Option<BlockAcl> GetAccess() => _blockChain.GetAclBlock();
//}
