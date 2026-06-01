namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class TicketListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? PriorityName { get; set; }
        public string? StatusName { get; set; }
        public bool IsClosed { get; set; }
        public string? ReporterUsername { get; set; }
        public string? TechnicianUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
