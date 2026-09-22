using System.Linq.Expressions;
using Esox.SharpAndRusty.Types;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRusty.EntityFrameworkCore.Extensions;

/// <summary>
/// Provides Entity Framework Core query helpers that return <see cref="Option{T}" /> values.
/// </summary>
public static class EntityFrameworkOptionExtensions
{
    /// <summary>
    /// Returns the first entity in a query, or <see cref="Option{T}.None" /> when no entity exists.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The first entity wrapped in an option, or an empty option.</returns>
    public static async Task<Option<TEntity>> FirstOrNoneAsync<TEntity>(
        this IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return entity is null ? new Option<TEntity>.None() : new Option<TEntity>.Some(entity);
    }

    /// <summary>
    /// Returns the first entity matching a predicate, or <see cref="Option{T}.None" /> when none matches.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="predicate">The filter applied to the query.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The matching entity wrapped in an option, or an empty option.</returns>
    public static Task<Option<TEntity>> FirstOrNoneAsync<TEntity>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return query.Where(predicate).FirstOrNoneAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the single entity in a query, or <see cref="Option{T}.None" /> when no entity exists.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The single entity wrapped in an option, or an empty option.</returns>
    public static async Task<Option<TEntity>> SingleOrNoneAsync<TEntity>(
        this IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return entity is null ? new Option<TEntity>.None() : new Option<TEntity>.Some(entity);
    }

    /// <summary>
    /// Returns the single entity matching a predicate, or <see cref="Option{T}.None" /> when none matches.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="predicate">The filter applied to the query.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The matching entity wrapped in an option, or an empty option.</returns>
    public static Task<Option<TEntity>> SingleOrNoneAsync<TEntity>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return query.Where(predicate).SingleOrNoneAsync(cancellationToken);
    }
}