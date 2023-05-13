using Microsoft.Extensions.Logging;
using Toolbox.Block.Access;
using Toolbox.Block.Test.Scenarios.Bank.Models;
using Toolbox.DocumentContainer;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Block.Test.Scenarios.Bank;

public class BankHost
{
    private readonly IMessageBroker _messageBroker;
    private readonly IDocumentStore _documentStore;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BankHost> _logger;

    public BankHost(IMessageBroker messageBroker, IDocumentStore documentStore, ILoggerFactory loggerFactory)
    {
        _messageBroker = messageBroker;
        _documentStore = documentStore;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<BankHost>();
    }

    public IMessageBroker Message => _messageBroker;

    public Task<Option<BankSC>> Create(DocumentId documentId, string accountName, string ownerPrincipleId)
    {
        var accountBlock = new BankAccountBlock(documentId, accountName, ownerPrincipleId);
        var sc = new BankSC(this, accountBlock, _messageBroker, _loggerFactory.CreateLogger<BankSC>());
        return Task.FromResult(sc.ToOption());
    }

    public async Task<Option<BankAccountBlock>> Get(DocumentId documentId, ScopeContext context)
    {
        Option<Document> oDocument = await _documentStore.Get(documentId);
        if (oDocument.StatusCode.IsError()) return oDocument.ToOption<BankAccountBlock>();

        BlockDocument document = oDocument.Value.ToObject<BlockDocument>();
        var sc = new BankAccountBlock(document);
        //sc.Validate();

        return new Option<BankAccountBlock>(sc);
    }

    public async Task<StatusCode> Set(BankAccountBlock sc, ScopeContext context)
    {
        //sc.Validate();

        Document document = sc.GetDocument();
        var status = await _documentStore.Set(document, context, document.ETag);
        return status;
    }

    public Task Start(BankSC sc, ScopeContext context)
    {
        sc.NotNull();
        _logger.LogInformation(context.Location(), "Starting, DocumentId={path}", sc.AccountBlock.DocumentId);

        _messageBroker.AddRoute<PushTransfer, TransferResult>(GetPushPath(sc), sc.PushCommand, context);
        _messageBroker.AddRoute<ApplyDeposit, TransferResult>(GetApplyDeplositPath(sc), sc.ApplyDeposit, context);

        return Task.CompletedTask;
    }

    public Task Stop(BankSC sc, ScopeContext context)
    {
        sc.NotNull();
        _messageBroker.RemoveRoute(GetPushPath(sc), context);
        _messageBroker.RemoveRoute(GetApplyDeplositPath(sc), context);

        return Task.CompletedTask;
    }

    private static string GetPushPath(BankSC sc) => $"{sc.AccountBlock.DocumentId}/push";
    private static string GetApplyDeplositPath(BankSC sc) => $"{sc.AccountBlock.DocumentId}/applyDeposit";
}
