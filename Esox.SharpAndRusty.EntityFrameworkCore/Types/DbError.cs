using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Esox.SharpAndRusty.EntityFrameworkCore.Types;

public enum DbErrorKind
{
    ConstraintViolation,
    ConcurrencyConflict,
    Timeout,
    ConnectionFailure,
    Cancelled,
    UpdateFailure,
    QueryFailure,
    Unknown
}

public sealed record DbError(
    string Message,
    DbErrorKind Kind,
    Exception? Exception = null,
    int? SqlErrorNumber = null,
    string? ConstraintName = null,
    bool IsTransient = false)
{
    public static DbError FromException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is OperationCanceledException)
        {
            return new DbError("Database operation was cancelled", DbErrorKind.Cancelled, exception);
        }

        if (exception is TimeoutException)
        {
            return new DbError(exception.Message, DbErrorKind.Timeout, exception, IsTransient: true);
        }

        if (exception is DbUpdateConcurrencyException)
        {
            return new DbError(
                "Database concurrency conflict occurred",
                DbErrorKind.ConcurrencyConflict,
                exception,
                IsTransient: true);
        }

        if (exception is DbUpdateException dbUpdateException)
        {
            return FromDbUpdateException(dbUpdateException);
        }

        if (exception is SqlException sqlException)
        {
            return FromSqlException(sqlException);
        }

        if (exception is InvalidOperationException)
        {
            return new DbError(exception.Message, DbErrorKind.QueryFailure, exception);
        }

        return new DbError(exception.Message, DbErrorKind.Unknown, exception);
    }

    private static DbError FromDbUpdateException(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            return FromSqlException(sqlException);
        }

        if (LooksLikeConstraintViolation(exception.Message) || LooksLikeConstraintViolation(exception.InnerException?.Message))
        {
            return new DbError(
                exception.Message,
                DbErrorKind.ConstraintViolation,
                exception,
                ConstraintName: TryExtractConstraintName(exception.Message));
        }

        return new DbError(exception.Message, DbErrorKind.UpdateFailure, exception);
    }

    private static DbError FromSqlException(SqlException exception)
    {
        var kind = exception.Number switch
        {
            2601 or 2627 or 547 or 515 => DbErrorKind.ConstraintViolation,
            -2 => DbErrorKind.Timeout,
            1205 or 4060 or 10053 or 10054 or 10060 => DbErrorKind.ConnectionFailure,
            _ => DbErrorKind.UpdateFailure
        };

        var isTransient = exception.Number is 1205 or -2 or 4060 or 10053 or 10054 or 10060;

        return new DbError(
            exception.Message,
            kind,
            exception,
            SqlErrorNumber: exception.Number,
            ConstraintName: TryExtractConstraintName(exception.Message),
            IsTransient: isTransient);
    }

    private static bool LooksLikeConstraintViolation(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("constraint", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || message.Contains("foreign key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("primary key", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryExtractConstraintName(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        const string marker = "constraint '";
        var markerIndex = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        var start = markerIndex + marker.Length;
        var end = message.IndexOf('\'', start);
        if (end <= start)
        {
            return null;
        }

        return message[start..end];
    }
}


