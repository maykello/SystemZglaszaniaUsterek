using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.Enums;
using SystemZglaszaniaUsterek.Models.Options;
using SystemZglaszaniaUsterek.Models.ViewModels;

namespace SystemZglaszaniaUsterek.Services
{
    public class CreateTicketResult
    {
        public bool Success { get; init; }
        public int? TicketId { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    }

    public class OperationResult
    {
        public bool Success { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public static OperationResult Ok() => new() { Success = true };
        public static OperationResult Fail(params string[] errors) => new() { Success = false, Errors = errors };
    }

    public interface ITicketService
    {
        Task<CreateTicketResult> CreateAsync(TicketCreateViewModel model, int reporterUserId, CancellationToken ct = default);
        Task<List<CategoryModel>> GetCategoriesAsync(CancellationToken ct = default);
        Task<List<PriorityModel>> GetPrioritiesAsync(CancellationToken ct = default);
        Task<List<StatusModel>> GetStatusesAsync(CancellationToken ct = default);
        Task<List<UserModel>> GetTechniciansAsync(CancellationToken ct = default);

        Task<TicketListResultViewModel> ListAsync(TicketFilterViewModel filter, int currentUserId, Role currentRole, CancellationToken ct = default);
        Task<TicketDetailsViewModel?> GetDetailsAsync(int id, int currentUserId, Role currentRole, CancellationToken ct = default);

        Task<OperationResult> ChangeStatusAsync(int ticketId, int newStatusId, int actorUserId, Role actorRole, CancellationToken ct = default);
        Task<OperationResult> AssignTechnicianAsync(int ticketId, int? technicianId, int actorUserId, Role actorRole, CancellationToken ct = default);
        Task<OperationResult> ChangePriorityAsync(int ticketId, int priorityId, int actorUserId, Role actorRole, CancellationToken ct = default);
        Task<OperationResult> AddCommentAsync(int ticketId, string content, int authorUserId, Role authorRole, CancellationToken ct = default);

        Task<List<TicketNotificationViewModel>> GetNewTicketsSinceAsync(DateTime sinceUtc, CancellationToken ct = default);
    }

    public class TicketService : ITicketService
    {
        private const string DefaultStatusName = "Nowe";

        private readonly SystemZglaszaniaUsterekDbContext _db;
        private readonly ICloudinaryService _cloudinary;
        private readonly IAttachmentValidator _validator;
        private readonly CloudinaryOptions _cloudinaryOptions;
        private readonly ILogger<TicketService> _logger;

        public TicketService(
            SystemZglaszaniaUsterekDbContext db,
            ICloudinaryService cloudinary,
            IAttachmentValidator validator,
            IOptions<CloudinaryOptions> cloudinaryOptions,
            ILogger<TicketService> logger)
        {
            _db = db;
            _cloudinary = cloudinary;
            _validator = validator;
            _cloudinaryOptions = cloudinaryOptions.Value;
            _logger = logger;
        }

        public Task<List<CategoryModel>> GetCategoriesAsync(CancellationToken ct = default)
            => _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

        public Task<List<PriorityModel>> GetPrioritiesAsync(CancellationToken ct = default)
            => _db.Priorities.AsNoTracking().OrderBy(p => p.Id).ToListAsync(ct);

        public Task<List<StatusModel>> GetStatusesAsync(CancellationToken ct = default)
            => _db.Statuses.AsNoTracking().OrderBy(s => s.Id).ToListAsync(ct);

        public Task<List<UserModel>> GetTechniciansAsync(CancellationToken ct = default)
            => _db.Users.AsNoTracking()
                .Where(u => !u.IsDeleted && (u.Role == Role.Technician || u.Role == Role.Administrator))
                .OrderBy(u => u.Username)
                .ToListAsync(ct);

        public async Task<CreateTicketResult> CreateAsync(TicketCreateViewModel model, int reporterUserId, CancellationToken ct = default)
        {
            var fileErrors = _validator.Validate(model.AttachedFiles);
            if (fileErrors.Count > 0)
            {
                return new CreateTicketResult { Success = false, Errors = fileErrors };
            }

            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == model.CategoryId, ct);
            if (category == null)
            {
                return new CreateTicketResult { Success = false, Errors = new[] { "Wybrana kategoria nie istnieje." } };
            }

