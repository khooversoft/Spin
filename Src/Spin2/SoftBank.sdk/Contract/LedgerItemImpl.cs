//using Microsoft.Extensions.Logging;
//using SoftBank.sdk.Models;
//using Toolbox.Block;
//using Toolbox.Security.Principal;
//using Toolbox.Tools;
//using Toolbox.Tools.Validation;
//using Toolbox.Types;

//namespace SoftBank.sdk;

//public class LedgerItemImpl
//{
//    private readonly BlockChain _blockChain;
//    private readonly ISign _sign;
//    private readonly IValidator<LedgerItem> _validator;
//    private readonly ILogger _logger;

//    public LedgerItemImpl(BlockChain blockChain, ISign sign, IValidator<LedgerItem> validator, ILogger logger)
//    {
//        _blockChain = blockChain.NotNull();
//        _sign = sign.NotNull();
//        _validator = validator.NotNull();
//        _logger = logger.NotNull();
//    }

//    public Option<BlockReader<LedgerItem>> GetReader(string principalId, ScopeContext context) => _blockChain
//        .GetReader<LedgerItem>(nameof(LedgerItem), principalId)
//        .LogResult(context.With(_logger).Location());

//    public async Task<Option> Add(LedgerItem ledger, ScopeContext context)
//    {
//        context = context.With(_logger);

//        var validator = _validator.Validate(ledger);
//        if (validator.IsError()) return validator.ToOptionStatus();

//        var writer = _blockChain.GetWriter<LedgerItem>(ledger.OwnerId).LogResult(context.Location());
//        if (writer.IsError()) writer.ToOptionStatus();

//        Option<DataBlock> blockData = await writer.Return().CreateDataBlock(ledger, ledger.OwnerId)
//            .Sign(_sign, context)
//            .LogResult(context.Location());

//        if (blockData.IsError()) return blockData.ToOptionStatus();

//        var add = writer.Return().Add(blockData.Return());
//        return add;
//    }

//    public Option<decimal> GetBalance(string principalId, ScopeContext context)
//    {
//        var stream = GetReader(principalId, context);
//        if (stream.IsError()) return stream.ToOptionStatus<decimal>();

//        return stream.Return().List().Sum(x => x.NaturalAmount);
//    }
//}

