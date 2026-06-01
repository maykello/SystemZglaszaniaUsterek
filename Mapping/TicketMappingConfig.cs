using Mapster;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.ViewModels;

namespace SystemZglaszaniaUsterek.Mapping
{
    public class TicketMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<TicketModel, TicketListItemViewModel>()
                .Map(d => d.CategoryName, s => s.Category != null ? s.Category.Name : string.Empty)
                .Map(d => d.PriorityName, s => s.Priority != null ? s.Priority.Name : null)
                .Map(d => d.StatusName, s => s.Status != null ? s.Status.Name : null)
                .Map(d => d.IsClosed, s => s.Status != null && s.Status.IsClosed)
                .Map(d => d.ReporterUsername, s => s.Reporter != null ? s.Reporter.Username : null)
                .Map(d => d.TechnicianUsername, s => s.Technician != null ? s.Technician.Username : null);

            config.NewConfig<TicketModel, TicketNotificationViewModel>()
                .Map(d => d.ReporterUsername, s => s.Reporter != null ? s.Reporter.Username : null)
                .Map(d => d.CategoryName, s => s.Category != null ? s.Category.Name : null)
                .Map(d => d.CreatedAtUtc, s => s.CreatedAt);

            config.NewConfig<AttachmentModel, AttachmentItemViewModel>();

            config.NewConfig<CommentModel, CommentItemViewModel>()
                .Map(d => d.AuthorUsername, s => s.User != null ? s.User.Username : null);

            config.NewConfig<TicketHistoryModel, HistoryItemViewModel>()
                .Map(d => d.OldStatusName, s => s.OldStatus != null ? s.OldStatus.Name : null)
                .Map(d => d.NewStatusName, s => s.NewStatus != null ? s.NewStatus.Name : null)
                .Map(d => d.ChangedByUsername, s => s.ChangedBy != null ? s.ChangedBy.Username : null);

            config.NewConfig<TicketModel, TicketDetailsViewModel>()
                .Map(d => d.CategoryName, s => s.Category != null ? s.Category.Name : string.Empty)
                .Map(d => d.PriorityId, s => s.Priority != null ? (int?)s.Priority.Id : null)
                .Map(d => d.PriorityName, s => s.Priority != null ? s.Priority.Name : null)
                .Map(d => d.StatusId, s => s.Status != null ? (int?)s.Status.Id : null)
                .Map(d => d.StatusName, s => s.Status != null ? s.Status.Name : null)
                .Map(d => d.IsClosed, s => s.Status != null && s.Status.IsClosed)
                .Map(d => d.ReporterId, s => s.Reporter != null ? (int?)s.Reporter.Id : null)
                .Map(d => d.ReporterUsername, s => s.Reporter != null ? s.Reporter.Username : null)
                .Map(d => d.TechnicianId, s => s.Technician != null ? (int?)s.Technician.Id : null)
                .Map(d => d.TechnicianUsername, s => s.Technician != null ? s.Technician.Username : null)
                .Map(d => d.Attachments, s => s.Attachments)
                .Map(d => d.Comments, s => s.Comments)
                .Map(d => d.History, s => s.History)
                .Ignore(d => d.AvailableStatuses!)
                .Ignore(d => d.AvailableTechnicians!)
                .Ignore(d => d.AvailablePriorities!)
                .Ignore(d => d.CanChangeStatus)
                .Ignore(d => d.CanAssignTechnician)
                .Ignore(d => d.CanSelfAssign)
                .Ignore(d => d.CanComment)
                .Ignore(d => d.CanEdit);
        }
    }
}