            var defaultStatus = await _db.Statuses.FirstOrDefaultAsync(s => s.Name == DefaultStatusName, ct);
            if (defaultStatus == null)
            {
                return new CreateTicketResult { Success = false, Errors = new[] { "Konfiguracja systemu jest niekompletna: brak statusu domyślnego." } };
            }

            var reporter = await _db.Users.FirstOrDefaultAsync(u => u.Id == reporterUserId, ct);
            if (reporter == null)
            {
                return new CreateTicketResult { Success = false, Errors = new[] { "Nie znaleziono konta zgłaszającego." } };
            }

            var ticket = new TicketModel
            {
                Title = model.Title!.Trim(),
                Description = model.Description!.Trim(),
                Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim(),
                Category = category,
                Status = defaultStatus,
                Reporter = reporter,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync(ct);

            var uploadedPublicIds = new List<string>();
            try
            {
                if (model.AttachedFiles is { Count: > 0 })
                {
                    var folder = $"{_cloudinaryOptions.FolderRoot}/{ticket.Id}";

                    foreach (var file in model.AttachedFiles)
                    {
                        if (file == null || file.Length == 0)
                        {
                            continue;
                        }

                        var uploadResult = await _cloudinary.UploadAsync(file, folder, ct);
                        uploadedPublicIds.Add(uploadResult.PublicId);

                        _db.Attachments.Add(new AttachmentModel
                        {
                            TicketId = ticket.Id,
                            Ticket = ticket,
                            Url = uploadResult.Url,
                            PublicId = uploadResult.PublicId,
                            OriginalFileName = AttachmentValidator.SanitizeFileName(file.FileName),
                            ContentType = file.ContentType ?? "application/octet-stream",
                            SizeBytes = file.Length,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }

                _db.TicketHistories.Add(new TicketHistoryModel
                {
                    Ticket = ticket,
                    OldStatus = null,
                    NewStatus = defaultStatus,
                    ChangedBy = reporter,
                    ChangedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync(ct);
                return new CreateTicketResult { Success = true, TicketId = ticket.Id };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to finalize ticket {TicketId}; rolling back uploads and ticket.", ticket.Id);

                foreach (var publicId in uploadedPublicIds)
                {
                    await _cloudinary.DeleteAsync(publicId, CancellationToken.None);
                }
                try
                {
                    _db.ChangeTracker.Clear();
                    var toRemove = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id, CancellationToken.None);
                    if (toRemove != null)
                    {
                        _db.Tickets.Remove(toRemove);
                        await _db.SaveChangesAsync(CancellationToken.None);
                    }
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to remove orphaned ticket {TicketId}", ticket.Id);
                }

                return new CreateTicketResult
                {
                    Success = false,
                    Errors = new[] { "Nie udało się zapisać zgłoszenia. Spróbuj ponownie za chwilę." }
                };
            }
        }

        public async Task<TicketListResultViewModel> ListAsync(TicketFilterViewModel filter, int currentUserId, Role currentRole, CancellationToken ct = default)
        {
            try
            {
                var query = BuildListQuery(filter, currentUserId, currentRole, out var effectiveScope);

                var totalCount = await query.CountAsync(ct);
                var page = filter.Page < 1 ? 1 : filter.Page;
                var pageSize = filter.PageSize <= 0 ? 20 : Math.Min(filter.PageSize, 100);

                var ordered = ApplySort(query, filter.SortBy, filter.SortDesc, currentRole);

                var items = await ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectToType<TicketListItemViewModel>()
                    .ToListAsync(ct);

                return new TicketListResultViewModel
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Filter = new TicketFilterViewModel
                    {
                        Scope = effectiveScope,
                        StatusId = filter.StatusId,
                        PriorityId = filter.PriorityId,
                        CategoryId = filter.CategoryId,
                        TechnicianId = filter.TechnicianId,
                        ReporterId = filter.ReporterId,
                        DateFrom = filter.DateFrom,
                        DateTo = filter.DateTo,
                        Search = filter.Search,
                        SortBy = filter.SortBy,
                        SortDesc = filter.SortDesc,
                        Page = page,
                        PageSize = pageSize
                    },
                    Statuses = await GetStatusesAsync(ct),
                    Priorities = await GetPrioritiesAsync(ct),
                    Categories = await GetCategoriesAsync(ct),
                    Technicians = currentRole == Role.Administrator ? await GetTechniciansAsync(ct) : new List<UserModel>()
                };
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new TicketListResultViewModel { Filter = filter };
            }
        }

        private static IOrderedQueryable<TicketModel> ApplySort(IQueryable<TicketModel> query, string? sortBy, bool sortDesc, Role currentRole)
        {
            switch (sortBy)
            {
                case "Id":
                    return sortDesc ? query.OrderByDescending(t => t.Id) : query.OrderBy(t => t.Id);
                case "Title":
                    return sortDesc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title);
                case "Category":
                    return sortDesc ? query.OrderByDescending(t => t.Category!.Name) : query.OrderBy(t => t.Category!.Name);
                case "Priority":
                    return sortDesc ? query.OrderByDescending(t => t.Priority!.Name) : query.OrderBy(t => t.Priority!.Name);
                case "Status":
                    return sortDesc ? query.OrderByDescending(t => t.Status!.Name) : query.OrderBy(t => t.Status!.Name);
                case "Reporter":
                    return sortDesc ? query.OrderByDescending(t => t.Reporter!.Username) : query.OrderBy(t => t.Reporter!.Username);
                case "Technician":
                    return sortDesc ? query.OrderByDescending(t => t.Technician!.Username) : query.OrderBy(t => t.Technician!.Username);
                case "CreatedAt":
                    return sortDesc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt);
                default:
                    return currentRole == Role.Administrator
                        ? query.OrderByDescending(t => t.Status != null && t.Status.Name == DefaultStatusName)
                               .ThenByDescending(t => t.Technician == null || t.Priority == null)
                               .ThenByDescending(t => t.CreatedAt)
                        : query.OrderByDescending(t => t.Status != null && t.Status.Name == DefaultStatusName)
                               .ThenByDescending(t => t.CreatedAt);
            }
        }

        private IQueryable<TicketModel> BuildListQuery(TicketFilterViewModel filter, int currentUserId, Role currentRole, out TicketScope effectiveScope)
        {
            effectiveScope = currentRole == Role.User ? TicketScope.Mine : filter.Scope;

            IQueryable<TicketModel> query = _db.Tickets.AsNoTracking()
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Category)
                .Include(t => t.Reporter)
                .Include(t => t.Technician);

            query = effectiveScope switch
            {
                TicketScope.Mine => query.Where(t => t.Reporter != null && t.Reporter.Id == currentUserId),
                TicketScope.Assigned when currentRole != Role.User => query.Where(t => t.Technician != null && t.Technician.Id == currentUserId),
                TicketScope.Assigned => query.Where(_ => false),
                _ => query
            };

            if (filter.StatusId.HasValue)
                query = query.Where(t => t.Status != null && t.Status.Id == filter.StatusId.Value);
            if (filter.PriorityId.HasValue)
                query = query.Where(t => t.Priority != null && t.Priority.Id == filter.PriorityId.Value);
            if (filter.CategoryId.HasValue)
                query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
            if (filter.TechnicianId.HasValue)
                query = query.Where(t => t.Technician != null && t.Technician.Id == filter.TechnicianId.Value);
            if (filter.ReporterId.HasValue && currentRole == Role.Administrator)
                query = query.Where(t => t.Reporter != null && t.Reporter.Id == filter.ReporterId.Value);
            if (filter.DateFrom.HasValue)
            {
                var from = DateTime.SpecifyKind(filter.DateFrom.Value.Date, DateTimeKind.Utc);
                query = query.Where(t => t.CreatedAt >= from);
            }
            if (filter.DateTo.HasValue)
            {
                var to = DateTime.SpecifyKind(filter.DateTo.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(t => t.CreatedAt < to);
            }
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                query = query.Where(t => EF.Functions.Like(t.Title, $"%{s}%") || EF.Functions.Like(t.Description, $"%{s}%"));
            }

            return query;
        }

