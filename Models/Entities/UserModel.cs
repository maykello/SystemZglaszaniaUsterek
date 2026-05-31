using System.ComponentModel.DataAnnotations;
using SystemZglaszaniaUsterek.Models.Enums;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Username { get; set; }
   
        [EmailAddress]
        public string? Email { get; set; }
        
        [Required]
        public required string PasswordHash { get; set; }
        
        [Required]
        public Role Role { get; set; } = Role.User;
        
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public ICollection<TicketModel> ReportedTickets { get; set; } = new List<TicketModel>();
        public ICollection<TicketModel> AssignedTickets { get; set; } = new List<TicketModel>();
        public ICollection<CommentModel> Comments { get; set; } = new List<CommentModel>();
        public ICollection<TicketHistoryModel> HistoryChanges { get; set; } = new List<TicketHistoryModel>();
    }
}
