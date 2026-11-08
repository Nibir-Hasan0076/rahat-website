using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models;
using StyleNest.Models.ViewModels;
using StyleNest.Services;

namespace StyleNest.Controllers;

public class CheckoutController : Controller
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;

    public CheckoutController(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _cart.SyncWithCatalogAsync(_db);
        if (items.Count == 0)
        {
            TempData["Error"] = "Your bag is empty. Add something you love first.";
            return RedirectToAction("Index", "Cart");
        }

        return View(BuildViewModel(new CheckoutViewModel(), items));
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel vm)
    {
        var items = await _cart.SyncWithCatalogAsync(_db);
        if (items.Count == 0)
        {
            TempData["Error"] = "Your bag is empty.";
            return RedirectToAction("Index", "Cart");
        }

        BuildViewModel(vm, items);

        if (!StoreSettings.PaymentMethods.Contains(vm.PaymentMethod))
            ModelState.AddModelError(nameof(vm.PaymentMethod), "Choose a valid payment method.");

        if (!ModelState.IsValid)
            return View(nameof(Index), vm);

        var ids = items.Select(i => i.ProductId).ToList();
        var products = await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var item in items)
        {
            if (!products.TryGetValue(item.ProductId, out var product) || !product.IsActive)
                ModelState.AddModelError(string.Empty, $"{item.Name} is no longer available. Please remove it from your bag.");
            else if (product.Stock < item.Quantity)
                ModelState.AddModelError(string.Empty, $"Only {product.Stock} pieces of {product.Name} are left in stock.");
        }

        if (!ModelState.IsValid)
            return View(nameof(Index), vm);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            CustomerName = vm.FullName.Trim(),
            Phone = vm.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
            Address = vm.Address.Trim(),
            City = vm.City.Trim(),
            Area = string.IsNullOrWhiteSpace(vm.Area) ? null : vm.Area.Trim(),
            Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim(),
            PaymentMethod = vm.PaymentMethod,
            Subtotal = vm.Subtotal,
            ShippingFee = vm.ShippingFee,
            Total = vm.Total,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in items)
        {
            var product = products[item.ProductId];
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price,
                Quantity = item.Quantity
            });
            product.Stock -= item.Quantity;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        _cart.Clear();
        TempData["OrderPlaced"] = "true";
        return RedirectToAction(nameof(Confirmation), new { orderNumber = order.OrderNumber });
    }

    public async Task<IActionResult> Confirmation(string orderNumber)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

        if (order is null)
            return NotFound();

        ViewData["JustPlaced"] = TempData["OrderPlaced"] != null;
        return View(order);
    }

    private static CheckoutViewModel BuildViewModel(CheckoutViewModel vm, List<CartItem> items)
    {
        vm.Items = items;
        vm.Subtotal = items.Sum(i => i.LineTotal);
        vm.ShippingFee = StoreSettings.ShippingFor(vm.Subtotal);
        vm.Total = vm.Subtotal + vm.ShippingFee;
        return vm;
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var suffix = RandomNumberGenerator.GetInt32(1000, 9999);
            var number = $"SN-{DateTime.Now:yyMMdd}-{suffix}";
            if (!await _db.Orders.AnyAsync(o => o.OrderNumber == number))
                return number;
        }

        return $"SN-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}
