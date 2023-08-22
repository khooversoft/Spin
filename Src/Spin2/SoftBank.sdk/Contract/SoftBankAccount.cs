//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Block;
//using Toolbox.Data;
//using Toolbox.Security.Principal;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace SoftBank.sdk;

//public class SoftBankAccount
//{
//    private readonly BlockChain _blockChain;
//    private readonly ILogger<SoftBankAccount> _logger;
//    private readonly AccountDetailImpl _accountDetailImpl;
//    //private readonly LedgerItemImpl _ledgerItemImpl;
//    private readonly AclImpl _aclImpl;
//    private readonly IServiceProvider _service;

//    public SoftBankAccount(BlockChain blockChain, IServiceProvider service, ILogger<SoftBankAccount> logger)
//    {
//        _blockChain = blockChain.NotNull();
//        _logger = logger.NotNull();

//        _accountDetailImpl = ActivatorUtilities.CreateInstance<AccountDetailImpl>(service, _blockChain, _logger);
//        _ledgerItemImpl = ActivatorUtilities.CreateInstance<LedgerItemImpl>(service, _blockChain, _logger);
//        _aclImpl = ActivatorUtilities.CreateInstance<AclImpl>(service, _blockChain, _logger);
//        _service = service;
//    }

//    public AccountDetailImpl AccountDetail => _accountDetailImpl;
//    //public LedgerItemImpl LedgerItems => _ledgerItemImpl;
//    public AclImpl Acl => _aclImpl;


//    public async Task<Option> ValidateBlockChain(ScopeContext context)
//    {
//        ISignValidate signValidate = _service.GetRequiredService<ISignValidate>();
//        return await _blockChain.ValidateBlockChain(signValidate, context);
//    }

//    public BlobPackage ToBlobPackage() => _blockChain.ToBlobPackage();
//    public Option IsOwner(string principalId) => _blockChain.IsOwner(principalId);

//    //public async Task PushToAccount(decimal amount, ObjectId toAccount, PrincipalId principalId)
//    //{
//    //}
//}

