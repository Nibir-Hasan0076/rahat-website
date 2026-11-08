using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models;
using StyleNest.Services;

namespace StyleNest.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;

    public CartController(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _cart.SyncWithCatalogAsync(_db);
        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

        if (product is null)
        {
            TempData["Error"] = "That product is no longer available.";
            return RedirectToAction("Index", "Shop");
        }

        if (product.Stock <= 0)
        {
            TempData["Error"] = $"{product.Name} is out of stock right now.";
            return RedirectToAction(nameof(Index));
        }

        if (quantity < 1) quantity = 1;

        _cart.Add(new CartItem
        {
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            Stock = product.Stock,
            CategoryName = product.Category?.Name
        }, quantity);

        TempData["Success"] = $"{product.Name} was added to your bag.";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product is not null && quantity > product.Stock)
        {
            quantity = product.Stock;
            TempData["Error"] = $"Only {product.Stock} pieces of {product.Name} are available.";
        }

        _cart.UpdateQuantity(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remove(int productId)
    {
        _cart.Remove(productId);
        TempData["Success"] = "Item removed from your bag.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Clear()
    {
        _cart.Clear();
        return RedirectToAction(nameof(Index));
    }
}
