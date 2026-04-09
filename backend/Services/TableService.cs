using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Common.Enums;
using RestaurantManagement.Api.Data;
using RestaurantManagement.Api.DTOs.Tables;
using RestaurantManagement.Api.Helpers;
using RestaurantManagement.Api.Interfaces.Services;

namespace RestaurantManagement.Api.Services;

public class TableService(ApplicationDbContext dbContext) : ITableService
{
    public async Task<IReadOnlyCollection<TableDto>> GetTablesAsync()
    {
        return await dbContext.Tables.AsNoTracking()
            .OrderBy(x => x.TableNumber)
            .Select(x => x.ToDto())
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<TableDto>> GetAvailableTablesAsync(int guests)
    {
        return await dbContext.Tables.AsNoTracking()
            .Where(x => x.Seats >= guests && x.Status == TableStatuses.Available)
            .OrderBy(x => x.TableNumber)
            .Select(x => x.ToDto())
            .ToListAsync();
    }

    public async Task<TableDto?> GetByIdAsync(Guid id)
    {
        var table = await dbContext.Tables.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return table?.ToDto();
    }

    public async Task<TableDto> CreateAsync(CreateTableDto request)
    {
        var duplicate = await dbContext.Tables.AnyAsync(x => x.TableNumber == request.TableNumber.Trim());
        if (duplicate)
        {
            throw new InvalidOperationException("A table with this number already exists.");
        }

        var table = new Entities.RestaurantTable
        {
            TableNumber = request.TableNumber.Trim(),
            Seats = request.Seats,
            Status = request.Status,
            Location = request.Location
        };

        await dbContext.Tables.AddAsync(table);
        await dbContext.SaveChangesAsync();
        return table.ToDto();
    }

    public async Task<TableDto> UpdateAsync(Guid id, UpdateTableDto request)
    {
        var table = await dbContext.Tables.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Table not found.");

        var duplicate = await dbContext.Tables.AnyAsync(x => x.Id != id && x.TableNumber == request.TableNumber.Trim());
        if (duplicate)
        {
            throw new InvalidOperationException("Another table already uses this number.");
        }

        table.TableNumber = request.TableNumber.Trim();
        table.Seats = request.Seats;
        table.Status = request.Status;
        table.Location = request.Location;

        await dbContext.SaveChangesAsync();
        return table.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var table = await dbContext.Tables.FirstOrDefaultAsync(x => x.Id == id) ??
            throw new KeyNotFoundException("Table not found.");

        dbContext.Tables.Remove(table);
        await dbContext.SaveChangesAsync();
    }
}
