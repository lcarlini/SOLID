using SolidFlow.Core;

namespace SolidFlow.Api;

public sealed record QuoteRequest(
    Guid CustomerId,
    CustomerTier CustomerTier,
    IReadOnlyCollection<QuoteItemRequest> Items);

public sealed record QuoteItemRequest(string Sku, int Quantity);

public interface IProductCatalog
{
    IReadOnlyCollection<Product> GetAll();
    Product? FindBySku(string sku);
}

public sealed class InMemoryProductCatalog : IProductCatalog
{
    private static readonly Product[] Products =
    [
        new("ARCH-BOOK", "Pragmatic Architecture Field Guide", 49.00m),
        new("DDD-CARDS", "Domain Modeling Workshop Cards", 29.00m),
        new("REVIEW-KIT", "Engineering Review Toolkit", 79.00m)
    ];

    public IReadOnlyCollection<Product> GetAll() => Products;

    public Product? FindBySku(string sku) =>
        Products.FirstOrDefault(product =>
            product.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase));
}
