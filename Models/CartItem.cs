namespace StyleNest.Models;

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public string? CategoryName { get; set; }

    public decimal LineTotal => Price * Quantity;
}
