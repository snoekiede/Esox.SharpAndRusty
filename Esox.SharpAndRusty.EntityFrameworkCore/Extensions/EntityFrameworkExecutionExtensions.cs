using Esox.SharpAndRusty.EntityFrameworkCore.Types;
using Esox.SharpAndRusty.Types;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRusty.EntityFrameworkCore.Extensions;

public static class EntityFrameworkExecutionExtensions
{
    public static Task<Result<T, DbError>> ExecuteSafeAsync<T>(
        this DbContext dbContext,
        Func<DbContext, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(operation);

        return dbContext.ExecuteSafeAsync((context, _) => operation(context), cancellationToken);
    }

    public static async Task<Result<T, DbError>> ExecuteSafeAsync<T>(
        this DbContext dbContext,
        Func<DbContext, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            var result = await operation(dbContext, cancellationToken).ConfigureAwait(false);
            return Result<T, DbError>.Ok(result);
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException
                                    or DbUpdateException
                                    or SqlException
                                    or InvalidOperationException
                                    or OperationCanceledException
                                    or TimeoutException)
        {
            return Result<T, DbError>.Err(DbError.FromException(ex));
        }
        catch (Exception ex)
        {
            return Result<T, DbError>.Err(DbError.FromException(ex));
        }
    }

    public static async Task<Result<int, DbError>> SaveChangesSafeAsync(
        this DbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        return await dbContext
            .ExecuteSafeAsync((ctx, ct) => ctx.SaveChangesAsync(ct), cancellationToken)
            .ConfigureAwait(false);
    }
}

