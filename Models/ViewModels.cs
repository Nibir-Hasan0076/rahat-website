using System.ComponentModel.DataAnnotations;

namespace StyleNest.Models.ViewModels;

public class HomeViewModel
{
    public List<Category> Categories { get; set; } = new();
    public Dictionary<int, int> ProductCounts { get; set; } = new();
    public List<Product> FeaturedProducts { get; set; } = new();
    public int TotalProducts { get; set; }
}

public class ShopViewModel
{
    public List<Category> Categories { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public int? SelectedCategoryId { get; set; }
    public string? SelectedCategoryName { get; set; }
    public string? Query { get; set; }
}

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = null!;
    public List<Product> RelatedProducts { get; set; } = new();
    public List<string> Gallery { get; set; } = new();
}

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(100, MinimumLength = 3)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a contact number.")]
    [RegularExpression(@"^[0-9+\-\s()]{7,20}$", ErrorMessage = "Enter a valid phone number.")]
    [Display(Name = "Phone number")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(120)]
    [Display(Name = "Email (optional)")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please enter your delivery address.")]
    [StringLength(300, MinimumLength = 8)]
    [Display(Name = "Delivery address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your city.")]
    [StringLength(80)]
    public string City { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Area { get; set; }

    [StringLength(500)]
    [Display(Name = "Order notes (optional)")]
    public string? Notes { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your username.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }
}

public class CategoryFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(80, MinimumLength = 2)]
    [Display(Name = "Category name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Description { get; set; }

    public string? ExistingImageUrl { get; set; }

    [Display(Name = "Category image")]
    public IFormFile? ImageFile { get; set; }
}

public class ProductFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(120, MinimumLength = 2)]
    [Display(Name = "Product name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Range(1, 1_000_000, ErrorMessage = "Enter a price greater than zero.")]
    [Display(Name = "Price (৳)")]
    public decimal Price { get; set; }

    [Range(0, 100_000, ErrorMessage = "Stock must be zero or more.")]
    [Display(Name = "Stock quantity")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "Choose a category.")]
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Show in featured collection")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Visible in store")]
    public bool IsActive { get; set; } = true;

    public string? ExistingImageUrl { get; set; }

    [Display(Name = "Product images")]
    public List<IFormFile> ImageFiles { get; set; } = new();

    public List<ProductImage> ExistingImages { get; set; } = new();

    public List<Category> Categories { get; set; } = new();
}

public class CategoryListRow
{
    public Category Category { get; set; } = null!;
    public int ProductCount { get; set; }
}

public class DashboardViewModel
{
    public int CategoryCount { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public int PendingOrderCount { get; set; }
    public decimal Revenue { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public List<Product> LowStockProducts { get; set; } = new();
}

public class AdminOrdersViewModel
{
    public List<Order> Orders { get; set; } = new();
    public OrderStatus? Status { get; set; }
    public int PendingCount { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<OrderStatus, int> StatusCounts { get; set; } = new();
}
