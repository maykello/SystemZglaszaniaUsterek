using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class TicketModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Title { get; set; }
        
        [Required]
        public required string Description { get; set; }
        
        public string? Location { get; set; }
        
        public PriorityModel? Priority { get; set; }
        
        public StatusModel? Status { get; set; }

        [Required]
        public CategoryModel Category { get; set; } = null!;

        public int CategoryId { get; set; }

        public UserModel? Reporter { get; set; }
        
        public UserModel? Technician { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
        
        public ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
        public ICollection<TicketHistoryModel> History { get; set; } = new List<TicketHistoryModel>();
        public ICollection<AttachmentModel> Attachments { get; set; } = new List<AttachmentModel>();
    }
}
