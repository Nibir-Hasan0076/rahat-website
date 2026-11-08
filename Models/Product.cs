using System.ComponentModel.DataAnnotations;

namespace StyleNest.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(0, 1_000_000, ErrorMessage = "Price must be a positive amount.")]
    public decimal Price { get; set; }

    [Range(0, 100_000, ErrorMessage = "Stock must be zero or more.")]
    public int Stock { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public List<ProductImage> Images { get; set; } = new();

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
