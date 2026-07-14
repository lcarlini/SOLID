namespace SolidFlow.Core;

public enum CustomerTier
{
    Standard,
    Premium
}

public sealed record Product
{
    public Product(string sku, string name, decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);

        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        UnitPrice = unitPrice;
    }

    public string Sku { get; }
    public string Name { get; }
    public decimal UnitPrice { get; }
}

public sealed record OrderItem
{
    public OrderItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);

        Product = product;
        Quantity = quantity;
    }

    public Product Product { get; }
    public int Quantity { get; }
    public decimal Subtotal => Product.UnitPrice * Quantity;
}

public sealed record CustomerProfile(Guid Id, CustomerTier Tier);

public sealed record PricingContext(
    CustomerProfile Customer,
    IReadOnlyCollection<OrderItem> Items,
    DateTimeOffset RequestedAt);

public sealed record PriceAdjustment(string Code, string Description, decimal Amount);

public sealed record PriceQuote(
    decimal Subtotal,
    decimal TotalDiscount,
    decimal Total,
    string Currency,
    IReadOnlyCollection<PriceAdjustment> Adjustments);
