using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Models.ViewModels
{
    public enum TicketScope
    {
        Mine = 0,
        Assigned = 1,
        All = 2
    }

    public class TicketFilterViewModel
    {
        public TicketScope Scope { get; set; } = TicketScope.All;

        public int? StatusId { get; set; }
        public int? PriorityId { get; set; }
        public int? CategoryId { get; set; }
        public int? TechnicianId { get; set; }
        public int? ReporterId { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public string? Search { get; set; }

        public string? SortBy { get; set; }
        public bool SortDesc { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class TicketListResultViewModel
    {
        public List<TicketListItemViewModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

        public TicketFilterViewModel Filter { get; set; } = new();
        public List<StatusModel> Statuses { get; set; } = new();
        public List<PriorityModel> Priorities { get; set; } = new();
        public List<CategoryModel> Categories { get; set; } = new();
        public List<UserModel> Technicians { get; set; } = new();
    }
}
