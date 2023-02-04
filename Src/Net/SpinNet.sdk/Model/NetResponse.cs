using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SpinNet.sdk.Model;

public record NetResponse
{
    public required HttpStatusCode StatusCode { get; init; }
    public string? Message { get; init; }
}
