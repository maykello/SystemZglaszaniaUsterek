using System.ComponentModel.DataAnnotations;
using SystemZglaszaniaUsterek.Models.Enums;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class EditUserRoleViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rola jest wymagana.")]
        [Display(Name = "Rola użytkownika")]
        public Role Role { get; set; }
    }
}
