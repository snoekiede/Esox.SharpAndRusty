using Esox.SharpAndRusty.EntityFrameworkCore.Types;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRusty.Tests.Extensions;

public class DbErrorTests
{
    [Fact]
    public void FromException_WhenUpdateMessageContainsConstraint_ExtractsConstraintName()
    {
        var exception = new DbUpdateException(
            "The INSERT statement conflicted with the constraint 'FK_Users_Roles'.",
            new InvalidOperationException("constraint violation"));

        var error = DbError.FromException(exception);

        Assert.Equal(DbErrorKind.ConstraintViolation, error.Kind);
        Assert.Equal("FK_Users_Roles", error.ConstraintName);
        Assert.Same(exception, error.Exception);
    }

    [Fact]
    public void FromException_WhenUnknownException_ReturnsUnknown()
    {
        var exception = new InvalidCastException("Unexpected database provider failure.");

        var error = DbError.FromException(exception);

        Assert.Equal(DbErrorKind.Unknown, error.Kind);
        Assert.Same(exception, error.Exception);
    }

    [Fact]
    public void FromException_WhenCancellationOccurs_ReturnsCancelled()
    {
        var exception = new OperationCanceledException("Cancelled.");

        var error = DbError.FromException(exception);

        Assert.Equal(DbErrorKind.Cancelled, error.Kind);
        Assert.False(error.IsTransient);
    }
}