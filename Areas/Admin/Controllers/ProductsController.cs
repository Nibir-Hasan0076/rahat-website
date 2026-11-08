using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models;
using StyleNest.Models.ViewModels;
using StyleNest.Services;

namespace StyleNest.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private const int MaxImages = 12;

    private readonly AppDbContext _db;
    private readonly ImageService _images;

    public ProductsController(AppDbContext db, ImageService images)
    {
        _db = db;
        _images = images;
    }

    public async Task<IActionResult> Index(string? q, int? category)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (category.HasValue)
            query = query.Where(p => p.CategoryId == category.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        ViewData["Categories"] = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
        ViewData["Query"] = q;
        ViewData["CategoryId"] = category;

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ProductFormViewModel
        {
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync()
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        await ValidateCategoryAsync(vm);

        var newFiles = vm.ImageFiles.Where(f => f is { Length: > 0 }).ToList();
        if (newFiles.Count > MaxImages)
            ModelState.AddModelError(nameof(vm.ImageFiles), $"You can upload up to {MaxImages} images per product.");

        if (!ModelState.IsValid)
        {
            vm.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(vm);
        }

        var (urls, error) = await SaveImagesAsync(newFiles);
        if (error is not null)
        {
            ModelState.AddModelError(nameof(vm.ImageFiles), error);
            vm.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            return View(vm);
        }

        var product = new Product
        {
            Name = vm.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
            Price = vm.Price,
            Stock = vm.Stock,
            CategoryId = vm.CategoryId!.Value,
            IsFeatured = vm.IsFeatured,
            IsActive = vm.IsActive,
            ImageUrl = urls.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        };

        product.Images = urls.Skip(1)
            .Select((url, index) => new ProductImage { Url = url, SortOrder = index })
            .ToList();

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product \"{vm.Name.Trim()}\" was added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return NotFound();

        return View(new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            IsFeatured = product.IsFeatured,
            IsActive = product.IsActive,
            ExistingImageUrl = product.ImageUrl,
            ExistingImages = product.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList(),
            Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProductFormViewModel vm)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == vm.Id);
        if (product is null)
            return NotFound();

        await ValidateCategoryAsync(vm);

        var newFiles = vm.ImageFiles.Where(f => f is { Length: > 0 }).ToList();
        var currentCount = (product.ImageUrl is null ? 0 : 1) + product.Images.Count;
        if (currentCount + newFiles.Count > MaxImages)
            ModelState.AddModelError(nameof(vm.ImageFiles), $"A product can have up to {MaxImages} images.");

        if (!ModelState.IsValid)
        {
            vm.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            vm.ExistingImageUrl = product.ImageUrl;
            vm.ExistingImages = product.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList();
            return View(vm);
        }

        var (urls, error) = await SaveImagesAsync(newFiles);
        if (error is not null)
        {
            ModelState.AddModelError(nameof(vm.ImageFiles), error);
            vm.Categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            vm.ExistingImageUrl = product.ImageUrl;
            vm.ExistingImages = product.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).ToList();
            return View(vm);
        }

        if (product.ImageUrl is null && urls.Count > 0)
        {
            product.ImageUrl = urls[0];
            urls.RemoveAt(0);
        }

        var nextSort = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.SortOrder) + 1;
        foreach (var url in urls)
        {
            product.Images.Add(new ProductImage { Url = url, SortOrder = nextSort++ });
        }

        product.Name = vm.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        product.Price = vm.Price;
        product.Stock = vm.Stock;
        product.CategoryId = vm.CategoryId!.Value;
        product.IsFeatured = vm.IsFeatured;
        product.IsActive = vm.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product \"{product.Name}\" was updated.";
        return RedirectToAction(nameof(Edit), new { id = product.Id });
    }

    [HttpPost]
    public async Task<IActionResult> SetCover(int id, int imageId)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        var image = product?.Images.FirstOrDefault(i => i.Id == imageId);
        if (product is null || image is null)
            return NotFound();

        var oldCover = product.ImageUrl;
        product.ImageUrl = image.Url;

        product.Images.Remove(image);
        _db.ProductImages.Remove(image);

        if (!string.IsNullOrWhiteSpace(oldCover)
            && !string.Equals(oldCover, image.Url, StringComparison.OrdinalIgnoreCase)
            && product.Images.All(i => !string.Equals(i.Url, oldCover, StringComparison.OrdinalIgnoreCase)))
        {
            var nextSort = product.Images.Count == 0 ? 0 : product.Images.Max(i => i.SortOrder) + 1;
            _db.ProductImages.Add(new ProductImage
            {
                ProductId = product.Id,
                Url = oldCover,
                SortOrder = nextSort
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Cover image updated.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveImage(int id, int imageId)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        var image = product?.Images.FirstOrDefault(i => i.Id == imageId);
        if (product is null || image is null)
            return NotFound();

        _images.Delete(image.Url);
        product.Images.Remove(image);
        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Image removed.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
            return NotFound();

        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = product.IsActive
            ? $"\"{product.Name}\" is now visible in the store."
            : $"\"{product.Name}\" was hidden from the store.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
            return NotFound();

        foreach (var image in product.Images)
            _images.Delete(image.Url);
        _images.Delete(product.ImageUrl);

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Product \"{product.Name}\" was deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<(List<string> Urls, string? Error)> SaveImagesAsync(List<IFormFile> files)
    {
        var urls = new List<string>();
        foreach (var file in files)
        {
            var (url, error) = await _images.SaveAsync(file, "products");
            if (error is not null)
            {
                foreach (var saved in urls)
                    _images.Delete(saved);
                return (new List<string>(), error);
            }

            if (url is not null)
                urls.Add(url);
        }

        return (urls, null);
    }

    private async Task ValidateCategoryAsync(ProductFormViewModel vm)
    {
        if (vm.CategoryId is null || !await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId))
            ModelState.AddModelError(nameof(vm.CategoryId), "Choose a valid category.");
    }
}
