namespace RestaurantManagement.Api.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveImageAsync(IFormFile file, CancellationToken cancellationToken = default);
}
