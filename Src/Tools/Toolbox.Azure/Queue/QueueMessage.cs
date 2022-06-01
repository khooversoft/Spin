using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue;

public record QueueMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();

    public string Version { get; init; } = "1.0";

    public string ContentType { get; init; } = null!;

    public string Content { get; init; } = null!;
}


public static class QueueMessageExtensions
{
    public static QueueMessage ToQueueMessage<T>(this T subject, string? contentType = null)
    {
        subject.NotNull();

        return new QueueMessage
        {
            ContentType = contentType ?? typeof(T).Name,
            Content = Json.Default.Serialize(subject),
        };
    }

    public static T GetContent<T>(this QueueMessage queueMessage)
    {
        queueMessage.NotNull();

        return queueMessage.Content.ToObject<T>()
            .NotNull(name: "Deserialize failure");
    }
}
