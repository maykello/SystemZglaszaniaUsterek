using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;
using SystemZglaszaniaUsterek.Models.Options;
using SystemZglaszaniaUsterek.Models.ViewModels;
using Microsoft.Extensions.Options;

namespace SystemZglaszaniaUsterek.Services
{
    public class CreateTicketResult
    {
        public bool Success { get; init; }
        public int? TicketId { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    }

    public interface ITicketService
    {
        Task<CreateTicketResult> CreateAsync(TicketCreateViewModel model, int reporterUserId, CancellationToken ct = default);
        Task<List<CategoryModel>> GetCategoriesAsync(CancellationToken ct = default);
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
        {
            return _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
        }

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

                        var attachment = new AttachmentModel
                        {
                            TicketId = ticket.Id,
                            Ticket = ticket,
                            Url = uploadResult.Url,
                            PublicId = uploadResult.PublicId,
                            OriginalFileName = AttachmentValidator.SanitizeFileName(file.FileName),
                            ContentType = file.ContentType ?? "application/octet-stream",
                            SizeBytes = file.Length,
                            UploadedAt = DateTime.UtcNow
                        };

                        _db.Attachments.Add(attachment);
                    }
                }

                var history = new TicketHistoryModel
                {
                    Ticket = ticket,
                    OldStatus = null,
                    NewStatus = defaultStatus,
                    ChangedBy = reporter,
                    ChangedAt = DateTime.UtcNow
                };
                _db.TicketHistories.Add(history);

                await _db.SaveChangesAsync(ct);
                return new CreateTicketResult { Success = true, TicketId = ticket.Id };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to finalize ticket {TicketId}; rolling back uploads and ticket.", ticket.Id);

                foreach (var publicId in uploadedPublicIds)
                {
                    var ok = await _cloudinary.DeleteAsync(publicId, CancellationToken.None);
                    if (!ok)
                    {
                        _logger.LogWarning("Cloudinary cleanup failed for orphaned upload {PublicId}", publicId);
                    }
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
    }
}
