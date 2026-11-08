using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StyleNest.Models;

namespace StyleNest.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<AdminUser> hasher, IConfiguration config)
    {
        await SeedAdminAsync(db, hasher, config);
        await SeedCatalogAsync(db);
    }

    private static async Task SeedAdminAsync(AppDbContext db, IPasswordHasher<AdminUser> hasher, IConfiguration config)
    {
        var username = config["AdminSeed:Username"] ?? "admin";
        var password = config["AdminSeed:Password"] ?? "Admin@123";
        var displayName = config["AdminSeed:DisplayName"] ?? "Store Admin";

        if (await db.AdminUsers.AnyAsync(u => u.Username == username))
            return;

        var admin = new AdminUser
        {
            Username = username,
            DisplayName = displayName
        };
        admin.PasswordHash = hasher.HashPassword(admin, password);
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();
    }

    private static async Task SeedCatalogAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync())
            return;

        var panjabi = new Category
        {
            Name = "Panjabi",
            Description = "Timeless festive and everyday panjabis cut from breathable cotton and silk blends.",
            ImageUrl = "/images/categories/panjabi.svg"
        };
        var polos = new Category
        {
            Name = "Polo Shirts",
            Description = "Smart-casual pique polos with a clean collar and an easy, comfortable fit.",
            ImageUrl = "/images/categories/polo.svg"
        };
        var shirts = new Category
        {
            Name = "Full Shirts",
            Description = "Everyday and occasion shirts in oxford, linen and poplin weaves.",
            ImageUrl = "/images/categories/shirt.svg"
        };
        var pants = new Category
        {
            Name = "Pants",
            Description = "Tailored chinos, denim and relaxed trousers built for all-day wear.",
            ImageUrl = "/images/categories/pants.svg"
        };

        db.Categories.AddRange(panjabi, polos, shirts, pants);
        await db.SaveChangesAsync();

        db.Products.AddRange(
            new Product
            {
                Name = "Heritage Cotton Panjabi",
                Description = "A clean-lined panjabi in soft ivory cotton. Features a mandarin collar, covered button placket and side vents for ease of movement. Perfect for Eid, dawats and Friday prayers.",
                Price = 2190, Stock = 18, IsFeatured = true, IsActive = true,
                CategoryId = panjabi.Id, ImageUrl = "/images/products/panjabi-ivory.svg"
            },
            new Product
            {
                Name = "Midnight Silk-Blend Panjabi",
                Description = "Deep navy panjabi with a subtle sheen, tonal embroidery at the collar and a relaxed straight fit. A refined pick for evening occasions.",
                Price = 3290, Stock = 9, IsActive = true,
                CategoryId = panjabi.Id, ImageUrl = "/images/products/panjabi-navy.svg"
            },
            new Product
            {
                Name = "Classic Pique Polo",
                Description = "Our everyday polo in heavyweight cotton pique. Ribbed collar, two-button placket and a fit that holds its shape wash after wash.",
                Price = 1190, Stock = 32, IsFeatured = true, IsActive = true,
                CategoryId = polos.Id, ImageUrl = "/images/products/polo-navy.svg"
            },
            new Product
            {
                Name = "Striped Pique Polo",
                Description = "A softly striped polo in sage and cream. Breathable pique knit with a relaxed fit and side splits.",
                Price = 1290, Stock = 24, IsActive = true,
                CategoryId = polos.Id, ImageUrl = "/images/products/polo-sage.svg"
            },
            new Product
            {
                Name = "Oxford Full-Sleeve Shirt",
                Description = "A wardrobe staple: mid-weight oxford cotton, button-down collar and a slightly tapered body. Wear it tucked or loose.",
                Price = 1690, Stock = 28, IsActive = true,
                CategoryId = shirts.Id, ImageUrl = "/images/products/shirt-oxford.svg"
            },
            new Product
            {
                Name = "Linen Casual Shirt",
                Description = "Airy linen-blend shirt in warm sand. Garment-washed for softness with a relaxed camp collar and coconut buttons.",
                Price = 1790, Stock = 16, IsActive = true,
                CategoryId = shirts.Id, ImageUrl = "/images/products/shirt-linen.svg"
            },
            new Product
            {
                Name = "Tailored Chino Pants",
                Description = "Slim-straight chinos in stretch cotton twill. Finished with a clean waistband and a touch of stretch for comfort.",
                Price = 1590, Stock = 22, IsActive = true,
                CategoryId = pants.Id, ImageUrl = "/images/products/pants-chino.svg"
            },
            new Product
            {
                Name = "Relaxed Denim Pants",
                Description = "Mid-rise relaxed denim in a stone wash. Sturdy 12oz cotton denim with a straight leg and classic five-pocket styling.",
                Price = 1890, Stock = 20, IsFeatured = true, IsActive = true,
                CategoryId = pants.Id, ImageUrl = "/images/products/pants-denim.svg"
            }
        );

        await db.SaveChangesAsync();
    }
}
