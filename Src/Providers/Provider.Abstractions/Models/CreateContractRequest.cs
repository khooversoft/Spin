using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Extensions;
using Toolbox.Abstractions.Protocol;
using Toolbox.Tools;

namespace Provider.Abstractions.Models;

public record CreateContractRequest
{
    public string PrincipleId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DocumentId DocumentId { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Description { get; init; } = null!;
}


public static class CreateContractRequestExtensions
{
    public static bool IsValid(this CreateContractRequest subject) =>
        subject != null &&
        subject.PrincipleId.IsNotEmpty() &&
        subject.Name.IsNotEmpty() &&
        subject.DocumentId != null &&
        subject.Issuer.IsNotEmpty() &&
        subject.Description.IsNotEmpty();
}