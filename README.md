# StyleNest

An e-commerce store built with ASP.NET Core MVC (.NET 10) and EF Core + SQLite.

## Run it

```bash
cd StyleNest
dotnet restore
dotnet run --urls http://localhost:5199
```

Open http://localhost:5199.

The SQLite database (`stylenest.db`) is created and seeded automatically on first start.
Delete that file to reset the store to its seeded state.

## Admin

- URL: http://localhost:5199/Admin/Account/Login
- Username: `admin`
- Password: `Admin@123`

Change these in `appsettings.json` under `AdminSeed` before using the store for real.

## What it does

Storefront
- Browse the catalogue, filter by category, and search products.
- Product detail pages with stock status, quantity selector, and related items.
- Session-based shopping bag: add, change quantity, remove, clear.
- Checkout with a validated delivery form (name, phone, email, address, city, area, notes)
  and payment preference (cash on delivery, bKash, card).
- Order confirmation with an order number and printable receipt.
- Delivery is ৳60, free on orders over ৳2,000.

Admin (`/Admin`)
- Dashboard with revenue, order/product/category counts, recent orders, and low-stock alerts.
- Categories: create, edit, delete, and upload a category image. A category with products
  cannot be deleted.
- Products: create, edit, show/hide, delete, upload multiple images (up to 12) with a
  choosable cover photo, set price, stock, and featured status.
- Orders: filter by status, view order details, and move orders through
  Pending → Confirmed → Shipped → Delivered (or Cancelled). Cancelling restores stock.

## Project layout

```
Controllers/            Storefront controllers (Home, Shop, Cart, Checkout)
Areas/Admin/            Admin controllers, views, and layout
Models/                 Entities and view models
Data/                   AppDbContext and database seeding
Services/               Cart, image upload, and admin authentication services
ViewComponents/         Dynamic category navigation
Views/                  Storefront Razor views
wwwroot/                CSS, JS, generated SVG artwork, and uploaded images
tools/generate-art.cs   .NET file-based generator for the sample product artwork
```

Uploaded images are stored under `wwwroot/uploads/categories` and
`wwwroot/uploads/products`. Allowed formats: JPG, PNG, WEBP, GIF (max 5 MB).

To regenerate the sample SVG artwork:

```bash
dotnet run --file tools/generate-art.cs
```

## Notes

- Each running instance uses the `stylenest.db` file next to the project. Run a single
  instance at a time; starting a second copy creates a separate, empty database and its
  orders will not appear in the first one.
- SQLite cannot aggregate `decimal` in queries, so dashboard revenue is summed in memory.
- Every POST is antiforgery-validated. Admin pages require the `Admin` role.
- The login page shows the demo credentials as a hint; remove that block before going live.
