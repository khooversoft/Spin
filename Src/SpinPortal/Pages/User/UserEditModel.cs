using System.ComponentModel.DataAnnotations;
using SpinCluster.sdk.Actors.User;

namespace SpinPortal.Pages.User;

public class UserEditModel
{
    public string GlobalPrincipleId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ActiveDate { get; set; }
    public string ActiveDateText => ActiveDate?.ToString() ?? "< not active >";

    [Required, StringLength(50)] public string UserId { get; set; } = null!;
    [Required, StringLength(100)] public string DisplayName { get; set; } = null!;
    [Required, StringLength(100)] public string FirstName { get; set; } = null!;
    [Required, StringLength(100)] public string LastName { get; set; } = null!;
    [Required, EmailAddress] public string Email { get; set; } = null!;
    public bool AccountEnabled { get; set; }
    [Required] public string PhoneNumber { get; set; } = null!;
    [Required] public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    [Required] public string City { get; set; } = null!;
    [Required] public string State { get; set; } = null!;
    [Required] public string ZipCode { get; set; } = null!;
    [Required] public string Country { get; set; } = null!;
}


public static class UserEditModelExtensions
{
    public static UserEditModel ConvertTo(this UserModel subject) => new UserEditModel
    {
        UserId = subject.UserId,
        GlobalPrincipleId = subject.GlobalId,
        DisplayName = subject.DisplayName,
        FirstName = subject.FirstName,
        LastName = subject.LastName,
        Email = subject.PrincipalId,
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

    public static UserModel ConvertTo(this UserEditModel subject) => new UserModel
    {
        UserId = subject.UserId,
        GlobalId = subject.GlobalPrincipleId,
        DisplayName = subject.DisplayName,
        FirstName = subject.FirstName,
        LastName = subject.LastName,
        PrincipalId = subject.Email,
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