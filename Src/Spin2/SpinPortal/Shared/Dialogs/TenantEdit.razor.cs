//using System.ComponentModel.DataAnnotations;
//using Microsoft.AspNetCore.Components.Forms;

//namespace SpinPortal.Shared.Dialogs;

//public partial class TenantEdit
//{
//    RegisterAccountForm model = new RegisterAccountForm();
//    bool success;

//    private void OnValidSubmit(EditContext context)
//    {
//        success = true;
//        StateHasChanged();
//    }


//    public class RegisterAccountForm
//    {
//        [Required]
//        [StringLength(8, ErrorMessage = "Name length can't be more than 8.")]
//        public string Username { get; set; } = null!;

//        [Required]
//        [EmailAddress]
//        public string Email { get; set; } = null!;

//        [Required]
//        [StringLength(30, ErrorMessage = "Password must be at least 8 characters long.", MinimumLength = 8)]
//        public string Password { get; set; } = null!;

//        [Required]
//        [Compare(nameof(Password))]
//        public string Password2 { get; set; } = null!;
//    }
//}
