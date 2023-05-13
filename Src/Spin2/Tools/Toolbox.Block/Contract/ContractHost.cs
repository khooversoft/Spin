using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Block.Access;
using Toolbox.DocumentContainer;
using Toolbox.Tools;
using Toolbox.Types.Maybe;
using Toolbox.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Block.Contract;

public class ContractHost
{
    private readonly IMessageBroker _messageBroker;
    private readonly IDocumentStore _documentStore;
    private readonly IServiceProvider _service;
    private readonly ILogger<ContractHost> _logger;

    public ContractHost(IMessageBroker messageBroker, IDocumentStore documentStore, IServiceProvider service, ILogger<ContractHost> logger)
    {
        _messageBroker = messageBroker.NotNull();
        _documentStore = documentStore.NotNull(); ;
        _service = service.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option<T>> Create<T>(DocumentId documentId,ScopeContext context)
        where T : IContract
    {
        T sc = ActivatorUtilities.CreateInstance<T>(_service, this);
        return Task.FromResult(sc.ToOption());
    }

    public async Task<Option<BlockDocument>> Get(DocumentId documentId, ScopeContext context)
    {
        Option<Document> oDocument = await _documentStore.Get(documentId);
        if (oDocument.StatusCode.IsError()) return oDocument.ToOption<BlockDocument>();

        BlockDocument document = oDocument.Value.ToObject<BlockDocument>();
        //document.Validate();
        return new Option<BlockDocument>(document);
    }

    public async Task<StatusCode> Set(BlockDocument document, DocumentId documentId, ScopeContext context)
    {
        //document.Validate();

        Document payload = document.ToDocument(documentId);

        var status = await _documentStore.Set(payload, context, payload.ETag);
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
