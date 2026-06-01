using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class AddCommentViewModel
    {
        [Required]
        public int TicketId { get; set; }

        [Required(ErrorMessage = "Treść komentarza jest wymagana.")]
        [StringLength(2000, MinimumLength = 1, ErrorMessage = "Komentarz musi mieć od 1 do 2000 znaków.")]
        [Display(Name = "Komentarz")]
        public string Content { get; set; } = string.Empty;
    }
}
