using System.ComponentModel.DataAnnotations;
using SystemZglaszaniaUsterek.Models.Enums;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class AnnouncementModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [Required]
        [MaxLength(4000)]
        public required string Content { get; set; }

        public AnnouncementSeverity Severity { get; set; } = AnnouncementSeverity.Info;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public UserModel? CreatedBy { get; set; }
    }
}
