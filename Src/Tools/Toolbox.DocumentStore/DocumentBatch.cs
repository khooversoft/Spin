﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Extensions;

namespace Toolbox.DocumentStore;

public class DocumentBatch
{
    private readonly IList<(object Value, string PrincipleId)> _values = new List<(object Value, string PrincipleId)>();

    public int Count => _values.Count;

    public DocumentId? DocumentId { get; set; }
    public DocumentBatch SetDocumentId(DocumentId documentId) => this.Action(x => x.DocumentId = documentId.NotNull());
    public DocumentBatch Add<T>(T value, string principleId) where T : class => this.Action(x => x._values.Add((value.NotNull(), principleId.NotEmpty())));

    public Batch<Document> Build()
    {
        DocumentId.NotNull(name: $"{nameof(DocumentId)} is required");
        _values.Count.Assert(x => x > 0, "Empty list");

        return new Batch<Document>
        {
            Items = _values.Select(x => new DocumentBuilder()
                .SetDocumentId(DocumentId)
                .SetData(x.Value)
                .SetObjectClass(x.Value.GetType().GetTypeName())
                .SetPrincipleId(x.PrincipleId)
                .Build()
            ).ToList()
        };
    }

    public static implicit operator Batch<Document>(DocumentBatch batch) => batch.NotNull().Build();
}
