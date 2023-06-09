using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Block.Access;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Block.Contract;

public interface IContractHost
{
    Task<Option<T>> Create<T>(ObjectId documentId, ScopeContext context) where T : IContract;
    Task<Option<BlockDocument>> Get(ObjectId documentId, ScopeContext context);
    Task<StatusCode> Set(BlockDocument document, ObjectId documentId, ScopeContext context);
    Task Start(IContract sc, ScopeContext context);
    Task Stop(IContract sc, ScopeContext context);
}

public class ContractHost : IContractHost
{
    private readonly IMessageBroker _messageBroker;
    private readonly IInMemoryStore _documentStore;
    private readonly IServiceProvider _service;
    private readonly ILogger<ContractHost> _logger;

    public ContractHost(IMessageBroker messageBroker, IInMemoryStore documentStore, IServiceProvider service, ILogger<ContractHost> logger)
    {
        _messageBroker = messageBroker.NotNull();
        _documentStore = documentStore.NotNull(); ;
        _service = service.NotNull();
        _logger = logger.NotNull();
    }

    public Task<Option<T>> Create<T>(ObjectId documentId, ScopeContext context)
        where T : IContract
    {
        T sc = ActivatorUtilities.CreateInstance<T>(_service, (IContractHost)this, documentId);
        return Task.FromResult(sc.ToOption());
    }

    public async Task<Option<T>> Load<T>(ObjectId documentId, ScopeContext context)
        where T : IContract
    {
        var oBlockDocument = await Get(documentId, context).ConfigureAwait(false);
        if (oBlockDocument.IsError()) return oBlockDocument.ToOption<T>();

        T sc = ActivatorUtilities.CreateInstance<T>(_service, (IContractHost)this, documentId, oBlockDocument.Return());
        return sc;
    }

    public async Task<Option<BlockDocument>> Get(ObjectId documentId, ScopeContext context)
    {
        Option<Document> oDocument = await _documentStore.Get(documentId);
        if (oDocument.StatusCode.IsError()) return oDocument.ToOption<BlockDocument>();

        BlockDocument document = oDocument.Value.ToObject<BlockDocument>();
        //document.Validate();
        return new Option<BlockDocument>(document);
    }

    public async Task<StatusCode> Set(BlockDocument document, ObjectId documentId, ScopeContext context)
    {
        //document.Validate();

        Document payload = document.ToDocument(documentId);

        var status = await _documentStore.Set(payload, context, payload.ETag);
        return status;
    }

    public async Task Start(IContract sc, ScopeContext context)
    {
        sc.NotNull();
        _logger.LogInformation(context.Location(), "Starting, DocumentId={path}", sc.DocumentId);

        await sc.Start(context);
    }

    public async Task Stop(IContract sc, ScopeContext context)
    {
        sc.NotNull();
        await sc.Stop(context);
    }
}
