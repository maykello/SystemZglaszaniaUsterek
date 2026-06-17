using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        [Display(Name = "Nazwa użytkownika")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Imię")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Display(Name = "Nazwisko")]
        public string? LastName { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Aktualne hasło (wymagane przy zmianie hasła)")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        [Display(Name = "Nowe hasło")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są takie same.")]
        [Display(Name = "Powtórz nowe hasło")]
        public string? ConfirmNewPassword { get; set; }
    }
}
