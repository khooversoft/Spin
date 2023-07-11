using Toolbox.Types;

namespace SoftBank.sdk;

public class AccountDetail
{
    public ObjectId ObjectId { get; init; } = null!;
    public ObjectId OwnerId { get; init; } = null!;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
}