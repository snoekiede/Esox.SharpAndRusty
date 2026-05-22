# Esox.SharpAndRusty.EntityFrameworkCore

Entity Framework Core integration for `Esox.SharpAndRusty`.

## License

This project is licensed under the MIT License. See `LICENSE.txt`.

## Disclaimer

This package is provided "as is" without warranty of any kind.

- Exception mapping is best-effort and provider-specific details can vary.
- `DbError` classification may differ between SQL Server, SQLite, PostgreSQL, and other providers.
- You should still validate behavior in your own environment, especially for constraint handling and retry logic.
- This package does not replace transactional design, idempotency, or proper database observability in production systems.

## What it adds

- `FirstOrNoneAsync()` and `SingleOrNoneAsync()` to return `Option<T>` instead of nullable entities.
- `ExecuteSafeAsync()` and `SaveChangesSafeAsync()` to return `Result<T, DbError>` instead of throwing for common EF/SQL failures.

## Quick example

```csharp
using Esox.SharpAndRusty.EntityFrameworkCore.Extensions;

var userOption = await dbContext.Users
    .Where(x => x.Email == email)
    .SingleOrNoneAsync(cancellationToken);

var saveResult = await dbContext.SaveChangesSafeAsync(cancellationToken);
```

## Notes

- `Option<T>` methods are constrained to entity reference types (`where T : class`).
- `ExecuteSafeAsync()` maps `DbUpdateException`, `DbUpdateConcurrencyException`, `SqlException`, timeout, cancellation, and query failures into `DbError`.
