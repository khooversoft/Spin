using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceAlive.sdk.Models;

public class UserRegistrationModel
{
    public string? UserName { get; set; }
    public string? Email { get; set; }

    // Contact
    public string? PhoneNumber { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}
