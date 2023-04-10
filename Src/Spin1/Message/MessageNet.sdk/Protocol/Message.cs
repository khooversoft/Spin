using System;
using System.Collections.Generic;
using System.Net;

namespace MessageNet.sdk.Protocol
{
    public record Message
    {
        public Guid MessageId { get; init; } = Guid.NewGuid();

        public string Version { get; init; } = "1.0";

        public Guid? FromMessageId { get; init; }

        public MessageUrl? From { get; init; }

        public MessageUrl Url { get; init; } = null!;

        public string Method { get; init; } = null!;

        public HttpStatusCode? Status { get; init; } = null!;

        public IReadOnlyList<Header> Headers { get; init; } = null!;

        public IReadOnlyList<Content> Contents { get; init; } = null!;

        public Message? InnerMessage { get; init; }
    }
}