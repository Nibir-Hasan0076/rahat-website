using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StyleNest.Data;
using StyleNest.Models;

namespace StyleNest.ViewComponents;

public class CategoryNavViewComponent : ViewComponent
{
    private readonly AppDbContext _db;

    public CategoryNavViewComponent(AppDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.Id)
            .Select(c => new Category { Id = c.Id, Name = c.Name })
            .ToListAsync();

        return View(categories);
    }
}
