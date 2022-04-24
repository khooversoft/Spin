using Artifact.sdk;
using Bank.Abstractions.Model;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.DocumentStore;
using Toolbox.Model;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankDocument
{
    private readonly ArtifactClient _artifactClient;
    private readonly ILogger<BankDocument> _logger;
    private readonly BankOption _bankOption;

    internal BankDocument(BankOption bankOption, ArtifactClient artifactClient, ILogger<BankDocument> logger)
    {
        _bankOption = bankOption.VerifyNotNull(nameof(bankOption));
        _artifactClient = artifactClient.VerifyNotNull(nameof(artifactClient));
        _logger = logger.VerifyNotNull(nameof(logger));
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token)
    {
        _logger.LogInformation($"Deleting documentId={documentId}");
        documentId = documentId.WithContainer(_bankOption.ArtifactContainerName);

        return await _artifactClient.Delete(documentId, token);
    }

    public async Task<BankAccount?> Get(DocumentId documentId, CancellationToken token)
    {
        _logger.LogInformation($"Getting documentId={documentId}");
        documentId = documentId.WithContainer(_bankOption.ArtifactContainerName);

        Document? document = await _artifactClient.Get(documentId, token);
        if (document == null) return null;

        return document.DeserializeData<BankAccount>();
    }

    public async Task Set(BankAccount entry, CancellationToken token)
    {
        _logger.LogInformation($"Set documentId={entry.AccountId}");

        DocumentId documentId = (DocumentId)entry.AccountId;
        documentId = documentId.WithContainer(_bankOption.ArtifactContainerName);

        Document document = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(entry)
            .Build();

        await _artifactClient.Set(document, token);
    }

    public async Task<BatchSet<DatalakePathItem>> Search(QueryParameter queryParameter, CancellationToken token)
    {
        queryParameter = queryParameter with { Container = _bankOption.ArtifactContainerName };
        BatchSet<DatalakePathItem> batch = await _artifactClient.Search(queryParameter).ReadNext(token);
        return batch;
    }
}
