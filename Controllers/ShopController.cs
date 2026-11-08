using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models.ViewModels;

namespace StyleNest.Controllers;

public class ShopController : Controller
{
    private readonly AppDbContext _db;

    public ShopController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int? category, string? q)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        string? selectedName = null;
        if (category.HasValue)
        {
            query = query.Where(p => p.CategoryId == category.Value);
            selectedName = await _db.Categories
                .Where(c => c.Id == category.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                p.Name.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)) ||
                (p.Category != null && p.Category.Name.Contains(term)));
        }

        var products = await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(new ShopViewModel
        {
            Categories = await _db.Categories.OrderBy(c => c.Id).ToListAsync(),
            Products = products,
            SelectedCategoryId = category,
            SelectedCategoryName = selectedName,
            Query = q?.Trim()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product is null)
            return NotFound();

        var related = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != product.Id)
            .OrderByDescending(p => p.IsFeatured)
            .Take(4)
            .ToListAsync();

        var gallery = new List<string>();
        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
            gallery.Add(product.ImageUrl);
        gallery.AddRange(product.Images
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .Select(i => i.Url)
            .Where(url => !gallery.Contains(url, StringComparer.OrdinalIgnoreCase)));
        if (gallery.Count == 0)
            gallery.Add("/images/placeholder-product.svg");

        return View(new ProductDetailsViewModel
        {
            Product = product,
            RelatedProducts = related,
            Gallery = gallery
        });
    }
}
