namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class TicketNotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ReporterUsername { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
