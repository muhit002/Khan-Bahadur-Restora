using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Users;

namespace RestaurantManagement.Api.Interfaces.Services;

public interface IUserService
{
    Task<PagedResult<UserSummaryDto>> GetUsersAsync(QueryParameters query, string? role);
    Task<UserDetailDto?> GetByIdAsync(Guid id);
    Task<UserDetailDto> CreateAsync(CreateUserDto request);
    Task<UserDetailDto> UpdateAsync(Guid id, UpdateUserDto request);
    Task DeleteAsync(Guid id);
}
