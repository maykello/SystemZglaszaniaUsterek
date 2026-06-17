using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        [Display(Name = "Adres e-mail")]
        public string Email { get; set; } = string.Empty;
    }
}
