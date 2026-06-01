namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public class ReportFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? TechnicianId { get; set; }
        public int? CategoryId { get; set; }
    }

    public class ReportsRepairViewModel
    {
        public ReportFilterViewModel Filter { get; set; } = new();

        public int Total { get; set; }
        public int Open { get; set; }
        public int Closed { get; set; }
        public double AvgResolutionHours { get; set; }

        public List<ReportRowViewModel> ByStatus { get; set; } = new();
        public List<ReportRowViewModel> ByPriority { get; set; } = new();
        public List<ReportRowViewModel> ByCategory { get; set; } = new();
        public List<ReportRowViewModel> ByTechnician { get; set; } = new();

        public List<TicketListItemViewModel> Items { get; set; } = new();

        public List<SystemZglaszaniaUsterek.Models.Entities.UserModel> Technicians { get; set; } = new();
        public List<SystemZglaszaniaUsterek.Models.Entities.CategoryModel> Categories { get; set; } = new();
    }

    public class ReportRowViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
