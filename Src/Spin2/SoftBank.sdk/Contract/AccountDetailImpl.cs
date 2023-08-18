using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Security.Principal;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk;

public class AccountDetailImpl
{
    private readonly BlockChain _blockChain;
    private readonly ISign _sign;
    private readonly IValidator<AccountDetail> _validator;
    private readonly ILogger _logger;

    public AccountDetailImpl(BlockChain blockChain, ISign sign, IValidator<AccountDetail> validator, ILogger logger)
    {
        _blockChain = blockChain;
        _sign = sign;
        _validator = validator;
        _logger = logger;
    }

    public Option<AccountDetail> Get(PrincipalId principalId, ScopeContext context) => _blockChain
        .GetReader<AccountDetail>(principalId)
        .LogResult(context.With(_logger).Location())
        .Bind(x => x.GetLatest());

    public async Task<Option> Set(AccountDetail detail, ScopeContext context)
    {
        context = context.With(_logger);

        var validator = _validator.Validate(detail);
        if (validator.IsError()) return validator.ToOptionStatus();

        var writer = _blockChain.GetWriter<AccountDetail>(detail.OwnerId).LogResult(context.Location());
        if (writer.IsError()) writer.ToOptionStatus();

        Option<DataBlock> blockData = await writer.Return().CreateDataBlock(detail, detail.OwnerId)
            .Sign(_sign, context)
            .LogResult(context.Location());

        if (blockData.IsError()) return blockData.ToOptionStatus();

        var add = writer.Return().Add(blockData.Return()).LogResult(context.Location());
        return add;
    }
}

