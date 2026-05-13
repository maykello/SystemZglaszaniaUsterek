using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class TicketHistoryModel
    {
        [Key]
        public int Id { get; set; }
        
        public TicketModel? Ticket { get; set; }
        
        public StatusModel? OldStatus { get; set; }
        
        public StatusModel? NewStatus { get; set; }
        
        public UserModel? ChangedBy { get; set; }
        
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
