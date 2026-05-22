using Esox.SharpAndRusty.Types;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Esox.SharpAndRusty.EntityFrameworkCore.Extensions;

public static class EntityFrameworkOptionExtensions
{
    public static async Task<Option<TEntity>> FirstOrNoneAsync<TEntity>(
        this IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return entity is null ? new Option<TEntity>.None() : new Option<TEntity>.Some(entity);
    }

    public static Task<Option<TEntity>> FirstOrNoneAsync<TEntity>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return query.Where(predicate).FirstOrNoneAsync(cancellationToken);
    }

    public static async Task<Option<TEntity>> SingleOrNoneAsync<TEntity>(
        this IQueryable<TEntity> query,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);

        var entity = await query.SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return entity is null ? new Option<TEntity>.None() : new Option<TEntity>.Some(entity);
    }

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

