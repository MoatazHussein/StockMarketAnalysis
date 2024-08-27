using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace StockMarket.Models
{
    public class UserRegisterRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Provide a valid UserName")]
        public string? UserName { get; set; }

        [Required]
        public string? Mobile { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=(.*[!@#$%^&*(),.?\':{}|<>]){2,}).+$", ErrorMessage = "Password must contain at least two special characters.")]
        public string Password { get; set; } = string.Empty;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
