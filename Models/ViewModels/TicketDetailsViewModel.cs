using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class TicketDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int? PriorityId { get; set; }
        public string? PriorityName { get; set; }

        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public bool IsClosed { get; set; }

        public int? ReporterId { get; set; }
        public string? ReporterUsername { get; set; }

        public int? TechnicianId { get; set; }
        public string? TechnicianUsername { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<AttachmentItemViewModel> Attachments { get; set; } = new();
        public List<CommentItemViewModel> Comments { get; set; } = new();
        public List<HistoryItemViewModel> History { get; set; } = new();

        public List<StatusModel> AvailableStatuses { get; set; } = new();
        public List<UserModel> AvailableTechnicians { get; set; } = new();
        public List<PriorityModel> AvailablePriorities { get; set; } = new();
        public bool CanChangeStatus { get; set; }
        public bool CanAssignTechnician { get; set; }
        public bool CanSelfAssign { get; set; }
        public bool CanComment { get; set; }
        public bool CanEdit { get; set; }
    }

    public class AttachmentItemViewModel
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class CommentItemViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? AuthorUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class HistoryItemViewModel
    {
        public int Id { get; set; }
        public string? OldStatusName { get; set; }
        public string? NewStatusName { get; set; }
        public string? ChangedByUsername { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
