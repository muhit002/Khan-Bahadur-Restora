using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Api.DTOs.Tables;

public record TableDto(
    Guid Id,
    string TableNumber,
    int Seats,
    string Status,
    string? Location);

public class CreateTableDto
{
    [Required, StringLength(50)]
    public string TableNumber { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Seats { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Location { get; set; }
}

public class UpdateTableDto : CreateTableDto;
