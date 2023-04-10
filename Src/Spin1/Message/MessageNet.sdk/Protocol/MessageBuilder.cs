using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace MessageNet.sdk.Protocol
{
    public class MessageBuilder
    {
        public Guid? OriginateMessageId { get; set; }

        public MessageUrl? Url { get; set; }

        public MessageUrl? From { get; set; }

        public MessageMethod? Method { get; set; }

        public List<Header> Headers { get; } = new List<Header>();

        public List<Content> Contents { get; } = new List<Content>();

        public HttpStatusCode? Status { get; set; }

        public Message? BaseMessage { get; set; }


        public MessageBuilder SetOriginateMessageId(Guid originateMessageId) => this.Action(x => x.OriginateMessageId = originateMessageId);

        public MessageBuilder SetUrl(MessageUrl messageUrl) => this.Action(x => x.Url = messageUrl);

        public MessageBuilder SetFrom(MessageUrl messageUrl) => this.Action(x => x.From = messageUrl);

        public MessageBuilder SetMethod(MessageMethod method) => this.Action(x => x.Method = method);

        public MessageBuilder AddHeader(params Header?[] headers) => this.Action(x => x.Headers.AddRange(headers.Where(x => x != null).Select(x => (Header)x!)));

        public MessageBuilder AddContent(params Content?[] contents) => this.Action(x => x.Contents.AddRange(contents.Where(x => x != null).Select(x => (Content)x!)));

        public MessageBuilder SetStatus(HttpStatusCode stats) => this.Action(x => x.Status = stats);

        public MessageBuilder SetBaseMessage(Message baseMessage) => this.Action(x => x.BaseMessage = baseMessage);

        public virtual Message Build()
        {
            Verify();

            return new Message
            {
                FromMessageId = OriginateMessageId ?? BaseMessage?.FromMessageId,
                Url = Url!,
                From = From ?? BaseMessage?.From,
                Method = Method.ToString()!,
                Headers = Headers.Concat(BaseMessage?.Headers ?? Array.Empty<Header>()).ToArray(),
                Contents = Contents.Concat(BaseMessage?.Contents ?? Array.Empty<Content>()).ToArray(),
                Status = Status ?? BaseMessage?.Status
            };
        }

        private void Verify()
        {
            Url.VerifyNotNull($"{nameof(Url)} is required");
            Method.VerifyNotNull($"{nameof(Method)} is required");
            Headers.ForEach(x => x.Verify());
            Contents.ForEach(x => x.Verify());
        }
    }
}