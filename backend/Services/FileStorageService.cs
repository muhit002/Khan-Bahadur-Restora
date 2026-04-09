using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class FileStorageService(IWebHostEnvironment environment, IConfiguration configuration) : IFileStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<string> SaveImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException("The uploaded file is empty.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only jpg, jpeg, png, and webp images are allowed.");
        }

        var uploadsFolder = configuration["FileStorage:UploadsFolder"] ?? "uploads";
        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var absoluteUploadsPath = Path.Combine(webRoot, uploadsFolder);
        Directory.CreateDirectory(absoluteUploadsPath);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(absoluteUploadsPath, fileName);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/{uploadsFolder.Trim('/').Replace("\\", "/")}/{fileName}";
    }
}