        public async Task<TicketDetailsViewModel?> GetDetailsAsync(int id, int currentUserId, Role currentRole, CancellationToken ct = default)
        {
            try
            {
                var ticket = await _db.Tickets.AsNoTracking()
                    .Include(t => t.Status)
                    .Include(t => t.Priority)
                    .Include(t => t.Category)
                    .Include(t => t.Reporter)
                    .Include(t => t.Technician)
                    .Include(t => t.Attachments)
                    .Include(t => t.Comments).ThenInclude(c => c.User)
                    .Include(t => t.History).ThenInclude(h => h.OldStatus)
                    .Include(t => t.History).ThenInclude(h => h.NewStatus)
                    .Include(t => t.History).ThenInclude(h => h.ChangedBy)
                    .FirstOrDefaultAsync(t => t.Id == id, ct);

                if (ticket == null)
                {
                    return null;
                }

                if (currentRole == Role.User && (ticket.Reporter == null || ticket.Reporter.Id != currentUserId))
                {
                    return null;
                }

                var dto = ticket.Adapt<TicketDetailsViewModel>();
                dto.Attachments = dto.Attachments.OrderBy(a => a.UploadedAt).ToList();
                dto.Comments = dto.Comments.OrderBy(c => c.CreatedAt).ToList();
                dto.History = dto.History.OrderBy(h => h.ChangedAt).ToList();
                var canChangeStatus = currentRole == Role.Administrator
                                      || (currentRole == Role.Technician && ticket.Technician?.Id == currentUserId);
                var canAssignAdmin = currentRole == Role.Administrator;
                var canSelfAssign = currentRole == Role.Technician
                                    && (ticket.Technician == null || ticket.Technician.Id == currentUserId);
                var isClosed = ticket.Status?.IsClosed ?? false;

                dto.CanChangeStatus = canChangeStatus;
                dto.CanAssignTechnician = canAssignAdmin;
                dto.CanSelfAssign = canSelfAssign;
                dto.CanComment = !isClosed
                                 && (currentRole == Role.Administrator
                                     || currentRole == Role.Technician
                                     || ticket.Reporter?.Id == currentUserId);
                dto.CanEdit = canAssignAdmin;

                dto.AvailableStatuses = canChangeStatus ? await GetStatusesAsync(ct) : new List<StatusModel>();
                dto.AvailableTechnicians = canAssignAdmin ? await GetTechniciansAsync(ct) : new List<UserModel>();
                dto.AvailablePriorities = canChangeStatus ? await GetPrioritiesAsync(ct) : new List<PriorityModel>();

                return dto;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return null;
            }
        }

