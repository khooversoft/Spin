using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue;

public class QueueMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public string Version { get; init; } = "1.0";

    public string ContentType { get; init; } = null!;

    public string Content { get; init; } = null!;

    public static QueueMessage Create<T>(T value)
    {
        value.VerifyNotNull(nameof(value));

        return new QueueMessage
        {
            ContentType = typeof(T).Name,
            Content = value.ToJson()
        };
    }
}


public static class QueueMessageExtensions
{
    public static T GetContent<T>(this QueueMessage queueMessage)
    {
        queueMessage.VerifyNotNull(nameof(queueMessage));
        queueMessage.ContentType.VerifyAssert(
            x => x == typeof(T).Name,
            x => $"Invalid content type, required type{typeof(T).Name}, is {queueMessage.ContentType}"
            );

        return queueMessage.Content.ToObject<T>()
            .VerifyNotNull("Deserialize failure");
    }
}
