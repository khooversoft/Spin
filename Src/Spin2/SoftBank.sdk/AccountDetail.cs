using Toolbox.Types;

namespace SoftBank.sdk;

public record AccountDetail
{
    public string ObjectId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}