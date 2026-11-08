using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models;
using StyleNest.Models.ViewModels;

namespace StyleNest.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // SQLite can't aggregate decimals in SQL, so totals are summed in memory.
        var orderTotals = await _db.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .Select(o => o.Total)
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            CategoryCount = await _db.Categories.CountAsync(),
            ProductCount = await _db.Products.CountAsync(),
            OrderCount = await _db.Orders.CountAsync(),
            PendingOrderCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
            Revenue = orderTotals.Sum(),
            RecentOrders = await _db.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(6)
                .ToListAsync(),
            LowStockProducts = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Stock <= 5)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync()
        };

        return View(vm);
    }
}
