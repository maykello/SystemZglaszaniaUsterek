using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public int CurrentIssues { get; set; }
        public int TotalResolved { get; set; }
        public int WeekResolved { get; set; }
        public string AvgResolutionTimeText { get; set; } = "0h";

        public List<AnnouncementModel> Announcements { get; set; } = new();
    }
}
