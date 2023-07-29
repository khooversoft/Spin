using Microsoft.Extensions.Logging;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk;

public class SoftBankFactory
{
    private readonly ISign _sign;
    private readonly ISignValidate _signValidate;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<SoftBankFactory> _logger;

    public SoftBankFactory(ISign sign, ISignValidate signValidate, ILoggerFactory loggerFactory)
    {
        _sign = sign.NotNull();
        _signValidate = signValidate.NotNull();
        _loggerFactory = loggerFactory.NotNull();
        _logger = _loggerFactory.CreateLogger<SoftBankFactory>();
    }

    public Task<Option<SoftBankAccount>> Create(ObjectId objectId, string principalId, ScopeContext context)
    {
        return Create(objectId, principalId, null, context);
    }

    public async Task<Option<SoftBankAccount>> Create(ObjectId objectId, string principalId, BlockAcl? blockAcl, ScopeContext context)
    {
        principalId.NotEmpty();
        objectId.NotNull();
        context = context.With(_logger);

        Option<BlockChain> blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId)
            .SetPrincipleId(principalId)
            .AddAccess(blockAcl)
            .Build(_sign, context)
            .LogResult(context.Location());

        if (blockChain.IsError()) return blockChain.ToOption<SoftBankAccount>();

        return new SoftBankAccount(blockChain.Return(), _sign, _signValidate, _loggerFactory.CreateLogger<SoftBankAccount>());
    }

    public async Task<Option<SoftBankAccount>> Create(BlobPackage package, ScopeContext context)
    {
        context = context.With(_logger);

        Option<BlockChain> blockChain = package.ToBlockChain(context);
        if (blockChain.IsError()) return blockChain.ToOption<SoftBankAccount>();

        Option validationResult = await blockChain.Return().ValidateBlockChain(_signValidate, context).LogResult(context.Location());
        if (validationResult.StatusCode.IsError()) return validationResult.ToOption<SoftBankAccount>();

        return new SoftBankAccount(blockChain.Return(), _sign, _signValidate, _loggerFactory.CreateLogger<SoftBankAccount>());
    }
}
