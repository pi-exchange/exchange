using Exchange.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Exchange.Api.Data;

public class ExchangeDbContext(DbContextOptions<ExchangeDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Trade> Trades => Set<Trade>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(254);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(16);
        });

        b.Entity<Asset>(e =>
        {
            e.HasIndex(x => x.Symbol).IsUnique();
            e.Property(x => x.Symbol).HasMaxLength(16);
            e.HasData(
                new Asset { Id = 1, Symbol = "BTC", Name = "Bitcoin", Decimals = 8 },
                new Asset { Id = 2, Symbol = "ETH", Name = "Ether", Decimals = 18 },
                new Asset { Id = 3, Symbol = "USDT", Name = "Tether USD", Decimals = 6 });
        });

        b.Entity<Balance>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.AssetId }).IsUnique();
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId);
            e.Property(x => x.Available).HasPrecision(38, 18);
            e.Property(x => x.Locked).HasPrecision(38, 18);
        });

        b.Entity<Order>(e =>
        {
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId);
            e.HasIndex(x => new { x.Market, x.Status, x.Side, x.Price });
            e.Property(x => x.Market).HasMaxLength(32);
            e.Property(x => x.Side).HasConversion<string>().HasMaxLength(8);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(8);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            e.Property(x => x.Price).HasPrecision(38, 18);
            e.Property(x => x.Quantity).HasPrecision(38, 18);
            e.Property(x => x.FilledQuantity).HasPrecision(38, 18);
        });

        b.Entity<Trade>(e =>
        {
            e.HasOne<Order>().WithMany().HasForeignKey(x => x.BuyOrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Order>().WithMany().HasForeignKey(x => x.SellOrderId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Market, x.ExecutedAt });
            e.Property(x => x.Market).HasMaxLength(32);
            e.Property(x => x.Price).HasPrecision(38, 18);
            e.Property(x => x.Quantity).HasPrecision(38, 18);
        });
    }
}
