using Toolbox.Actor;
using Toolbox.Tools;

namespace Toolbox.Abstractions;

public static class Extensions
{
    public static ActorKey ToActorKey(this DocumentId documentId) => (ActorKey)documentId.NotNull().Id;
    public static DocumentId ToDocumentId(this ActorKey actorKey) => (DocumentId)actorKey.Value;
}
