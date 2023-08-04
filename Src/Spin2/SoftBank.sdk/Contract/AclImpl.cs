using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Block;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk;

public class AclImpl
{
    private readonly BlockChain _blockChain;
    private readonly ISign _sign;
    private readonly ILogger _logger;

    public AclImpl(BlockChain blockChain, ISign sign, ILogger logger)
    {
        _blockChain = blockChain;
        _sign = sign;
        _logger = logger;
    }

    public Option<BlockAcl> Get(PrincipalId principalId, ScopeContext context) => _blockChain
        .GetReader<BlockAcl>(principalId)
        .LogResult(context.With(_logger).Location())
        .Bind(x => x.GetLatest());

    public async Task<Option> Write(BlockAcl acl, PrincipalId principalId, ScopeContext context)
    {
        var writer = _blockChain.GetWriter<BlockAcl>(principalId).LogResult(context.With(_logger).Location());
        if (writer.IsError()) writer.ToOptionStatus();

        Option<DataBlock> blockData = await writer.Return().CreateDataBlock(acl, principalId).Sign(_sign, context);
        if (blockData.IsError()) return blockData.ToOptionStatus();

        var add = writer.Return().Add(blockData.Return());
        return add;
    }
}

