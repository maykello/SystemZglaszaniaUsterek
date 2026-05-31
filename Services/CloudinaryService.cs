using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SystemZglaszaniaUsterek.Models.Options;

namespace SystemZglaszaniaUsterek.Services
{
    public record CloudinaryUploadResult(string PublicId, string Url);

    public interface ICloudinaryService
    {
        Task<CloudinaryUploadResult> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
        Task<bool> DeleteAsync(string publicId, CancellationToken ct = default);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinaryOptions _options;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinaryOptions> options, ILogger<CloudinaryService> logger)
        {
            _options = options.Value;
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_options.CloudName) ||
                string.IsNullOrWhiteSpace(_options.ApiKey) ||
                string.IsNullOrWhiteSpace(_options.ApiSecret))
            {
                _logger.LogWarning("Cloudinary credentials are not configured. File uploads will fail until configured via user-secrets or environment.");
            }

            var account = new Account(_options.CloudName, _options.ApiKey, _options.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<CloudinaryUploadResult> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            await using var stream = file.OpenReadStream();
            var fileDescription = new FileDescription(file.FileName, stream);

            var contentType = file.ContentType ?? string.Empty;
            var resourceType = contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase) ? "video" : "image";

            UploadResult result;
            if (resourceType == "video")
            {
                var uploadParams = new VideoUploadParams
                {
                    File = fileDescription,
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                result = await _cloudinary.UploadAsync(uploadParams, ct);
            }
            else
            {
                var uploadParams = new ImageUploadParams
                {
                    File = fileDescription,
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                result = await _cloudinary.UploadAsync(uploadParams, ct);
            }

            if (result.Error != null)
            {
                throw new InvalidOperationException($"Cloudinary upload failed: {result.Error.Message}");
            }

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Cloudinary upload returned status {result.StatusCode}.");
            }

            return new CloudinaryUploadResult(result.PublicId, result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? string.Empty);
        }

        public async Task<bool> DeleteAsync(string publicId, CancellationToken ct = default)
        {
            try
            {
                var deletionParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Auto
                };
                var result = await _cloudinary.DestroyAsync(deletionParams);
                return result.Result == "ok" || result.Result == "not found";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloudinary delete failed for {PublicId}", publicId);
                return false;
            }
        }
    }
}
