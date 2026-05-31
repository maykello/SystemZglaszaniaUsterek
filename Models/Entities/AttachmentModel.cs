using System.ComponentModel.DataAnnotations;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class AttachmentModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TicketModel Ticket { get; set; } = null!;

        public int TicketId { get; set; }

        [Required]
        public required string Url { get; set; }

        [Required]
        public required string PublicId { get; set; }

        [Required]
        public required string OriginalFileName { get; set; }

        [Required]
        public required string ContentType { get; set; }

        public long SizeBytes { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
