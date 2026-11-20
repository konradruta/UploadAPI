using Microsoft.AspNetCore.Mvc;

namespace UploadAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        //private readonly string _isolatedStorage = "/var/app/storage_outside_webroot";
        private readonly string _isolatedStorage = OperatingSystem.IsWindows()
    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "storage_outside_webroot")
    : "/var/app/storage_outside_webroot";

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;

            // Upewniamy się, że katalogi istnieją
            var webRootStorage = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
            Directory.CreateDirectory(webRootStorage);
            Directory.CreateDirectory(_isolatedStorage);
        }

        // ============================================================================
        // 1. ENDPOINT PODATNY (BEFORE) - ASP.NET Core Controller
        // Podatności: brak walidacji typu, zapis w web-root, zachowanie oryginalnej nazwy
        // ============================================================================
        [HttpPost("upload-vulnerable")]
        public async Task<IActionResult> UploadVulnerable(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Nie przesłano pliku." });

            var webRootStorage = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
            var filePath = Path.Combine(webRootStorage, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new
            {
                status = "VULNERABLE_UPLOAD_SUCCESS",
                originalName = file.FileName,
                publicUrl = $"/uploads/{file.FileName}"
            });
        }

        // ============================================================================
        // 2. ENDPOINT BEZPIECZNY (AFTER) - ASP.NET Core Controller
        // Zabezpieczenia: Weryfikacja Magic Bytes, nazwa UUID, zapis poza web-root
        // ============================================================================
        [HttpPost("upload-secure")]
        public async Task<IActionResult> UploadSecure(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Brak pliku w żądaniu." });

            // KROK 1: Biała lista rozszerzeń
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                return BadRequest(new { error = "Niedozwolone rozszerzenie pliku." });
            }

            // KROK 2: Weryfikacja nagłówków Magic Bytes (sygnatury binarnej)
            using (var stream = file.OpenReadStream())
            {
                if (!IsValidMagicBytes(stream, extension))
                {
                    return BadRequest(new { error = "Weryfikacja binarnej sygnatury pliku (Magic Bytes) nie powiodła się." });
                }
            }

            // KROK 3: Bezpieczna nazwa (UUID) & usuwanie ataku Path Traversal
            var safeFileName = $"{Guid.NewGuid()}{extension}";

            // KROK 4: Zapis w izolowanym katalogu poza wwwroot
            var destinationPath = Path.Combine(_isolatedStorage, safeFileName);

            using (var fileStream = new FileStream(destinationPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(new
            {
                status = "SECURE_UPLOAD_SUCCESS",
                fileId = safeFileName,
                message = "Plik został pomyślnie zweryfikowany i bezpiecznie zapisany poza web-root."
            });
        }

        // Funkcja pomocnicza: Sprawdzanie bajtów nagłówkowych Magic Bytes
        private static bool IsValidMagicBytes(Stream stream, string extension)
        {
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, buffer.Length);
            stream.Position = 0; // Reset pozycji strumienia

            return extension switch
            {
                // PNG: 89 50 4E 47 0D 0A 1A 0A
                ".png" => buffer.Take(4).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
                // JPEG: FF D8 FF
                ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
                // PDF: %PDF (25 50 44 46)
                ".pdf" => buffer.Take(4).SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }),
                _ => false
            };
        }
    }
}
