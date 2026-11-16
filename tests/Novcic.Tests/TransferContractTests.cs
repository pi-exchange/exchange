using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Novcic.Core;
using Npgsql;

namespace Novcic.Tests;

// Real module/database integration; HTTP routing and authentication are outside this fixture.
[Collection("database")]
public class TransferContractTests : IAsyncLifetime
{
    private readonly Guid user = Guid.NewGuid();
    private readonly string connection = Environment.GetEnvironmentVariable("TEST_DB")
        ?? throw new InvalidOperationException("Set TEST_DB to the isolated pi_exchange_lab4_tests database.");

    private ExchangeDb Db() => new(new DbContextOptionsBuilder<ExchangeDb>().UseNpgsql(connection).Options);

    public async Task InitializeAsync()
    {
        if (new NpgsqlConnectionStringBuilder(connection).Database != "pi_exchange_lab4_tests")
            throw new InvalidOperationException("Lab 4 only resets the disposable pi_exchange_lab4_tests database.");

        await using var db = Db();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        // Arrange the contract's starting state directly, without a faucet or external services.
        foreach (var kind in TradingService.Kinds)
        foreach (var asset in TradingService.Assets)
            db.Wallets.Add(new Wallet
            {
                UserId = user, Kind = kind, Asset = asset,
                Available = asset == "USDT" ? (kind == "Funding" ? 500m : 100m) : 0m
            });
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<string> Transfer(decimal amount, string key)
    {
        await using var db = Db();
        var service = new TradingService(db);
        var request = new TransferRequest("USDT", "Funding", "Unified Trading", amount);
        return await service.Command(user, key, "transfer", request, () => service.Transfer(user, request));
    }

    private async Task AssertBalances(decimal funding, decimal trading)
    {
        // A new context sees committed state, not tracked changes from the command.
        await using var db = Db();
        var wallets = await db.Wallets.AsNoTracking().Where(w => w.UserId == user).ToListAsync();
        Assert.Equal(4, wallets.Count);
        Assert.Equal(funding, Assert.Single(wallets, w => w.Kind == "Funding" && w.Asset == "USDT").Available);
        Assert.Equal(trading, Assert.Single(wallets, w => w.Kind == "Unified Trading" && w.Asset == "USDT").Available);
        Assert.All(wallets, w => Assert.Equal(0m, w.Locked));
        Assert.All(wallets.Where(w => w.Asset == "BTC"), w => Assert.Equal(0m, w.Available));
    }

    [Fact]
    public async Task Transfer25CommitsBalancesAndAccounting()
    {
        var key = "11111111-1111-4111-8111-111111111104";
        var response = await Transfer(25.00000000m, key);

        using var json = JsonDocument.Parse(response);
        Assert.Equal("Transfer completed.", json.RootElement.GetProperty("message").GetString());
        var reference = json.RootElement.GetProperty("reference").GetGuid();
        Assert.NotEqual(Guid.Empty, reference);
        // Deliberately wrong for the lab's first CI run; restore 475 in a separate commit.
        await AssertBalances(474m, 125m);

        await using var db = Db();
        var entries = await db.Ledger.OrderBy(e => e.AvailableDelta).ToListAsync();
        Assert.Collection(entries,
            entry => { Assert.Equal("Funding", entry.Wallet); Assert.Equal(-25m, entry.AvailableDelta); },
            entry => { Assert.Equal("Unified Trading", entry.Wallet); Assert.Equal(25m, entry.AvailableDelta); });
        Assert.All(entries, entry =>
        {
            Assert.Equal(user, entry.UserId);
            Assert.Equal("USDT", entry.Asset);
            Assert.Equal(reference, entry.ReferenceId);
            Assert.Equal(0m, entry.LockedDelta);
        });
        var journal = await db.Journal.ToListAsync();
        Assert.Equal(4, journal.Count);
        Assert.All(journal, line => { Assert.Equal(reference, line.ReferenceId); Assert.Equal("USDT", line.Asset); });
        Assert.Equal(0m, journal.Sum(line => line.Delta));
        var receipt = Assert.Single(await db.Receipts.ToListAsync());
        Assert.Equal(user, receipt.UserId);
        Assert.Equal(key, receipt.Key);
        Assert.Equal(response, receipt.Response);
    }

    [Fact]
    public async Task Transfer600RejectsWithoutPersistingChanges()
    {
        var error = await Assert.ThrowsAsync<RuleException>(() =>
            Transfer(600m, "22222222-2222-4222-8222-222222222204"));

        Assert.Equal("Insufficient available USDT in Funding.", error.Message);
        await AssertBalances(500m, 100m);
        await using var db = Db();
        Assert.Empty(await db.Ledger.ToListAsync());
        Assert.Empty(await db.Journal.ToListAsync());
        Assert.Empty(await db.Receipts.ToListAsync());
    }

    [Fact]
    public async Task RepeatedTransferReturnsReceiptWithoutAnotherDebit()
    {
        var key = Guid.NewGuid().ToString();
        var first = await Transfer(25m, key);
        var repeated = await Transfer(25m, key);

        Assert.Equal(first, repeated);
        await AssertBalances(475m, 125m);
        await using var db = Db();
        Assert.Equal(2, await db.Ledger.CountAsync());
        Assert.Equal(4, await db.Journal.CountAsync());
        var receipt = Assert.Single(await db.Receipts.ToListAsync());
        Assert.Equal(key, receipt.Key);
        Assert.Equal(first, receipt.Response);
    }
}
