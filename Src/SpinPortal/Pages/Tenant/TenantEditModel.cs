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
        if (!NameId.IsValid(TenantId))
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
        TenantName = subject.Name,
        Contact = subject.ContactName,
        Email = subject.Email,
        AccountEnabled = subject.AccountEnabled,
        CreatedDate = subject.CreatedDate.ToUniversalTime(),
        ActiveDate = subject.ActiveDate?.ToUniversalTime(),

        //PhoneNumber = subject.Phone.FirstOrDefault()?.Number!,
        //Address1 = subject.Address.FirstOrDefault()?.Address1!,
        //Address2 = subject.Address.FirstOrDefault()?.Address2!,
        //City = subject.Address.FirstOrDefault()?.City!,
        //State = subject.Address.FirstOrDefault()?.State!,
        //ZipCode = subject.Address.FirstOrDefault()?.ZipCode!,
        //Country = subject.Address.FirstOrDefault()?.Country!,
    };

    public static TenantModel ConvertTo(this TenantEditModel subject) => new TenantModel
    {
        TenantId = subject.TenantId,
        GlobalId = subject.GlobalPrincipleId,
        Name = subject.TenantName,
        ContactName = subject.Contact,
        Email = subject.Email,
        AccountEnabled = subject.AccountEnabled,
        CreatedDate = subject.CreatedDate,
        ActiveDate = subject.ActiveDate,

        //Phone = new UserPhoneModel { Type = "Default", Number = subject.PhoneNumber }.ToEnumerable().ToArray(),

        //Address = new UserAddressModel
        //{
        //    Type = "Default",
        //    Address1 = subject.Address1,
        //    Address2 = subject.Address2,
        //    City = subject.City,
        //    State = subject.State,
        //    ZipCode = subject.ZipCode,
        //    Country = subject.Country,
        //}.ToEnumerable().ToArray(),
    };
}