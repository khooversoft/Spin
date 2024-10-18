using System.ComponentModel.DataAnnotations;

namespace TicketShareWeb.Components.Account;

public class UserProfileModel
{
    [Display(Name = "Name")]
    public string? Name { get; set; } = null!;

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; } = null!;

    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; } = null!;

    [Display(Name = "Address 1")]
    public string? Address1 { get; set; } = null!;

    [Display(Name = "Address 2")]
    public string? Address2 { get; set; } = null!;

    [Display(Name = "City")]
    public string? City { get; set; } = null!;

    [Display(Name = "State")]
    public string? State { get; set; } = null!;

    [Display(Name = "Zip Code")]
    public string? ZipCode { get; set; } = null!;
}
