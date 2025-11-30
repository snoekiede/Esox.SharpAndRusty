# Error Type Examples

This file demonstrates practical usage of the `Error` type in the Esox.SharpAndRusty library.

## Example 1: File Processing with Error Context

```csharp
using Esox.SharpAndRusty.Types;
using Esox.SharpAndRusty.Extensions;

public class ConfigurationService
{
    public Result<AppConfig, Error> LoadConfiguration(string path)
    {
        return ReadFile(path)
            .Context($"Failed to load configuration from '{path}'")
            .WithMetadata("configPath", path)
            .Bind(content => ParseJson(content)
                .Context("Failed to parse configuration JSON")
                .WithKind(ErrorKind.ParseError))
            .Bind(json => ValidateConfig(json)
                .Context("Configuration validation failed"));
    }

    private Result<string, Error> ReadFile(string path)
    {
        return ErrorExtensions.Try(() => File.ReadAllText(path))
            .WithKind(ErrorKind.Io);
    }

    private Result<JsonDocument, Error> ParseJson(string content)
    {
        return ErrorExtensions.Try(() => JsonDocument.Parse(content))
            .WithKind(ErrorKind.ParseError);
    }

    private Result<AppConfig, Error> ValidateConfig(JsonDocument json)
    {
        // Validation logic...
        return Result<AppConfig, Error>.Ok(new AppConfig());
    }
}

// Usage:
var service = new ConfigurationService();
var result = service.LoadConfiguration("/etc/myapp/config.json");

result.Match(
    success: config => Console.WriteLine("Configuration loaded successfully"),
    failure: error => 
    {
        // Logs the full error chain:
        // "Configuration validation failed"
        //   Caused by: "Failed to parse configuration JSON"
        //     Caused by: "Failed to load configuration from '/etc/myapp/config.json'"
        //       Caused by: "Could not find file..."
        Console.Error.WriteLine(error.GetFullMessage());
    }
);
```

## Example 2: HTTP API Error Handling

```csharp
public class UserApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public async Task<Result<User, Error>> GetUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await ErrorExtensions.TryAsync(
            async () => await _httpClient.GetStringAsync($"/api/users/{userId}"),
            cancellationToken)
            .ContextAsync($"Failed to fetch user {userId} from API")
            .WithMetadataAsync("userId", userId, cancellationToken)
            .WithMetadataAsync("endpoint", "/api/users", cancellationToken)
            .BindAsync(
                async json => ParseUser(json),
                cancellationToken);
    }

    private Result<User, Error> ParseUser(string json)
    {
        return ErrorExtensions.Try(() => JsonSerializer.Deserialize<User>(json))
            .Context("Failed to deserialize user data")
            .WithKind(ErrorKind.ParseError);
    }
}

// Usage:
var client = new UserApiClient(httpClient, logger);
var result = await client.GetUserAsync(123, cancellationToken);

if (result.TryGetError(out var error))
{
    // Check error kind to determine appropriate action
    switch (error.Kind)
    {
        case ErrorKind.Timeout:
            await RetryOperation();
            break;
        case ErrorKind.NotFound:
            await CreateDefaultUser();
            break;
        case ErrorKind.ParseError:
            logger.LogError(error.GetFullMessage());
            break;
    }
}
```

## Example 3: Database Operations with Retry Logic

```csharp
public class DatabaseService
{
    private readonly IDbConnection _connection;

    public async Task<Result<Order, Error>> SaveOrderAsync(
        Order order,
        CancellationToken cancellationToken = default)
    {
        return await TryWithRetry(
            async () => await SaveOrderInternalAsync(order, cancellationToken),
            maxRetries: 3,
            cancellationToken)
            .ContextAsync("Failed to save order after retries")
            .WithMetadataAsync("orderId", order.Id, cancellationToken)
            .WithMetadataAsync("customerId", order.CustomerId, cancellationToken);
    }

    private async Task<Result<T, Error>> TryWithRetry<T>(
        Func<Task<Result<T, Error>>> operation,
        int maxRetries,
        CancellationToken cancellationToken)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            var result = await operation();
            
            if (result.IsSuccess)
                return result;

            if (result.TryGetError(out var error))
            {
                // Only retry on transient errors
                if (error.Kind is ErrorKind.Timeout or ErrorKind.ConnectionReset)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                        continue;
                    }
                }

                return result.WithMetadata("attempts", attempt);
            }
        }

        return Result<T, Error>.Err(
            Error.New("Maximum retry attempts reached", ErrorKind.Timeout)
                .WithMetadata("maxRetries", maxRetries));
    }

    private async Task<Result<Order, Error>> SaveOrderInternalAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        return await ErrorExtensions.TryAsync(
            async () =>
            {
                await _connection.ExecuteAsync(
                    "INSERT INTO Orders (Id, CustomerId, Total) VALUES (@Id, @CustomerId, @Total)",
                    order);
                return order;
            },
            cancellationToken);
    }
}
```

## Example 4: Validation Chain

