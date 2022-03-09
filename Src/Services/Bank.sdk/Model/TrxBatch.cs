using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Tools;

namespace Bank.sdk.Model;

public record TrxBatch<T>
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<T> Items { get; init; } = new List<T>();
}


public static class TrxBatchExtensions
{
    public static QueueMessage ToQueueMessage<T>(this TrxBatch<T> subject)
    {
        subject.VerifyNotNull(nameof(subject));

        return new QueueMessage
        {
            ContentType = typeof(T).Name,
            Content = Json.Default.Serialize(subject),
        };
    }

    public static TrxBatch<T> ToBatch<T>(this QueueMessage queueMessage)
    {
        queueMessage.VerifyNotNull(nameof(queueMessage));

        return queueMessage.ContentType switch
        {
            nameof(T) => queueMessage.GetContent<TrxBatch<T>>(),

            _ => throw new ArgumentException($"Unknown contentType={queueMessage.ContentType}")
        };
    }
}

