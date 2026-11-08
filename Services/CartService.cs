using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models;

namespace StyleNest.Services;

public class CartService
{
    private const string SessionKey = "StyleNest.Cart";
    private readonly IHttpContextAccessor _accessor;

    public CartService(IHttpContextAccessor accessor) => _accessor = accessor;

    private ISession Session => _accessor.HttpContext!.Session;

    public List<CartItem> GetItems()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
            return new List<CartItem>();

        try
        {
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }
        catch (JsonException)
        {
            return new List<CartItem>();
        }
    }

    public void Save(List<CartItem> items) =>
        Session.SetString(SessionKey, JsonSerializer.Serialize(items));

    public void Add(CartItem item, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing is null)
        {
            item.Quantity = Math.Clamp(quantity, 1, Math.Max(1, item.Stock));
            items.Add(item);
        }
        else
        {
            existing.Stock = item.Stock;
            existing.Price = item.Price;
            existing.Name = item.Name;
            existing.ImageUrl = item.ImageUrl;
            existing.Quantity = Math.Clamp(existing.Quantity + quantity, 1, Math.Max(1, item.Stock));
        }
        Save(items);
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var items = GetItems();
        var item = items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            return;

        if (quantity <= 0)
            items.Remove(item);
        else
            item.Quantity = Math.Clamp(quantity, 1, Math.Max(1, item.Stock));

        Save(items);
    }

    public void Remove(int productId)
    {
        var items = GetItems();
        items.RemoveAll(i => i.ProductId == productId);
        Save(items);
    }

    public void Clear() => Session.Remove(SessionKey);

    public int TotalQuantity => GetItems().Sum(i => i.Quantity);
    public decimal Subtotal => GetItems().Sum(i => i.LineTotal);

    /// <summary>
    /// Re-reads products from the database so cart names, prices and stock
    /// stay accurate even if an admin edited them after items were added.
    /// Returns the refreshed cart; unavailable products are dropped.
    /// </summary>
    public async Task<List<CartItem>> SyncWithCatalogAsync(AppDbContext db)
    {
        var items = GetItems();
        if (items.Count == 0)
            return items;

        var ids = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in items.ToList())
        {
            if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive || product.Stock <= 0)
            {
                items.Remove(item);
                continue;
            }

            item.Name = product.Name;
            item.Price = product.Price;
            item.ImageUrl = product.ImageUrl;
            item.Stock = product.Stock;
            item.CategoryName = product.Category?.Name;
            if (item.Quantity > product.Stock)
                item.Quantity = product.Stock;
        }

        Save(items);
        return items;
    }
}
