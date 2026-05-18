using System.ComponentModel.DataAnnotations;
using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class TicketCreateViewModel
    {
        [Required(ErrorMessage = "Tytuł usterki jest wymagany")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Tytuł musi zawierać od 5 do 200 znaków")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Opis usterki jest wymagany")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Opis musi zawierać od 10 do 2000 znaków")]
        public string? Description { get; set; }

        public string? Location { get; set; }

        [Display(Name = "Kategoria")]
        public int? CategoryId { get; set; }

        public List<CategoryModel>? Categories { get; set; }

        [Display(Name = "Przesłane Zdjęcia")]
        public List<IFormFile>? AttachedFiles { get; set; }
    }
}

