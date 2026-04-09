using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Common;
using RestaurantManagement.Api.DTOs.Users;
using RestaurantManagement.Api.Entities;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class UserService(ApplicationDbContext dbContext, IPasswordService passwordService) : IUserService
{
    public async Task<PagedResult<UserSummaryDto>> GetUsersAsync(QueryParameters query, string? role)
    {
        var usersQuery = dbContext.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            usersQuery = usersQuery.Where(x => x.Role == role);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            usersQuery = usersQuery.Where(x =>
                x.FullName.ToLower().Contains(search) ||
                x.Email.ToLower().Contains(search));
        }

        return await usersQuery
            .OrderBy(x => x.FullName)
            .Select(x => x.ToSummaryDto())
            .ToPagedResultAsync(query.PageNumber, query.PageSize);
    }

    public async Task<UserDetailDto?> GetByIdAsync(Guid id)
    {
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return user?.ToDetailDto();
    }

    public async Task<UserDetailDto> CreateAsync(CreateUserDto request)
    {
        ValidateRole(request.Role);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(x => x.Email == normalizedEmail);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var (hash, salt) = passwordService.HashPassword(request.Password);
        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = request.Role,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            IsActive = true
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        return user.ToDetailDto();
    }

    public async Task<UserDetailDto> UpdateAsync(Guid id, UpdateUserDto request)
    {
        ValidateRole(request.Role);

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("User not found.");

        user.FullName = request.FullName.Trim();
        user.Role = request.Role;
        user.PhoneNumber = request.PhoneNumber;
        user.Address = request.Address;
        user.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var (hash, salt) = passwordService.HashPassword(request.Password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        await dbContext.SaveChangesAsync();
        return user.ToDetailDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("User not found.");

        var isReferenced = await dbContext.Orders.AnyAsync(x =>
            x.CustomerId == id || x.WaiterId == id);

        if (isReferenced)
        {
            throw new InvalidOperationException("This user is already linked to orders. Deactivate the account instead of deleting it.");
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
    }

    private static void ValidateRole(string role)
    {
        if (!AppRoles.All.Contains(role))
        {
            throw new InvalidOperationException("Invalid role value.");
        }
    }
}
