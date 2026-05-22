using Esox.SharpAndRusty.EntityFrameworkCore.Extensions;
using Esox.SharpAndRusty.EntityFrameworkCore.Types;
using Esox.SharpAndRusty.Types;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRust.Tests.Extensions;

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

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestUser> Users => Set<TestUser>();
    }

    private sealed class TestUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}

