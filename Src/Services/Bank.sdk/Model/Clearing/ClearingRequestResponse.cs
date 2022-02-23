using Toolbox.Azure.Queue;

namespace Bank.sdk.Model;


public record ClearingRequestResponse
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public string ReferenceId { get; init; } = null!;

    public string FromId { get; init; } = null!;

    public string ToId { get; init; } = null!;

    public decimal Amount { get; init; }

    public TrxStatus Status { get; set; }
}


public static class ClearingRequestResponseExtensions
{
    public static QueueMessage ToQueueMessage(this ClearingRequestResponse clearingRequest) => QueueMessage.Create(clearingRequest);

    public static ClearingRequestResponse ToClearingRequestResponse(this QueueMessage queueMessage) => queueMessage.GetContent<ClearingRequestResponse>();
}
