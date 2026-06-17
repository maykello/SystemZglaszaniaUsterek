using Microsoft.AspNetCore.Http;

namespace SystemZglaszaniaUsterek.Services
{
    public interface IAttachmentValidator
    {
        IReadOnlyList<string> Validate(IReadOnlyList<IFormFile>? files);
    }

    public class AttachmentValidator : IAttachmentValidator
    {
        public const int MaxFileCount = 5;
        public const long MaxFileSizeBytes = 10L * 1024L * 1024L; // 10 MB

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "video/mp4",
            "video/webm"
        };

        public IReadOnlyList<string> Validate(IReadOnlyList<IFormFile>? files)
        {
            var errors = new List<string>();

            if (files == null || files.Count == 0)
            {
                return errors;
            }

            if (files.Count > MaxFileCount)
            {
                errors.Add($"Maksymalna liczba załączników to {MaxFileCount}.");
            }

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    errors.Add($"Plik '{file?.FileName ?? "(brak nazwy)"}' jest pusty.");
                    continue;
                }

                if (file.Length > MaxFileSizeBytes)
                {
                    errors.Add($"Plik '{file.FileName}' przekracza maksymalny rozmiar 10 MB.");
                    continue;
                }

                if (!AllowedContentTypes.Contains(file.ContentType ?? string.Empty))
                {
                    errors.Add($"Plik '{file.FileName}' ma niedozwolony format ({file.ContentType}). Dozwolone: JPG, PNG, GIF, WebP, MP4, WebM.");
                    continue;
                }

                if (!HasValidMagicBytes(file))
                {
                    errors.Add($"Plik '{file.FileName}' ma uszkodzony nagłówek lub typ pliku nie zgadza się z deklarowanym.");
                }
            }

            return errors;
        }

        public static bool HasValidMagicBytes(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            return HasValidMagicBytes(stream, file.ContentType ?? string.Empty);
        }

        public static bool HasValidMagicBytes(Stream stream, string contentType)
        {
            var buffer = new byte[32];
            var origPosition = stream.CanSeek ? stream.Position : 0L;
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            int read = 0;
            int offset = 0;
            while (offset < buffer.Length)
            {
                int n = stream.Read(buffer, offset, buffer.Length - offset);
                if (n <= 0)
                {
                    break;
                }
                offset += n;
                read = offset;
            }

            if (stream.CanSeek)
            {
                stream.Position = origPosition;
            }

            if (read < 4)
            {
                return false;
            }

            return contentType.ToLowerInvariant() switch
            {
                "image/jpeg" => StartsWith(buffer, read, 0xFF, 0xD8, 0xFF),
                "image/png" => StartsWith(buffer, read, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
                "image/gif" => StartsWith(buffer, read, 0x47, 0x49, 0x46, 0x38) &&
                                read >= 6 && (buffer[4] == 0x37 || buffer[4] == 0x39) && buffer[5] == 0x61,
                "image/webp" => read >= 12 &&
                                buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
                                buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50,
                "video/mp4" => read >= 8 &&
                                buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70,
                "video/webm" => StartsWith(buffer, read, 0x1A, 0x45, 0xDF, 0xA3),
                _ => false
            };
        }

        private static bool StartsWith(byte[] buffer, int read, params byte[] expected)
        {
            if (read < expected.Length)
            {
                return false;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (buffer[i] != expected[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "plik";
            }
            var name = Path.GetFileName(fileName);
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var ch in invalid)
            {
                name = name.Replace(ch, '_');
            }

            name = name.Trim();
            if (name.Length > 200)
            {
                name = name.Substring(0, 200);
            }

            return string.IsNullOrEmpty(name) ? "plik" : name;
        }
    }
}
