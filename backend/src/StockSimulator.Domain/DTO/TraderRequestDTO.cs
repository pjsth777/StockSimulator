using System.ComponentModel.DataAnnotations;

namespace StockSimulator.Domain.DTO;

public class TraderRequestDTO
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Required]
    public bool IsBuy { get; set; }
}
