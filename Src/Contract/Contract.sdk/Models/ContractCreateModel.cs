using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.Tools;

namespace Contract.sdk.Models;

public record ContractCreateModel
{
    public string DocumentId { get; init; } = null!;
    public string Creator { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Name { get; set; } = null!;
    public string PrincipleId { get; set; } = null!;
}


public static class ContractCreateModelExtensions
{
    public static void Verify(this ContractCreateModel subject)
    {
        DocumentId.VerifyId(subject.DocumentId);
        subject.Creator.NotEmpty();
        subject.Description.NotEmpty();
        subject.Name.NotEmpty();
        subject.PrincipleId.NotEmpty();
    }
}
