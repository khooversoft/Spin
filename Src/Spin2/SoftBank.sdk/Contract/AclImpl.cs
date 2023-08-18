using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Block;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk;

public class AclImpl
{
    private readonly BlockChain _blockChain;
    private readonly IValidator<BlockAcl> _validator;
    private readonly ISign _sign;
    private readonly ILogger _logger;

    public AclImpl(BlockChain blockChain, IValidator<BlockAcl> validator, ISign sign, ILogger logger)
    {
        _blockChain = blockChain.NotNull();
        _validator = validator.NotNull();
        _sign = sign.NotNull();
        _logger = logger.NotNull();
    }

    public Option<BlockAcl> Get(PrincipalId principalId, ScopeContext context) => _blockChain
        .GetReader<BlockAcl>(principalId)
        .LogResult(context.With(_logger).Location())
        .Bind(x => x.GetLatest());

    public async Task<Option> Set(BlockAcl acl, PrincipalId principalId, ScopeContext context)
    {
        context = context.With(_logger);

        var validator = _validator.Validate(acl).LogResult(context.Location());
        if (validator.IsError()) return validator.ToOptionStatus();

        var writer = _blockChain.GetWriter<BlockAcl>(principalId).LogResult(context.With(_logger).Location());
        if (writer.IsError()) writer.ToOptionStatus();

        Option<DataBlock> blockData = await writer.Return().CreateDataBlock(acl, principalId).Sign(_sign, context);
        if (blockData.IsError()) return blockData.ToOptionStatus();

        var add = writer.Return().Add(blockData.Return());
        return add;
    }
}

