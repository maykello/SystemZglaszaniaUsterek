using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.Enums
{
    public enum AnnouncementSeverity
    {
        [Display(Name = "Informacja")]
        Info = 0,

        [Display(Name = "Ostrzeżenie")]
        Warning = 1,

        [Display(Name = "Krytyczne")]
        Critical = 2
    }
}
