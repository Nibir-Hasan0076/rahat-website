namespace StyleNest.Models;

public static class StoreSettings
{
    public const string StoreName = "StyleNest";
    public const string CurrencySymbol = "৳";
    public const decimal ShippingFee = 60m;
    public const decimal FreeShippingThreshold = 2000m;

    public static readonly string[] PaymentMethods =
    {
        "Cash on Delivery", "bKash", "Card"
    };

    public static decimal ShippingFor(decimal subtotal) =>
        subtotal >= FreeShippingThreshold ? 0m : ShippingFee;

    public static string Money(decimal amount) =>
        $"{CurrencySymbol}{amount:N0}";
}
