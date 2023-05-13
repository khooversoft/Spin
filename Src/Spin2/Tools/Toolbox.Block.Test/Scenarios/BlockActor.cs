using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Block.Access;
using Toolbox.DocumentContainer;
using Toolbox.Types.Maybe;
using Toolbox.Types;

namespace Toolbox.Block.Test.Scenarios;

public class BlockActor<T>
{
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<BankAccountSCActor> _logger;

    public BlockActor(IDocumentStore documentStore, ILogger<BankAccountSCActor> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
    }

    public async Task<Option<T>> Get(DocumentId documentId, ScopeContext context)
    {
        Option<Document> oDocument = await _documentStore.Get(documentId);
        if (oDocument.StatusCode.IsError()) return oDocument.ToOption<T>();

        T document = oDocument.Value.ToObject<T>();

        return new Option<T>(document);
    }

    public async Task<StatusCode> Set(BankAccountBlock sc, ScopeContext context)
    {
        Document document = sc.GetDocument();
        var status = await _documentStore.Set(document, context, document.ETag);
        return status;
    }
}
