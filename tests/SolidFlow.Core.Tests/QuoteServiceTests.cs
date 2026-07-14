using SolidFlow.Core;

namespace SolidFlow.Core.Tests;

public sealed class QuoteServiceTests
{
    private static readonly Product Product = new("TEST-SKU", "Test product", 20m);

    [Fact]
    public void CreateQuote_WithoutEligiblePolicies_ReturnsSubtotal()
    {
        var service = new QuoteService(
        [
            new VolumePricingPolicy(),
            new PremiumCustomerPolicy()
        ]);

        var quote = service.CreateQuote(CreateContext(CustomerTier.Standard, quantity: 2));

        Assert.Equal(40m, quote.Subtotal);
        Assert.Equal(0m, quote.TotalDiscount);
        Assert.Equal(40m, quote.Total);
        Assert.Empty(quote.Adjustments);
    }

    [Fact]
    public void CreateQuote_ComposesPoliciesInRegistrationOrder()
    {
        var service = new QuoteService(
        [
            new VolumePricingPolicy(),
            new PremiumCustomerPolicy()
        ]);

        var quote = service.CreateQuote(CreateContext(CustomerTier.Premium, quantity: 10));

        Assert.Equal(200m, quote.Subtotal);
        Assert.Equal(29m, quote.TotalDiscount);
        Assert.Equal(171m, quote.Total);
        Assert.Collection(
            quote.Adjustments,
            adjustment => Assert.Equal("VOLUME", adjustment.Code),
            adjustment => Assert.Equal("PREMIUM", adjustment.Code));
    }

    [Fact]
    public void CreateQuote_WithNewPolicy_AppliesItWithoutChangingService()
    {
        var service = new QuoteService([new FixedDiscountPolicy(7m)]);

        var quote = service.CreateQuote(CreateContext(CustomerTier.Standard, quantity: 1));

        Assert.Equal(13m, quote.Total);
        Assert.Equal("FIXED", Assert.Single(quote.Adjustments).Code);
    }

    [Fact]
    public void CreateQuote_WhenPolicyExceedsTotal_ClampsAtZero()
    {
        var service = new QuoteService([new FixedDiscountPolicy(100m)]);

        var quote = service.CreateQuote(CreateContext(CustomerTier.Standard, quantity: 1));

        Assert.Equal(0m, quote.Total);
        Assert.Equal(20m, quote.TotalDiscount);
    }

    [Fact]
    public void CreateQuote_WithoutItems_Throws()
    {
        var service = new QuoteService([]);
        var context = new PricingContext(
            new CustomerProfile(Guid.NewGuid(), CustomerTier.Standard),
            [],
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() => service.CreateQuote(context));
    }

    private static PricingContext CreateContext(CustomerTier tier, int quantity) =>
        new(
            new CustomerProfile(Guid.NewGuid(), tier),
            [new OrderItem(Product, quantity)],
            DateTimeOffset.UtcNow);

    private sealed class FixedDiscountPolicy(decimal amount) : IPricingPolicy
    {
        public PriceAdjustment Evaluate(PricingContext context, decimal currentTotal) =>
            new("FIXED", "Fixed test discount", amount);
    }
}
