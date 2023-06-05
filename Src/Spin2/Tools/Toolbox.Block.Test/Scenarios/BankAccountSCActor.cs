//using Microsoft.Extensions.Logging;
//using Toolbox.Block.Access;
//using Toolbox.DocumentContainer;
//using Toolbox.Extensions;
//using Toolbox.Types;
//using Toolbox.Types.Maybe;

//namespace Toolbox.Block.Test.Scenarios;

//public class BankAccountSCActor
//{
//    private readonly IDocumentStore _documentStore;
//    private readonly ILogger<BankAccountSCActor> _logger;

//    public BankAccountSCActor(IDocumentStore documentStore, ILogger<BankAccountSCActor> logger)
//    {
//        _documentStore = documentStore;
//        _logger = logger;
//    }

//    public Task<Option<BankAccountBlock>> Create(DocumentId documentId, string accountName, string ownerPrincipleId, ScopeContext context)
//    {
//        return Task.FromResult(new BankAccountBlock(documentId, accountName, ownerPrincipleId).ToOption());
//    }

//    public async Task<Option<BankAccountBlock>> Get(DocumentId documentId, ScopeContext context)
//    {
//        Option<Document> oDocument = await _documentStore.Get(documentId);
//        if (oDocument.StatusCode.IsError()) return oDocument.ToOption<BankAccountBlock>();

//        BlockDocument document = oDocument.Value.ToObject<BlockDocument>();
//        var sc = new BankAccountBlock(document);

//        return new Option<BankAccountBlock>(sc);
//    }

//    public async Task<StatusCode> Set(BankAccountBlock sc, ScopeContext context)
//    {
//        Document document = sc.GetDocument();
//        var status = await _documentStore.Set(document, context, document.ETag);
//        return status;
//    }
//}
