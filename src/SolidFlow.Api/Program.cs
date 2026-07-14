using System.Text.Json.Serialization;
using SolidFlow.Api;
using SolidFlow.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<IProductCatalog, InMemoryProductCatalog>();
builder.Services.AddSingleton<IPricingPolicy, VolumePricingPolicy>();
builder.Services.AddSingleton<IPricingPolicy, PremiumCustomerPolicy>();
builder.Services.AddSingleton<IQuoteService, QuoteService>();

var app = builder.Build();

app.UseHttpsRedirection();

var api = app.MapGroup("/api/v1");

api.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "solidflow",
    timestamp = DateTimeOffset.UtcNow
}));

api.MapGet("/products", (IProductCatalog catalog) => Results.Ok(catalog.GetAll()));

api.MapPost("/quotes", (
    QuoteRequest request,
    IProductCatalog catalog,
    IQuoteService quoteService) =>
{
    if (request.Items is null || request.Items.Count == 0)
    {
        return Results.BadRequest(new { error = "At least one item is required." });
    }

    var items = new List<OrderItem>();

    foreach (var requestedItem in request.Items)
    {
        if (requestedItem.Quantity < 1)
        {
            return Results.BadRequest(new
            {
                error = $"Quantity for '{requestedItem.Sku}' must be greater than zero."
            });
        }

        var product = catalog.FindBySku(requestedItem.Sku);
        if (product is null)
        {
            return Results.BadRequest(new
            {
                error = $"Product '{requestedItem.Sku}' was not found."
            });
        }

        items.Add(new OrderItem(product, requestedItem.Quantity));
    }

    var context = new PricingContext(
        new CustomerProfile(request.CustomerId, request.CustomerTier),
        items,
        DateTimeOffset.UtcNow);

    return Results.Ok(quoteService.CreateQuote(context));
});

app.Run();