```csharp
public class OrderValidator
{
    public Result<Order, Error> ValidateOrder(Order order)
    {
        return ValidateCustomer(order.CustomerId)
            .Bind(_ => ValidateItems(order.Items))
            .Bind(_ => ValidateTotal(order.Total))
            .Bind(_ => ValidateShippingAddress(order.ShippingAddress))
            .Map(_ => order)
            .Context("Order validation failed")
            .WithMetadata("orderId", order.Id);
    }

    private Result<Unit, Error> ValidateCustomer(int customerId)
    {
        if (customerId <= 0)
            return Result<Unit, Error>.Err(
                Error.New("Invalid customer ID", ErrorKind.InvalidInput)
                    .WithMetadata("customerId", customerId));

        return Result<Unit, Error>.Ok(Unit.Value);
    }

    private Result<Unit, Error> ValidateItems(List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            return Result<Unit, Error>.Err(
                Error.New("Order must contain at least one item", ErrorKind.InvalidInput));

        foreach (var item in items)
        {
            if (item.Quantity <= 0)
                return Result<Unit, Error>.Err(
                    Error.New("Item quantity must be positive", ErrorKind.InvalidInput)
                        .WithMetadata("itemId", item.Id)
                        .WithMetadata("quantity", item.Quantity));

            if (item.Price < 0)
                return Result<Unit, Error>.Err(
                    Error.New("Item price cannot be negative", ErrorKind.InvalidInput)
                        .WithMetadata("itemId", item.Id)
                        .WithMetadata("price", item.Price));
        }

        return Result<Unit, Error>.Ok(Unit.Value);
    }

    private Result<Unit, Error> ValidateTotal(decimal total)
    {
        if (total <= 0)
            return Result<Unit, Error>.Err(
                Error.New("Order total must be positive", ErrorKind.InvalidInput)
                    .WithMetadata("total", total));

        return Result<Unit, Error>.Ok(Unit.Value);
    }

    private Result<Unit, Error> ValidateShippingAddress(Address address)
    {
        if (string.IsNullOrWhiteSpace(address.Street))
            return Result<Unit, Error>.Err(
                Error.New("Shipping address street is required", ErrorKind.InvalidInput));

        if (string.IsNullOrWhiteSpace(address.City))
            return Result<Unit, Error>.Err(
                Error.New("Shipping address city is required", ErrorKind.InvalidInput));

        if (string.IsNullOrWhiteSpace(address.PostalCode))
            return Result<Unit, Error>.Err(
                Error.New("Shipping address postal code is required", ErrorKind.InvalidInput));

        return Result<Unit, Error>.Ok(Unit.Value);
    }
}

// Usage with detailed error reporting:
var validator = new OrderValidator();
var result = validator.ValidateOrder(order);

if (result.TryGetError(out var error))
{
    // Get structured validation errors
    var validationErrors = new List<ValidationError>();
    var current = error;
    
    while (current != null)
    {
        if (current.Kind == ErrorKind.InvalidInput)
        {
            var field = current.TryGetMetadata("itemId", out var itemId) 
                ? $"Item {itemId}" 
                : "Order";
            
            validationErrors.Add(new ValidationError
            {
                Field = field,
                Message = current.Message,
                Metadata = GetAllMetadata(current)
            });
        }
        
        current = current.Source;
    }
    
    return BadRequest(new { errors = validationErrors });
}
```

## Example 5: Multi-Step Process with Detailed Error Tracking

```csharp
public class OrderProcessingService
{
    public async Task<Result<OrderConfirmation, Error>> ProcessOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        return await ValidateRequest(request)
            .Context("Order validation failed")
            .WithMetadata("step", "validation")
            .WithMetadata("startTime", startTime)
            .BindAsync(async _ =>
                await ReserveInventoryAsync(request.Items, cancellationToken)
                    .ContextAsync("Failed to reserve inventory")
                    .WithMetadataAsync("step", "inventory", cancellationToken),
                cancellationToken)
            .BindAsync(async reservation =>
                await ProcessPaymentAsync(request.Payment, cancellationToken)
                    .ContextAsync("Payment processing failed")
                    .WithMetadataAsync("step", "payment", cancellationToken)
                    .WithMetadataAsync("reservationId", reservation.Id, cancellationToken),
                cancellationToken)
            .BindAsync(async payment =>
                await CreateOrderAsync(request, payment, cancellationToken)
                    .ContextAsync("Failed to create order")
                    .WithMetadataAsync("step", "orderCreation", cancellationToken)
                    .WithMetadataAsync("paymentId", payment.Id, cancellationToken),
                cancellationToken)
            .BindAsync(async order =>
                await SendConfirmationAsync(order, cancellationToken)
                    .ContextAsync("Failed to send confirmation email")
                    .WithMetadataAsync("step", "notification", cancellationToken)
                    .WithMetadataAsync("orderId", order.Id, cancellationToken),
                cancellationToken)
            .TapAsync(
                onSuccess: async confirmation =>
                {
                    var duration = DateTime.UtcNow - startTime;
                    await _metrics.RecordSuccessAsync("order.process", duration);
                },
                onFailure: async error =>
                {
                    var duration = DateTime.UtcNow - startTime;
                    await _metrics.RecordFailureAsync("order.process", duration);
                    await _logger.LogErrorAsync(error.GetFullMessage());
                    
                    // Compensate based on the step that failed
                    if (error.TryGetMetadata("step", out var step))
                    {
                        await CompensateFailedStepAsync(step.ToString(), error);
                    }
                },
                cancellationToken);
    }

    private async Task CompensateFailedStepAsync(string step, Error error)
    {
        switch (step)
        {
            case "payment":
                // Rollback inventory reservation
                if (error.TryGetMetadata("reservationId", out var resId))
                    await ReleaseInventoryAsync(resId);
                break;
            case "orderCreation":
                // Refund payment
                if (error.TryGetMetadata("paymentId", out var payId))
                    await RefundPaymentAsync(payId);
                break;
        }
    }
}
```

These examples demonstrate how the `Error` type enables:
- **Context chaining** for better error messages
- **Metadata attachment** for debugging and monitoring
- **Error categorization** for appropriate handling
- **Exception boundaries** with automatic conversion
- **Structured error handling** in complex workflows
