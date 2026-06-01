using System.ComponentModel.DataAnnotations;
using SystemZglaszaniaUsterek.Models.Enums;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Nazwa użytkownika jest wymagana.")]
        [StringLength(64, MinimumLength = 3, ErrorMessage = "Nazwa użytkownika musi mieć od 3 do 64 znaków.")]
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

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [DataType(DataType.Password)]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Hasło musi mieć co najmniej 6 znaków.")]
        [Display(Name = "Hasło")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Hasła nie są takie same.")]
        [Display(Name = "Powtórz hasło")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rola jest wymagana.")]
        [Display(Name = "Rola")]
        public Role Role { get; set; } = Role.User;
    }
}
