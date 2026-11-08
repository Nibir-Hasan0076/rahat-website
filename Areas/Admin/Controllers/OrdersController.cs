using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models;
using StyleNest.Models.ViewModels;

namespace StyleNest.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly AppDbContext _db;

    public OrdersController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(OrderStatus? status)
    {
        var query = _db.Orders.AsQueryable();
        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var statusCounts = await _db.Orders
            .GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

        var vm = new AdminOrdersViewModel
        {
            Status = status,
            Orders = await query
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(),
            PendingCount = statusCounts.GetValueOrDefault(OrderStatus.Pending),
            TotalCount = await _db.Orders.CountAsync(),
            StatusCounts = statusCounts
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return NotFound();

        if (order.Status == status)
            return RedirectToAction(nameof(Details), new { id });

        await using var transaction = await _db.Database.BeginTransactionAsync();

        if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            // Cancelling returns reserved stock to the catalogue.
            foreach (var item in order.Items.Where(i => i.ProductId.HasValue))
            {
                var product = await _db.Products.FindAsync(item.ProductId!.Value);
                if (product is not null)
                    product.Stock += item.Quantity;
            }
        }
        else if (order.Status == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
        {
            foreach (var item in order.Items.Where(i => i.ProductId.HasValue))
            {
                var product = await _db.Products.FindAsync(item.ProductId!.Value);
                if (product is not null)
                    product.Stock = Math.Max(0, product.Stock - item.Quantity);
            }
        }

        order.Status = status;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["Success"] = $"Order {order.OrderNumber} is now {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
