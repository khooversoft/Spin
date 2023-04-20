﻿namespace Toolbox.Protocol;

public sealed record Document
{
    public required string DocumentId { get; init; } = null!;
    public required string TypeName { get; init; } = null!;
    public required string Content { get; init; } = null!;
    public string? HashBase64 { get; init; }
    public string? PrincipleId { get; init; }
    public string? Tags { get; init; } = null!;

    public bool Equals(Document? obj)
    {
        return obj is Document document &&
               DocumentId == document.DocumentId &&
               TypeName == document.TypeName &&
               Content == document.Content &&
               HashBase64 == document.HashBase64 &&
               PrincipleId == document.PrincipleId &&
               Tags == document.Tags;
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, TypeName, Content, HashBase64, PrincipleId, Tags);
}
