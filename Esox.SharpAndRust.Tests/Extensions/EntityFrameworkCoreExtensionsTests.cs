using System.ComponentModel.DataAnnotations;
using Esox.SharpAndRusty.EntityFrameworkCore.Extensions;
using Esox.SharpAndRusty.EntityFrameworkCore.Types;
using Esox.SharpAndRusty.Types;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRusty.Tests.Extensions;

public class EntityFrameworkCoreExtensionsTests
{
    [Fact]
    public async Task FirstOrNoneAsync_WhenMissing_ReturnsNone()
    {
        await using var context = CreateContext();

        var result = await context.Users.Where(x => x.Id == 999).FirstOrNoneAsync();

        Assert.IsType<Option<TestUser>.None>(result);
    }

    [Fact]
    public async Task FirstOrNoneAsync_WhenFound_ReturnsSome()
    {
        await using var context = CreateContext();

        var result = await context.Users.Where(x => x.Email == "alice@example.com").FirstOrNoneAsync();

        var some = Assert.IsType<Option<TestUser>.Some>(result);
        Assert.Equal("alice@example.com", some.Value.Email);
    }

    [Fact]
    public async Task SingleOrNoneAsync_WhenMissing_ReturnsNone()
    {
        await using var context = CreateContext();

        var result = await context.Users.Where(x => x.Email == "missing@example.com").SingleOrNoneAsync();

        Assert.IsType<Option<TestUser>.None>(result);
    }

    [Fact]
    public async Task SingleOrNoneAsync_WhenFound_ReturnsSome()
    {
        await using var context = CreateContext();

        var result = await context.Users.Where(x => x.Id == 1).SingleOrNoneAsync();

        var some = Assert.IsType<Option<TestUser>.Some>(result);
        Assert.Equal(1, some.Value.Id);
    }

    [Fact]
    public async Task ExecuteSafeAsync_WhenSuccessful_ReturnsOk()
    {
        await using var context = CreateContext();

        var result = await context.ExecuteSafeAsync(async (ctx, ct) =>
        {
            await Task.Delay(1, ct);
            return await ctx.Set<TestUser>().CountAsync(ct);
        });

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var count));
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ExecuteSafeAsync_WhenConstraintLikeUpdateError_ReturnsConstraintViolation()
    {
        await using var context = CreateContext();

        var result = await context.ExecuteSafeAsync<int>((_, _) =>
            throw new DbUpdateException("UNIQUE constraint failed: Users.Email", new Exception("duplicate")));

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(DbErrorKind.ConstraintViolation, error.Kind);
        Assert.Contains("UNIQUE", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteSafeAsync_WhenConcurrencyException_ReturnsConcurrencyConflict()
    {
        await using var context = CreateContext();

        var result = await context.ExecuteSafeAsync<int>((_, _) =>
            throw new DbUpdateConcurrencyException("row version mismatch"));

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(DbErrorKind.ConcurrencyConflict, error.Kind);
    }

    [Fact]
    public async Task SaveChangesSafeAsync_WhenSuccessful_ReturnsAffectedRows()
    {
        await using var context = CreateContext();
        context.Users.Add(new TestUser { Id = 3, Email = "carol@example.com" });

        var result = await context.SaveChangesSafeAsync();

        Assert.True(result.IsSuccess);
        Assert.True(result.TryGetValue(out var affectedRows));
        Assert.Equal(1, affectedRows);
    }

    [Fact]
    public async Task ExecuteSafeAsync_WhenCancelled_ReturnsCancelledError()
    {
        await using var context = CreateContext();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await context.ExecuteSafeAsync<int>(
            (_, ct) => Task.FromCanceled<int>(ct),
            cancellation.Token);

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(DbErrorKind.Cancelled, error.Kind);
        Assert.True(error.Exception is OperationCanceledException canceled && canceled.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public async Task ExecuteSafeAsync_WhenTimeoutOccurs_ReturnsTransientTimeoutError()
    {
        await using var context = CreateContext();

        var result = await context.ExecuteSafeAsync<int>(
            (_, _) => throw new TimeoutException("The database operation timed out."));

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(DbErrorKind.Timeout, error.Kind);
        Assert.True(error.IsTransient);
    }

    [Fact]
    public async Task ExecuteSafeAsync_WhenQueryFails_ReturnsQueryFailure()
    {
        await using var context = CreateContext();

        var result = await context.ExecuteSafeAsync<int>(
            (_, _) => throw new InvalidOperationException("The query could not be translated."));

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(DbErrorKind.QueryFailure, error.Kind);
    }

    [Fact]
    public async Task SaveChangesSafeAsync_WhenSqliteConstraintFails_ReturnsConstraintViolation()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateSqliteContext(connection);
        await context.Database.EnsureCreatedAsync();

        context.Users.Add(new TestUser { Id = 1, Email = "duplicate@example.com" });
        Assert.True((await context.SaveChangesSafeAsync()).IsSuccess);

        context.Users.Add(new TestUser { Id = 2, Email = "duplicate@example.com" });
        var result = await context.SaveChangesSafeAsync();

        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Equal(DbErrorKind.ConstraintViolation, error.Kind);
    }

    [Fact]
    public async Task SingleOrNoneAsync_WithSqliteQueryTranslation_ReturnsMatchingEntity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateSqliteContext(connection);
        await context.Database.EnsureCreatedAsync();
        context.Users.Add(new TestUser { Id = 1, Email = "sqlite@example.com" });
        await context.SaveChangesAsync();

        var result = await context.Users
            .Where(user => user.Email.EndsWith("@example.com"))
            .SingleOrNoneAsync();

        var some = Assert.IsType<Option<TestUser>.Some>(result);
        Assert.Equal("sqlite@example.com", some.Value.Email);
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var context = new TestDbContext(options);
        context.Users.AddRange(
            new TestUser { Id = 1, Email = "alice@example.com" },
            new TestUser { Id = 2, Email = "bob@example.com" });
        context.SaveChanges();

        return context;
    }

    private static SqliteTestDbContext CreateSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SqliteTestDbContext>()
            .UseSqlite(connection)
            .Options;

        return new SqliteTestDbContext(options);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestUser> Users => Set<TestUser>();
    }

    private sealed class SqliteTestDbContext(DbContextOptions<SqliteTestDbContext> options) : DbContext(options)
    {
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<TestUser>().HasIndex(user => user.Email).IsUnique();
    }

    private sealed class TestUser
    {
        public int Id { get; init; }

        [MaxLength(320)]
        public string Email { get; init; } = string.Empty;
    }
}