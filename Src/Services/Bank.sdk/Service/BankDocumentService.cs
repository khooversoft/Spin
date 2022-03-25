using Artifact.sdk;
using Bank.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankDocumentService
{
    private readonly ArtifactClient _artifactClient;
    private readonly ILogger<BankDocumentService> _logger;
    private readonly string _container;

    public BankDocumentService(string container, ArtifactClient artifactClient, ILogger<BankDocumentService> logger)
    {
        _container = container.VerifyNotEmpty(nameof(container));
        _artifactClient = artifactClient.VerifyNotNull(nameof(artifactClient));
        _logger = logger.VerifyNotNull(nameof(logger));
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token)
    {
        _logger.LogInformation($"Deleting documentId={documentId}");
        documentId = documentId.WithContainer(_container);

        return await _artifactClient.Delete(documentId, token);
    }

    public async Task<BankAccount?> Get(DocumentId documentId, CancellationToken token)
    {
        _logger.LogInformation($"Getting documentId={documentId}");
        documentId = documentId.WithContainer(_container);

        Document? document = await _artifactClient.Get(documentId, token);
        if (document == null) return null;

        return document.DeserializeData<BankAccount>();
    }

    public async Task Set(BankAccount entry, CancellationToken token)
    {
        _logger.LogInformation($"Set documentId={entry.AccountId}");

        DocumentId documentId = (DocumentId)entry.AccountId;
        documentId = documentId.WithContainer(_container);

        Document document = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(entry)
            .Build();

        await _artifactClient.Set(document, token);
    }

    public async Task<BatchSet<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token)
    {
        queryParameter = queryParameter with { Container = "bank" };
        BatchSet<DatalakePathItem> batch = await _artifactClient.Search(queryParameter).ReadNext(token);
        return batch;
    }
}
