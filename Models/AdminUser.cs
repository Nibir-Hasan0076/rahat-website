using System.ComponentModel.DataAnnotations;

namespace StyleNest.Models;

public class AdminUser
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(80)]
    public string DisplayName { get; set; } = "Store Admin";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
