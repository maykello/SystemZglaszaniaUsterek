using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class PriorityModel
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public required string Name { get; set; }
        
        public ICollection<TicketModel> Tickets { get; set; } = new List<TicketModel>();
    }
}
