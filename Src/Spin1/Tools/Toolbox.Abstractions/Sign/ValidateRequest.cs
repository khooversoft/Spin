using Toolbox.Tools;

namespace Toolbox.Sign;

public record ValidateRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public IReadOnlyList<PrincipleDigest> PrincipleDigests { get; init; } = new List<PrincipleDigest>();
}


public static class ValidateRequestExtensions
{
    public static void Verify(this ValidateRequest subject)
    {
        subject.NotNull();
        subject.PrincipleDigests.NotNull();
        subject.PrincipleDigests.Assert(x => x.Count > 0, nameof(subject.PrincipleDigests));
    }

    public static ValidateRequest ToValidateRequest(this IEnumerable<PrincipleDigest> digests) => new ValidateRequest { PrincipleDigests = digests.ToList() };
}
