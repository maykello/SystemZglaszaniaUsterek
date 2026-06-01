using System.ComponentModel.DataAnnotations;
using SystemZglaszaniaUsterek.Models.Enums;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class AnnouncementFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tytuł musi mieć od 3 do 200 znaków.")]
        [Display(Name = "Tytuł")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Treść jest wymagana.")]
        [StringLength(4000, MinimumLength = 5, ErrorMessage = "Treść musi mieć od 5 do 4000 znaków.")]
        [Display(Name = "Treść ogłoszenia")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Waga")]
        public AnnouncementSeverity Severity { get; set; } = AnnouncementSeverity.Info;

        [Display(Name = "Aktywne (widoczne na stronie głównej)")]
        public bool IsActive { get; set; } = true;
    }
}
