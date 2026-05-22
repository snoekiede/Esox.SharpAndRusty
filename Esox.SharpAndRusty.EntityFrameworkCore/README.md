# Esox.SharpAndRusty.EntityFrameworkCore

Entity Framework Core integration for `Esox.SharpAndRusty`.

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

