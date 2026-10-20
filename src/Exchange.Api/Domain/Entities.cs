namespace Exchange.Api.Domain;

public enum UserRole { Trader = 0, Administrator = 1 }

public enum OrderSide { Buy = 0, Sell = 1 }

public enum OrderType { Limit = 0, Market = 1 }

public enum OrderStatus { New = 0, PartiallyFilled = 1, Filled = 2, Cancelled = 3 }

public class User
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Trader;
    public DateTime CreatedAt { get; set; }
}

public class Asset
{
    public int Id { get; set; }
    public required string Symbol { get; set; }
    public required string Name { get; set; }
    public int Decimals { get; set; }
}

/// <summary>Баланс пользователя по активу: свободные и заблокированные под ордера средства.</summary>
public class Balance
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public int AssetId { get; set; }
    public decimal Available { get; set; }
    public decimal Locked { get; set; }
}

public class Order
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    /// <summary>Символ рынка в виде BASE/QUOTE, например BTC/USDT.</summary>
    public required string Market { get; set; }
    public OrderSide Side { get; set; }
    public OrderType Type { get; set; }
    /// <summary>Цена; null для рыночных ордеров.</summary>
    public decimal? Price { get; set; }
    public decimal Quantity { get; set; }
    public decimal FilledQuantity { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public DateTime CreatedAt { get; set; }
}

public class Trade
{
    public long Id { get; set; }
    public required string Market { get; set; }
    public long BuyOrderId { get; set; }
    public long SellOrderId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public DateTime ExecutedAt { get; set; }
}
