namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public int CurrentIssues { get; set; }
        public int TotalResolved { get; set; }
        public int WeekResolved { get; set; }
        public string AvgResolutionTimeText { get; set; } = "0h";
    }
}
