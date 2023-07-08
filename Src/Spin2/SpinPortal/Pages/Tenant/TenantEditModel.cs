using System.ComponentModel.DataAnnotations;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinPortal.Pages.Tenant;

public class TenantEditModel : IValidatableObject
{
    public string GlobalPrincipleId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ActiveDate { get; set; }
    public string ActiveDateText => ActiveDate?.ToString() ?? "< not active >";

    [Required, StringLength(50)] public string TenantId { get; set; } = null!;
    [Required, StringLength(100)] public string TenantName { get; set; } = null!;
    [Required, StringLength(100)] public string Contact { get; set; } = null!;
    [Required, EmailAddress] public string Email { get; set; } = null!;
    public bool AccountEnabled { get; set; }
    [Required] public string PhoneNumber { get; set; } = null!;
    [Required] public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    [Required] public string City { get; set; } = null!;
    [Required] public string State { get; set; } = null!;
    [Required] public string ZipCode { get; set; } = null!;
    [Required] public string Country { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ObjectId.IsNameValid(TenantId))
        {
            yield return new ValidationResult("Tenant Id is not valid, only alpha numeric, [-._]", new[] { nameof(TenantId) });
        }
    }
}


public static class TenantEditModelExtensions
{
    public static TenantEditModel ConvertTo(this TenantModel subject) => new TenantEditModel
    {
        TenantId = subject.TenantId,
        GlobalPrincipleId = subject.GlobalId,
        TenantName = subject.TenantName,
        Contact = subject.Contact,
        Email = subject.Email,
        AccountEnabled = subject.AccountEnabled,
        CreatedDate = subject.CreatedDate.ToUniversalTime(),
        ActiveDate = subject.ActiveDate?.ToUniversalTime(),

        PhoneNumber = subject.Phone.FirstOrDefault()?.Number!,
        Address1 = subject.Addresses.FirstOrDefault()?.Address1!,
        Address2 = subject.Addresses.FirstOrDefault()?.Address2!,
        City = subject.Addresses.FirstOrDefault()?.City!,
        State = subject.Addresses.FirstOrDefault()?.State!,
        ZipCode = subject.Addresses.FirstOrDefault()?.ZipCode!,
        Country = subject.Addresses.FirstOrDefault()?.Country!,
    };

    public static TenantModel ConvertTo(this TenantEditModel subject) => new TenantModel
    {
        TenantId = subject.TenantId,
        GlobalId = subject.GlobalPrincipleId,
        TenantName = subject.TenantName,
        Contact = subject.Contact,
        Email = subject.Email,
        AccountEnabled = subject.AccountEnabled,
        CreatedDate = subject.CreatedDate,
        ActiveDate = subject.ActiveDate,

        Phone = new UserPhoneModel { Type = "Default", Number = subject.PhoneNumber }.ToEnumerable().ToArray(),

        Addresses = new UserAddressModel
        {
            Type = "Default",
            Address1 = subject.Address1,
            Address2 = subject.Address2,
            City = subject.City,
            State = subject.State,
            ZipCode = subject.ZipCode,
            Country = subject.Country,
        }.ToEnumerable().ToArray(),
    };
}