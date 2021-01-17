using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Azure.Queue
{
    public record MessagePayload
    {
        public string ContentType { get; init; } = null!;

        public byte[] Data { get; init; } = null!;

        public Guid MessageId { get; init; } = Guid.NewGuid();

        public Guid? CallerMessageId { get; init; }

        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    }
}
