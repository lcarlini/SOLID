namespace SolidFlow.Core;

public interface IPricingPolicy
{
    PriceAdjustment? Evaluate(PricingContext context, decimal currentTotal);
}

public interface IQuoteService
{
    PriceQuote CreateQuote(PricingContext context);
}

public sealed class QuoteService(IEnumerable<IPricingPolicy> policies) : IQuoteService
{
    private readonly IReadOnlyCollection<IPricingPolicy> _policies = policies.ToArray();

    public PriceQuote CreateQuote(PricingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Items.Count == 0)
        {
            throw new ArgumentException("A quote requires at least one item.", nameof(context));
        }

        var subtotal = context.Items.Sum(item => item.Subtotal);
        var currentTotal = subtotal;
        var adjustments = new List<PriceAdjustment>();

        foreach (var policy in _policies)
        {
            var adjustment = policy.Evaluate(context, currentTotal);
            if (adjustment is null || adjustment.Amount <= 0)
            {
                continue;
            }

            var appliedAmount = Math.Min(adjustment.Amount, currentTotal);
            adjustments.Add(adjustment with { Amount = appliedAmount });
            currentTotal -= appliedAmount;
        }

        return new PriceQuote(
            Subtotal: subtotal,
            TotalDiscount: subtotal - currentTotal,
            Total: currentTotal,
            Currency: "USD",
            Adjustments: adjustments);
    }
}

public sealed class PremiumCustomerPolicy(decimal discountRate = 0.10m) : IPricingPolicy
{
    public PriceAdjustment? Evaluate(PricingContext context, decimal currentTotal)
    {
        if (context.Customer.Tier is not CustomerTier.Premium)
        {
            return null;
        }

        return new PriceAdjustment(
            "PREMIUM",
            $"{discountRate:P0} premium customer discount",
            decimal.Round(currentTotal * discountRate, 2));
    }
}

public sealed class VolumePricingPolicy(
    int minimumQuantity = 10,
    decimal discountRate = 0.05m) : IPricingPolicy
{
    public PriceAdjustment? Evaluate(PricingContext context, decimal currentTotal)
    {
        var eligibleSubtotal = context.Items
            .Where(item => item.Quantity >= minimumQuantity)
            .Sum(item => item.Subtotal);

        if (eligibleSubtotal == 0)
        {
            return null;
        }

        return new PriceAdjustment(
            "VOLUME",
            $"{discountRate:P0} volume discount on eligible lines",
            decimal.Round(eligibleSubtotal * discountRate, 2));
    }
}
