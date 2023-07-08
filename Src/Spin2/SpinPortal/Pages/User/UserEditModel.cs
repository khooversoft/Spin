using SpinCluster.sdk.Actors.Key;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinPortal.Pages.Tenant;
using System;
using System.ComponentModel.DataAnnotations;
using Toolbox.Extensions;

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

    public static UserModel ConvertTo(this UserEditModel subject) => new UserModel
    {
        UserId = subject.UserId,
        GlobalId = subject.GlobalPrincipleId,
        DisplayName = subject.DisplayName,
        FirstName = subject.FirstName,
        LastName = subject.LastName,
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