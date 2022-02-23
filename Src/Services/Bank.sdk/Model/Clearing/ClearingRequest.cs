using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;

namespace Bank.sdk.Model;

public record ClearingRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public string FromId { get; init; } = null!;

    public string ToId { get; init; } = null!;

    public decimal Amount { get; init; }

    public IReadOnlyList<string> Properties { get; init; } = null!;
}


public static class ClearingRequestExtensions
{
    public static QueueMessage ToQueueMessage(this ClearingRequest clearingRequest) => QueueMessage.Create(clearingRequest);

    public static ClearingRequest ToClearingRequest(this QueueMessage queueMessage) => queueMessage.GetContent<ClearingRequest>();

    public static ClearingRequestResponse ToClearingRequestResponse(this ClearingRequest clearingRequest, ClearingStatus clearingStatus) => new ClearingRequestResponse
    {
        ReferenceId = clearingRequest.Id,
        ToId = clearingRequest.FromId,
        FromId = clearingRequest.ToId,
        Amount = clearingRequest.Amount,
        Status = clearingStatus
    };

}


