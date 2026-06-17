using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        [Display(Name = "Nowe hasło")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są takie same.")]
        [Display(Name = "Powtórz nowe hasło")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
