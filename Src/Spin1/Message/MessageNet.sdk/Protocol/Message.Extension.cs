using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public static class MessageExtensions
    {
        [return: NotNull]
        public static Message Verify(this Message? subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.From.VerifyNotNull(nameof(subject.From));
            subject.Url.VerifyNotNull(nameof(subject.Url));
            subject.From.VerifyNotNull(nameof(subject.From));
            subject.Method.VerifyNotNull(nameof(subject.Method));

            subject.Headers.ForEach(x => x.Verify());
            subject.Contents.ForEach(x => x.Verify());

            return subject;
        }

        public static MessageBuilder ToBuilder(this Message message) => new MessageBuilder().SetBaseMessage(message);

        public static bool IsSuccessStatusCode(this Message message) => message.Status switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.Created => true,
            HttpStatusCode.Accepted => true,
            HttpStatusCode.NoContent => true,

            _ => false
        };

        public static Message EnsureSuccessStatusCode(this Message message) => message
                .VerifyNotNull(nameof(message))
                .VerifyAssert(x => x.IsSuccessStatusCode(), x => $"Invalid status code: {x}");
    }
}
