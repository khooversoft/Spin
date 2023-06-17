using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directory.sdk.Models;

public record UserContact
{
    public string? StreeAddress { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Country { get; init; }
    public string? BusinessPhone { get; init; }
    public string? MobilePhone { get; init; }
    public string? HomePhone { get; init; }
    public string? Email { get; init; }
}
