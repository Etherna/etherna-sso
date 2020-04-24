using System.ComponentModel.DataAnnotations;

namespace Etherna.SSOServer.ViewModels
{
    public class RegisterPswdViewModel
    {
        // Properties.
        [Required(ErrorMessage = "EnterUsername")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "EnterPassword")]
        [StringLength(100, ErrorMessage = "MinimumPasswordLength", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword")]
        [Compare("RegisterPassword", ErrorMessage = "PasswordDoesNotMatch")]
        public string ConfirmPassword { get; set; }
    }
}
