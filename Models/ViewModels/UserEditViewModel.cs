using System.ComponentModel.DataAnnotations;
using SystemZglaszaniaUsterek.Models.Enums;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nazwa użytkownika")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Nieprawidłowy adres e-mail.")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Imię")]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "Nazwisko")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Rola jest wymagana.")]
        [Display(Name = "Rola")]
        public Role Role { get; set; }

        [DataType(DataType.Password)]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        [Display(Name = "Nowe hasło (opcjonalnie)")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Hasła nie są takie same.")]
        [Display(Name = "Powtórz nowe hasło")]
        public string? ConfirmNewPassword { get; set; }
    }
}
