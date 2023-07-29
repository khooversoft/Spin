using Microsoft.Extensions.Logging;
using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk;

public class SoftBankAccount
{
    private readonly BlockChain _blockChain;
    private readonly ISign _sign;
    private readonly ISignValidate _validate;
    private readonly ILogger<SoftBankAccount> _logger;

    public SoftBankAccount(BlockChain blockChain, ISign sign, ISignValidate validate, ILogger<SoftBankAccount> logger)
    {
        _blockChain = blockChain.NotNull();
        _sign = sign.NotNull();
        _validate = validate.NotNull();
        _logger = logger.NotNull();

        Access = new SoftBankAccess(_blockChain, _sign, _logger);
    }

    public BlockScalarStream<AccountDetail> GetAccountDetailStream() => _blockChain.GetScalarStream<AccountDetail>(nameof(AccountDetail));
    public BlockStream<LedgerItem> GetLedgerStream() => _blockChain.GetStream<LedgerItem>(nameof(LedgerItem));
    public SoftBankAccess Access { get; }

    public async Task<Option> ValidateBlockChain(ISignValidate signValidate, ScopeContext context)
    {
        return await _blockChain.ValidateBlockChain(signValidate, context);
    }

    public decimal GetBalance() => GetLedgerStream().Get().Sum(x => x.NaturalAmount);

    public BlobPackage ToBlobPackage() => _blockChain.ToBlobPackage();

    public async Task TransferFunds(decimal amount, ObjectId toAccount, PrincipalId ownerId)
    {
    }
}


public static class SoftBankAccountExtensions
{
    public static Option<AccountDetail> GetAccountDetail(this SoftBankAccount softbank) => softbank.NotNull()
        .GetAccountDetailStream()
        .Get();

    public static Option CanAccess(this SoftBankAccount softbank, SoftBankGrant access, PrincipalId principalId) => softbank.NotNull()
        .GetAccountDetailStream()
        .Get() switch
    {
        { StatusCode: StatusCode.OK } v => v.Return().CanAccess(access.ToString(), principalId),
        _ => new Option(StatusCode.Unauthorized),
    };
}
