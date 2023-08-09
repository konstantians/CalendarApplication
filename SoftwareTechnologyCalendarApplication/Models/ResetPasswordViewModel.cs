using SoftwareTechnologyCalendarApplicationMVC.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace SoftwareTechnologyCalendarApplication.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [Display(Name = "New Password")]
        [CompareFields("ConfirmPassword", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Token{ get; set; }
    }
}
