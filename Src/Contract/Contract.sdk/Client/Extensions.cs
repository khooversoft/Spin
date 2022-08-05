using Contract.sdk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Contract.sdk.Client;

public static class Extensions
{
    public static T? GetLast<T>(this IEnumerable<DataBlockResult> values) where T : class => values
        .LastOrDefault(x => x.BlockType == typeof(T).GetTypeName())
        ?.Document
        ?.ToObject<T>();

    public static IReadOnlyList<T> GetAll<T>(this IEnumerable<DataBlockResult> values) where T : class => values
        .Where(x => x.BlockType == typeof(T).GetTypeName())
        .Select(x => x.Document.ToObject<T>())
        .ToList();


    public static Task<AppendResult> Append<T>(this ContractClient client, DocumentId documentId, T value, string principleId, CancellationToken token = default) where T : class
    {
        client.NotNull();
        documentId.NotNull();
        value.NotNull();
        principleId.NotNull();

        var batch = new DocumentBatch()
            .SetDocumentId(documentId)
            .Add(value, principleId)
            .Build();

        return client.Append(batch, token);
    }
}
