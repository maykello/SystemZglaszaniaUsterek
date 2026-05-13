using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class CommentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Content { get; set; }
        
        public TicketModel? Ticket { get; set; }
        
        public UserModel? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
