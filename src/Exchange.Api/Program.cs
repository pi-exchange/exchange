using Exchange.Api.Data;
using Exchange.Api.Domain;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ExchangeDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

var markets = app.Configuration.GetSection("Markets").Get<MarketInfo[]>() ?? [];

// Миграции применяются при старте; БД может подниматься параллельно, поэтому несколько попыток.
await ApplyMigrationsAsync(app);

app.MapGet("/health", async (ExchangeDbContext db, CancellationToken ct) =>
    await db.Database.CanConnectAsync(ct)
        ? Results.Ok(new { status = "ok", database = "up" })
        : Results.Json(new { status = "degraded", database = "down" }, statusCode: StatusCodes.Status503ServiceUnavailable));

app.MapGet("/api/v1/markets", () => markets);

app.MapGet("/api/v1/markets/{baseAsset}/{quoteAsset}/orderbook", async (string baseAsset, string quoteAsset, ExchangeDbContext db, CancellationToken ct) =>
{
    var symbol = $"{baseAsset}/{quoteAsset}".ToUpperInvariant();
    if (!markets.Any(m => m.Symbol == symbol))
        return Results.NotFound(new { error = $"Unknown market '{symbol}'" });

    var open = db.Orders.Where(o => o.Market == symbol && o.Type == OrderType.Limit
        && (o.Status == OrderStatus.New || o.Status == OrderStatus.PartiallyFilled));

    // Агрегация по цене выполняется в БД; в BookLevel уровни переводятся уже в памяти.
    async Task<List<BookLevel>> LevelsAsync(OrderSide side, bool descending)
    {
        var levels = open.Where(o => o.Side == side)
            .GroupBy(o => o.Price!.Value)
            .Select(g => new { Price = g.Key, Quantity = g.Sum(o => o.Quantity - o.FilledQuantity) });
        var rows = await (descending ? levels.OrderByDescending(l => l.Price) : levels.OrderBy(l => l.Price))
            .ToListAsync(ct);
        return rows.Select(l => new BookLevel(l.Price, l.Quantity)).ToList();
    }

    return Results.Ok(new
    {
        market = symbol,
        bids = await LevelsAsync(OrderSide.Buy, descending: true),
        asks = await LevelsAsync(OrderSide.Sell, descending: false),
    });
});

app.Run();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ExchangeDbContext>().Database.MigrateAsync();
            return;
        }
        catch (Exception ex) when (attempt < 10)
        {
            app.Logger.LogWarning("Database is not ready (attempt {Attempt}/10): {Message}", attempt, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}

record MarketInfo(string Symbol, string BaseAsset, string QuoteAsset);

record BookLevel(decimal Price, decimal Quantity);
