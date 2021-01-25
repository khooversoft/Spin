using System;
using System.Collections.Generic;

namespace MessageNet.sdk.Protocol
{
    public record Message
    {
        public Guid MessageId { get; init; } = Guid.NewGuid();

        public Guid? OriginateMessageId { get; init; }

        public EndpointId FromEndpoint { get; init; } = null!;

        public EndpointId ToEndpoint { get; init; } = null!;

        public IReadOnlyDictionary<string, Header>? Headers { get; init; }

        public IReadOnlyList<Content> Contents { get; init; } = null!;
    }
}