using Esox.SharpAndRusty.EntityFrameworkCore.Types;
using Esox.SharpAndRusty.Types;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRusty.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides helpers for executing Entity Framework Core operations as database results.
/// </summary>
public static class EntityFrameworkExecutionExtensions
{
    /// <summary>
    /// Executes a database operation and converts supported exceptions to <see cref="DbError" />.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="dbContext">The context used to execute the operation.</param>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A result containing the operation value or a mapped database error.</returns>
    public static Task<Result<T, DbError>> ExecuteSafeAsync<T>(
        this DbContext dbContext,
        Func<DbContext, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(operation);

        return dbContext.ExecuteSafeAsync((context, _) => operation(context), cancellationToken);
    }

    /// <summary>
    /// Executes a cancellation-aware database operation and converts exceptions to <see cref="DbError" />.
    /// </summary>
    /// <typeparam name="T">The operation result type.</typeparam>
    /// <param name="dbContext">The context used to execute the operation.</param>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="cancellationToken">The token passed to the operation.</param>
    /// <returns>A result containing the operation value or a mapped database error.</returns>
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

    /// <summary>
    /// Saves pending changes and converts database exceptions to <see cref="DbError" />.
    /// </summary>
    /// <param name="dbContext">The context whose pending changes are saved.</param>
    /// <param name="cancellationToken">The token used to cancel the save operation.</param>
    /// <returns>A result containing the number of affected rows or a mapped database error.</returns>
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