        public async Task<OperationResult> ChangeStatusAsync(int ticketId, int newStatusId, int actorUserId, Role actorRole, CancellationToken ct = default)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Status)
                .Include(t => t.Technician)
                .FirstOrDefaultAsync(t => t.Id == ticketId, ct);
            if (ticket == null) return OperationResult.Fail("Zgłoszenie nie istnieje.");

            if (actorRole == Role.User)
                return OperationResult.Fail("Brak uprawnień do zmiany statusu zgłoszenia.");
            if (actorRole == Role.Technician && ticket.Technician?.Id != actorUserId)
                return OperationResult.Fail("Możesz zmieniać status tylko przydzielonych do Ciebie zgłoszeń.");

            var newStatus = await _db.Statuses.FirstOrDefaultAsync(s => s.Id == newStatusId, ct);
            if (newStatus == null) return OperationResult.Fail("Wybrany status nie istnieje.");
            if (ticket.Status?.Id == newStatus.Id) return OperationResult.Ok();

            var oldStatus = ticket.Status;
            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (newStatus.IsClosed)
            {
                if (ticket.ResolvedAt == null)
                    ticket.ResolvedAt = DateTime.UtcNow;
            }
            else
            {
                ticket.ResolvedAt = null;
            }

            var actor = await _db.Users.FirstOrDefaultAsync(u => u.Id == actorUserId, ct);
            _db.TicketHistories.Add(new TicketHistoryModel
            {
                Ticket = ticket,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedBy = actor,
                ChangedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> AssignTechnicianAsync(int ticketId, int? technicianId, int actorUserId, Role actorRole, CancellationToken ct = default)
        {
            if (actorRole != Role.Administrator && actorRole != Role.Technician)
                return OperationResult.Fail("Brak uprawnień do przydzielania technika.");

            var ticket = await _db.Tickets
                .Include(t => t.Technician)
                .FirstOrDefaultAsync(t => t.Id == ticketId, ct);
            if (ticket == null) return OperationResult.Fail("Zgłoszenie nie istnieje.");

            if (actorRole == Role.Technician)
            {
                if (!technicianId.HasValue || technicianId.Value != actorUserId)
                    return OperationResult.Fail("Technik może przydzielić wyłącznie samego siebie.");
                if (ticket.Technician != null && ticket.Technician.Id != actorUserId)
                    return OperationResult.Fail("Zgłoszenie jest już przydzielone do innego technika.");
            }

            if (technicianId.HasValue)
            {
                var tech = await _db.Users.FirstOrDefaultAsync(u => u.Id == technicianId.Value, ct);
                if (tech == null || tech.IsDeleted) return OperationResult.Fail("Wybrany technik nie istnieje.");
                if (tech.Role != Role.Technician && tech.Role != Role.Administrator)
                    return OperationResult.Fail("Wybrany użytkownik nie może być technikiem.");
                ticket.Technician = tech;
            }
            else
            {
                ticket.Technician = null;
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> ChangePriorityAsync(int ticketId, int priorityId, int actorUserId, Role actorRole, CancellationToken ct = default)
        {
            if (actorRole != Role.Administrator && actorRole != Role.Technician)
                return OperationResult.Fail("Brak uprawnień do zmiany priorytetu.");

            var ticket = await _db.Tickets
                .Include(t => t.Priority)
                .Include(t => t.Technician)
                .FirstOrDefaultAsync(t => t.Id == ticketId, ct);
            if (ticket == null) return OperationResult.Fail("Zgłoszenie nie istnieje.");

            if (actorRole == Role.Technician && ticket.Technician?.Id != actorUserId)
                return OperationResult.Fail("Możesz zmieniać priorytet tylko przydzielonych do Ciebie zgłoszeń.");

            var priority = await _db.Priorities.FirstOrDefaultAsync(p => p.Id == priorityId, ct);
            if (priority == null) return OperationResult.Fail("Wybrany priorytet nie istnieje.");

            ticket.Priority = priority;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<OperationResult> AddCommentAsync(int ticketId, string content, int authorUserId, Role authorRole, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(content))
                return OperationResult.Fail("Treść komentarza jest wymagana.");

            var ticket = await _db.Tickets
                .Include(t => t.Status)
                .Include(t => t.Reporter)
                .Include(t => t.Technician)
                .FirstOrDefaultAsync(t => t.Id == ticketId, ct);
            if (ticket == null) return OperationResult.Fail("Zgłoszenie nie istnieje.");

            if (ticket.Status != null && ticket.Status.IsClosed)
                return OperationResult.Fail("Nie można dodawać komentarzy do zamkniętego zgłoszenia.");

            var allowed = authorRole == Role.Administrator
                          || authorRole == Role.Technician
                          || ticket.Reporter?.Id == authorUserId;
            if (!allowed) return OperationResult.Fail("Brak uprawnień do dodawania komentarzy w tym zgłoszeniu.");

            var author = await _db.Users.FirstOrDefaultAsync(u => u.Id == authorUserId, ct);
            if (author == null) return OperationResult.Fail("Nie znaleziono autora komentarza.");

            _db.Comments.Add(new CommentModel
            {
                Content = content.Trim(),
                Ticket = ticket,
                User = author,
                CreatedAt = DateTime.UtcNow
            });

            ticket.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return OperationResult.Ok();
        }

        public async Task<List<TicketNotificationViewModel>> GetNewTicketsSinceAsync(DateTime sinceUtc, CancellationToken ct = default)
        {
            try
            {
                return await _db.Tickets.AsNoTracking()
                    .Where(t => t.CreatedAt > sinceUtc)
                    .OrderBy(t => t.CreatedAt)
                    .Take(50)
                    .ProjectToType<TicketNotificationViewModel>()
                    .ToListAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new List<TicketNotificationViewModel>();
            }
        }
    }
}
