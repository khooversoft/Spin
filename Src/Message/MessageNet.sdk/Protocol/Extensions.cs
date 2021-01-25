using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public static class Extensions
    {
        public static MessagePacket CreateResponseMessage(this MessagePacket packet, Content? content = null, IEnumerable<Header>? headers = null)
        {
            var responseMessage = packet.GetMessage()
                .VerifyNotNull("No messages")
                .CreateResponseMessage(content, headers);

            return packet with
            {
                Messages = packet.Messages
                    .Prepend(responseMessage)
                    .ToArray(),
            };
        }

        public static Message CreateResponseMessage(this Message message, Content? content = null, IEnumerable<Header>? headers = null)
        {
            return new Message
            {
                OriginateMessageId = message.MessageId,
                ToEndpoint = message.FromEndpoint,
                FromEndpoint = message.ToEndpoint,
                Contents = content != null ? new[] { content } : Array.Empty<Content>(),
                Headers = (headers ?? Array.Empty<Header>()).ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase)
            };
        }

        public static Message? GetMessage(this MessagePacket packet) => packet?.Messages?[0];

        public static Guid? GetMessageId(this MessagePacket packet) => packet.GetMessage()?.OriginateMessageId;

        public static Guid? GetOriginateMessageId(this MessagePacket packet) => packet.GetMessage()?.OriginateMessageId;

        public static bool IsValid(this EndpointId endpointId)
        {
            if (endpointId == null) return false;
            return !endpointId.ToString().IsEmpty();
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

            subject.Headers?.ForEach(x => x.Value.Verify());

            subject.Contents
                .VerifyNotNull(nameof(subject.Contents))
                .VerifyAssert(x => x.Any(), "content(s) is empty")
                .ForEach(x => x.Verify());
        }

        public static void Verify(this MessagePacket subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Version.VerifyNotEmpty(nameof(subject.Version));

            subject.Messages
                .VerifyNotNull(nameof(subject.Messages))
                .VerifyAssert(x => x.Count > 0, "message(s) are empty")
                .ForEach(x => x.Verify());
        }

        public static MessagePacket WithMessage(this MessagePacket subject, Message message)
        {
            subject.VerifyNotNull(nameof(subject));
            message.VerifyNotNull(nameof(message));

            return new MessagePacket
            {
                Messages = subject.Messages
                    .Prepend(message)
                    .ToArray(),
            };
        }
    }
}