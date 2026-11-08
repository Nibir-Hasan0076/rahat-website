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
public class CategoriesController : Controller
{
    private readonly AppDbContext _db;
    private readonly ImageService _images;

    public CategoriesController(AppDbContext db, ImageService images)
    {
        _db = db;
        _images = images;
    }

    public async Task<IActionResult> Index()
    {
        var rows = await _db.Categories
            .OrderBy(c => c.Id)
            .Select(c => new CategoryListRow
            {
                Category = c,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        return View(rows);
    }

    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(CategoryFormViewModel vm)
    {
        var name = vm.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower()))
            ModelState.AddModelError(nameof(vm.Name), "A category with this name already exists.");

        if (!ModelState.IsValid)
            return View(vm);

        var (url, error) = await _images.SaveAsync(vm.ImageFile, "categories");
        if (error is not null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), error);
            return View(vm);
        }

        _db.Categories.Add(new Category
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
            ImageUrl = url,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Category \"{name}\" was added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return NotFound();

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ExistingImageUrl = category.ImageUrl
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoryFormViewModel vm)
    {
        var category = await _db.Categories.FindAsync(vm.Id);
        if (category is null)
            return NotFound();

        var name = vm.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.Id != vm.Id && c.Name.ToLower() == name.ToLower()))
            ModelState.AddModelError(nameof(vm.Name), "A category with this name already exists.");

        if (!ModelState.IsValid)
            return View(vm);

        var (url, error) = await _images.SaveAsync(vm.ImageFile, "categories");
        if (error is not null)
        {
            ModelState.AddModelError(nameof(vm.ImageFile), error);
            return View(vm);
        }

        if (url is not null)
        {
            _images.Delete(category.ImageUrl);
            category.ImageUrl = url;
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Category \"{name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return NotFound();

        if (await _db.Products.AnyAsync(p => p.CategoryId == id))
        {
            TempData["Error"] = $"\"{category.Name}\" still has products. Move or delete them first.";
            return RedirectToAction(nameof(Index));
        }

        _images.Delete(category.ImageUrl);
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Category \"{category.Name}\" was deleted.";
        return RedirectToAction(nameof(Index));
    }
}
