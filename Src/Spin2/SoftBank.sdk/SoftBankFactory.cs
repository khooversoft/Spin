using Microsoft.Extensions.DependencyInjection;
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
    private readonly IServiceProvider _service;
    private readonly ILogger<SoftBankFactory> _logger;

    public SoftBankFactory(ISign sign, ISignValidate signValidate, IServiceProvider service, ILogger<SoftBankFactory> logger)
    {
        _sign = sign.NotNull();
        _signValidate = signValidate.NotNull();
        _service = service.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option<SoftBankAccount>> Create(ObjectId objectId, PrincipalId principalId, ScopeContext context)
    {
        return Create(objectId, principalId, null, context);
    }

    public async Task<Option<SoftBankAccount>> Create(ObjectId objectId, PrincipalId principalId, BlockAcl? blockAcl, ScopeContext context)
    {
        objectId.NotNull();
        principalId.NotNull();
        context = context.With(_logger);

        Option<BlockChain> blockChain = await new BlockChainBuilder()
            .SetObjectId(objectId)
            .SetPrincipleId(principalId)
            .AddAccess(blockAcl)
            .Build(_sign, context)
            .LogResult(context.Location());

        if (blockChain.IsError()) return blockChain.ToOptionStatus<SoftBankAccount>();

        var softBank = ActivatorUtilities.CreateInstance<SoftBankAccount>(_service, blockChain.Return());
        return softBank;
    }

    public async Task<Option<SoftBankAccount>> Create(BlobPackage package, ScopeContext context)
    {
        context = context.With(_logger);

        Option<BlockChain> blockChain = package.ToBlockChain(context);
        if (blockChain.IsError()) return blockChain.ToOptionStatus<SoftBankAccount>();

        Option validationResult = await blockChain.Return().ValidateBlockChain(_signValidate, context).LogResult(context.Location());
        if (validationResult.StatusCode.IsError()) return validationResult.ToOptionStatus<SoftBankAccount>();

        var softBank = ActivatorUtilities.CreateInstance<SoftBankAccount>(_service, blockChain.Return());
        return softBank;
    }
}
