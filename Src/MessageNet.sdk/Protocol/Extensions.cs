using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public static class Extensions
    {
        public static Message CreateResponseMessage(this Message message, Content? content = null, IEnumerable<Header>? headers = null)
        {
            return new Message
            {
                ToEndpoint = message.FromEndpoint,
                FromEndpoint = message.FromEndpoint,
                ContentItems = content != null ? new[] { content } : Array.Empty<Content>(),
                Headers = (headers ?? Array.Empty<Header>()).ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase)
            };
        }

        public static Message? GetFromMessage(this Packet packet)
        {
            packet.VerifyNotNull(nameof(packet));

            return (packet.Messages ?? Array.Empty<Message>())
                .Skip(1)
                .FirstOrDefault();
        }

        public static void Verify(this Content subject)
        {
            subject.VerifyNotNull(nameof(subject));
            subject.ContentType.VerifyNotEmpty(nameof(subject.ContentType));
        }

        public static void Verify(this Header subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Name.VerifyNotEmpty(nameof(subject.Name));
        }

        public static void Verify(this Message subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.FromEndpoint.VerifyNotNull(nameof(subject.FromEndpoint));
            subject.ToEndpoint.VerifyNotNull(nameof(subject.ToEndpoint));

            subject.Headers
                .VerifyNotNull(nameof(subject.Headers))
                .ForEach(x => x.Value.Verify());

            subject.ContentItems
                .VerifyNotNull(nameof(subject.ContentItems))
                .VerifyAssert(x => x.Count > 0, "content(s) is empty")
                .ForEach(x => x.Verify());
        }

        public static void Verify(this Packet subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Version.VerifyNotEmpty(nameof(subject.Version));

            subject.Messages
                .VerifyNotNull(nameof(subject.Messages))
                .VerifyAssert(x => x.Count > 0, "message(s) are empty")
                .ForEach(x => x.Verify());
        }

        public static Packet WithMessage(this Packet packet, Message message)
        {
            packet.VerifyNotNull(nameof(packet));

            return packet with
            {
                Messages = (packet.Messages ?? Array.Empty<Message>())
                    .Prepend(message)
                    .ToArray(),
            };
        }
    }
}