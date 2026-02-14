# Esox.SharpAndRusty.AspNetCore

ASP.NET Core integration for **Esox.SharpAndRusty** functional types (`Option`, `Result`, `Either`, `Validation`).

## Features

- ✅ **Action Result Conversions** - Convert `Result`/`Option`/`Either`/`Validation` to `IActionResult`
- ✅ **RFC 7807 ProblemDetails** - Automatic conversion of `Error` types to standardized problem details
- ✅ **Model Binding** - Bind `Option<T>` from request parameters/body
- ✅ **Global Error Handling** - Middleware for catching exceptions and converting to ProblemDetails
- ✅ **Automatic Status Codes** - ErrorKind automatically maps to appropriate HTTP status codes
- ✅ **Validation Integration** - `Validation<T, E>` converts to ValidationProblemDetails

## Installation

```bash
dotnet add package Esox.SharpAndRusty.AspNetCore
```

## Quick Start

### 1. Configure Services

```csharp
// Program.cs or Startup.cs
builder.Services.AddSharpAndRusty();
```

### 2. Add Middleware

```csharp
// Development
if (app.Environment.IsDevelopment())
{
    app.UseResultMiddlewareDevelopment(); // Includes stack traces
}
else
{
    app.UseResultMiddlewareProduction(); // Production-safe
}
```

### 3. Use in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    {
        var result = _userService.GetUser(id); // Returns Result<User, Error>
        return result.ToActionResult(); // Automatic conversion!
    }
}
```

---

## Action Result Conversions

### Result<T, E> → IActionResult

```csharp
// Automatic status code mapping for Result<T, Error>
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    var result = GetUser(id); // Result<User, Error>
    return result.ToActionResult(); // 200 OK or 404/400/500 based on ErrorKind
}

// Custom status code for generic errors
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    var result = CreateUser(dto); // Result<User, string>
    return result.ToActionResult(statusCode: 422); // 422 Unprocessable Entity on error
}

// Created result (201)
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    var result = CreateUser(dto); // Result<User, Error>
    return result.ToCreatedResult(user => $"/api/users/{user.Id}");
    // Returns 201 Created with Location header
}

// No content result (204)
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
    var result = DeleteUser(id); // Result<Unit, Error>
    return result.ToNoContentResult(); // 204 No Content or error
}

// Accepted result (202)
[HttpPost("batch")]
public IActionResult BatchProcess([FromBody] BatchRequest request)
{
    var result = QueueBatchJob(request); // Result<JobId, Error>
    return result.ToAcceptedResult($"/api/jobs/{result.Value.Id}");
    // Returns 202 Accepted with Location
}
```

### Option<T> → IActionResult

```csharp
// Automatic 404 for None
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    var option = FindUser(id); // Option<User>
    return option.ToActionResult(); // 200 OK (Some) or 404 Not Found (None)
}

// Custom 404 message
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    var option = FindUser(id); // Option<User>
    return option.ToActionResult($"User {id} not found");
}

// Custom result mapping
[HttpGet("{id}")]
public IActionResult Get(int id)
{
    var option = FindUser(id); // Option<User>
    return option.ToActionResult(
        someResult: user => Ok(new { user, message = "Found!" }),
        noneResult: () => NotFound(new { message = "Not found", id })
    );
}
```

### Either<L, R> → IActionResult

```csharp
// Left = success, Right = error (Rust convention)
[HttpPost]
public IActionResult Process([FromBody] ProcessRequest request)
{
    var either = ProcessData(request); // Either<ProcessResult, Error>
    return either.ToActionResult(); // 200 OK (Left) or error (Right)
}

// Custom status code for Right
[HttpPost]
public IActionResult Process([FromBody] ProcessRequest request)
{
    var either = ProcessData(request); // Either<ProcessResult, ValidationError>
    return either.ToActionResult(rightStatusCode: 422);
}
```

### Validation<T, E> → IActionResult

```csharp
// Automatic ValidationProblemDetails format
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    var validation = ValidateUser(dto); // Validation<User, Error>
    return validation.ToActionResult();
    // Returns 200 OK or 400 Bad Request with ALL validation errors
}

// Custom field mapping
[HttpPost]
public IActionResult Create([FromBody] CreateUserDto dto)
{
    var validation = ValidateUser(dto); // Validation<User, FieldError>
    return validation.ToValidationResult(
        keySelector: error => error.FieldName,
        messageSelector: error => error.Message
    );
}
```

---

## RFC 7807 ProblemDetails Integration

The library automatically converts `Error` types to RFC 7807 ProblemDetails with appropriate status codes:

### Automatic Status Code Mapping

| ErrorKind | HTTP Status | Title |
|-----------|-------------|-------|
| `NotFound` | 404 | Not Found |
| `PermissionDenied` | 403 | Forbidden |
| `Unauthorized` | 401 | Unauthorized |
| `InvalidInput` | 400 | Bad Request |
| `ParseError` | 400 | Bad Request |
| `InvalidOperation` | 400 | Bad Request |
| `AlreadyExists` | 409 | Conflict |
| `Timeout` | 408 | Request Timeout |
| `ResourceExhausted` | 429 | Too Many Requests |
| `NotSupported` | 501 | Not Implemented |
| `ConnectionRefused` | 503 | Service Unavailable |
| `ConnectionReset` | 503 | Service Unavailable |
| `Io` | 500 | Internal Server Error |
| `Other` | 500 | Internal Server Error |

### Example ProblemDetails Response

```csharp
// Code
var error = Error.New("User not found", ErrorKind.NotFound)
    .WithContext("Database query returned no results")
    .WithMetadata("userId", 123);

return error.ToProblemDetails();
```

```json
// HTTP Response: 404 Not Found
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "User not found: Database query returned no results",
  "instance": "/api/users/123",
  "errors": [
    {
      "message": "User not found",
      "kind": "NotFound"
    },
    {
      "message": "Database query returned no results",
      "kind": "NotFound"
    }
  ],
  "traceId": "0HM7...",
  "requestId": "0HM7..."
}
```

---

## Global Error Handling Middleware

### Basic Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharpAndRusty();

var app = builder.Build();

// Add middleware based on environment
if (app.Environment.IsDevelopment())
{
    app.UseResultMiddlewareDevelopment(); // Includes stack traces
}
else
{
    app.UseResultMiddlewareProduction(); // Production-safe
}

app.MapControllers();
app.Run();
```

### Custom Configuration

```csharp
app.UseResultMiddleware(new ResultMiddlewareOptions
{
    IncludeStackTrace = app.Environment.IsDevelopment(),
    IncludeFileInfo = app.Environment.IsDevelopment(),
    WriteIndented = app.Environment.IsDevelopment(),
    
    // Only handle specific exceptions
    HandleException = ex => ex is not OperationCanceledException,
    
    // Add custom metadata
    CustomMetadataProvider = (context, error) => error
        .WithMetadata("user_id", context.User?.FindFirst("sub")?.Value)
        .WithMetadata("tenant_id", context.Request.Headers["X-Tenant-Id"].FirstOrDefault())
});
```

### What It Does

1. **Catches unhandled exceptions** globally
2. **Converts to `Error`** types
3. **Maps to ProblemDetails** with appropriate status code
4. **Adds request context** (trace ID, path)
5. **Logs errors** using `ILogger`
6. **Returns JSON** in RFC 7807 format

---

## Model Binding

### Option<T> Binding

The library provides automatic model binding for `Option<T>`, treating missing/null values as `None` instead of validation errors:

```csharp
[HttpGet]
public IActionResult Search(
    [FromQuery] string query,
    [FromQuery] Option<int> pageSize,      // Optional parameter
    [FromQuery] Option<string> sortBy)     // Optional parameter
{
    var size = pageSize.UnwrapOr(10);     // Default to 10
    var sort = sortBy.UnwrapOr("name");    // Default to "name"
    
    var results = _searchService.Search(query, size, sort);
    return Ok(results);
}

// Usage:
// GET /api/search?query=test                     -> pageSize: None, sortBy: None
// GET /api/search?query=test&pageSize=20         -> pageSize: Some(20), sortBy: None
// GET /api/search?query=test&sortBy=date         -> pageSize: None, sortBy: Some("date")
```

### Complex Types

```csharp
public class UpdateUserDto
{
    public required string Name { get; set; }
    public Option<string> Email { get; set; }       // Optional update
    public Option<DateTime> BirthDate { get; set; } // Optional update
    public Option<string?> Bio { get; set; }        // Optional, can be set to null
}

[HttpPatch("{id}")]
public IActionResult Update(int id, [FromBody] UpdateUserDto dto)
{
    var user = GetUser(id);
    
    // Only update fields that are Some
    dto.Email.Iter(email => user.Email = email);
    dto.BirthDate.Iter(date => user.BirthDate = date);
    dto.Bio.Iter(bio => user.Bio = bio);
    
    SaveUser(user);
    return NoContent();
}
```

---

## Real-World Examples

### Example 1: CRUD Operations

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    [HttpGet]
    public IActionResult GetAll()
    {
        var products = _productService.GetAll();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        // Option<Product> - 200 OK or 404 Not Found
        return _productService.FindById(id).ToActionResult();
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateProductDto dto)
    {
        // Validation<Product, Error> - accumulates all validation errors
        var validation = _productService.Validate(dto);
        
        return validation
            .Map(product => _productService.Create(product))
            .ToActionResult(); // 200 OK with product or 400 with all errors
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateProductDto dto)
    {
        // Result<Product, Error> - short-circuits on first error
        var result = _productService.Update(id, dto);
        
        return result.ToActionResult(); // 200 OK or appropriate error status
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        // Result<Unit, Error>
        return _productService.Delete(id).ToNoContentResult(); // 204 No Content or error
    }
}
```

### Example 2: Search with Optional Parameters

```csharp
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public IActionResult Search(
        [FromQuery] string query,
        [FromQuery] Option<int> page,
        [FromQuery] Option<int> pageSize,
        [FromQuery] Option<string> category,
        [FromQuery] Option<decimal> minPrice,
        [FromQuery] Option<decimal> maxPrice)
    {
        var searchParams = new SearchParameters
        {
            Query = query,
            Page = page.UnwrapOr(1),
            PageSize = pageSize.UnwrapOr(20),
            Category = category,
            PriceRange = minPrice.FlatMap(min =>
                maxPrice.Map(max => new PriceRange(min, max)))
        };

        var results = _searchService.Search(searchParams);
        return Ok(results);
    }
}
```

### Example 3: Complex Validation

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        // Validate all fields and accumulate errors
        var validations = new[]
        {
            ValidateEmail(dto.Email),
            ValidatePassword(dto.Password),
            ValidateAge(dto.Age),
            ValidateUsername(dto.Username)
        };

        // Sequence validations - returns Valid only if ALL are valid
        var result = validations.Sequence() // From ValidationExtensions
            .Map(values => new User(dto.Email, dto.Password, dto.Age, dto.Username))
            .Bind(user => _userService.Create(user));

        return result.ToActionResult();
        // Returns all validation errors if any fail, or 200 OK with created user
    }

    private Validation<string, Error> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Validation<string, Error>.Invalid("Email is required");
        if (!email.Contains("@"))
            return Validation<string, Error>.Invalid("Email must be valid");
        
        return Validation<string, Error>.Valid(email);
    }

    private Validation<string, Error> ValidatePassword(string password)
    {
        var errors = new List<Error>();
        
        if (password.Length < 8)
            errors.Add(Error.New("Password must be at least 8 characters"));
        if (!password.Any(char.IsUpper))
            errors.Add(Error.New("Password must contain uppercase letter"));
        if (!password.Any(char.IsDigit))
            errors.Add(Error.New("Password must contain number"));
        
        return errors.Any()
            ? Validation<string, Error>.Invalid(errors)
            : Validation<string, Error>.Valid(password);
    }
}
```

### Example 4: Async Operations with Result

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        // Chain async operations with Result
        var result = await _orderService
            .ValidateInventory(dto.Items)
            .BindAsync(async _ => await _paymentService.ProcessPayment(dto.Payment))
            .BindAsync(async payment => await _orderService.CreateOrder(dto, payment))
            .MapAsync(async order => await _notificationService.SendConfirmation(order));

        return result.ToCreatedResult(order => $"/api/orders/{order.Id}");
    }
}
```

---

## Integration with Existing Code

### Wrapping Existing Services

```csharp
public class UserService
{
    // Existing method that might return null
    public User? GetUserById(int id)
    {
        return _db.Users.Find(id);
    }

    // Wrap with Option
    public Option<User> FindUserById(int id)
    {
        return Option<User>.From(GetUserById(id));
    }

    // Existing method that throws exceptions
    public void DeleteUser(int id)
    {
        var user = GetUserById(id);
        if (user == null)
            throw new NotFoundException($"User {id} not found");
        
        _db.Users.Remove(user);
        _db.SaveChanges();
    }

    // Wrap with Result
    public Result<Unit, Error> TryDeleteUser(int id)
    {
        try
        {
            DeleteUser(id);
            return Result<Unit, Error>.Ok(Unit.Default);
        }
        catch (Exception ex)
        {
            return Result<Unit, Error>.Err(Error.FromException(ex));
        }
    }
}
```

---

## Best Practices

### ✅ DO

```csharp
// Use Result<T, Error> for operations that can fail
public Result<User, Error> GetUser(int id);

// Use Option<T> for values that may not exist
public Option<User> FindUser(int id);

// Use Validation<T, E> when you need ALL errors
public Validation<User, Error> ValidateUser(CreateUserDto dto);

// Let the middleware handle unhandled exceptions
app.UseResultMiddleware();

// Use appropriate status codes
return result.ToCreatedResult(user => $"/api/users/{user.Id}");
```

### ❌ DON'T

```csharp
// Don't return nullable types when Option makes sense
public User? GetUser(int id); // ❌
public Option<User> GetUser(int id); // ✅

// Don't throw exceptions for expected failures
public User GetUser(int id)
{
    var user = _db.Find(id);
    if (user == null)
        throw new NotFoundException(); // ❌
}
// Use Result instead ✅
public Result<User, Error> GetUser(int id);

// Don't catch and return 500 for everything
catch (Exception ex)
{
    return StatusCode(500, "Error"); // ❌
}
// Let middleware handle it ✅
```

---

## Configuration Options

### Service Configuration

```csharp
builder.Services.AddSharpAndRusty(options =>
{
    options.EnableOptionModelBinding = true;  // Enable Option<T> binding
    options.EnableResultModelBinding = false; // Usually not needed
});
```

### Middleware Configuration

```csharp
app.UseResultMiddleware(new ResultMiddlewareOptions
{
    // Include stack traces (dev only)
    IncludeStackTrace = true,
    
    // Include file paths in stack traces
    IncludeFileInfo = true,
    
    // Pretty-print JSON
    WriteIndented = true,
    
    // Filter exceptions
    HandleException = ex => ex is not OperationCanceledException,
    
    // Add custom metadata
    CustomMetadataProvider = (context, error) =>
    {
        return error
            .WithMetadata("correlation_id", context.TraceIdentifier)
            .WithMetadata("user_agent", context.Request.Headers["User-Agent"].ToString());
    }
});
```

---

## Testing

```csharp
[Fact]
public void GetUser_WithValidId_ReturnsOk()
{
    // Arrange
    var user = new User { Id = 1, Name = "Test" };
    _mockService.Setup(s => s.GetUser(1))
        .Returns(Result<User, Error>.Ok(user));

    // Act
    var result = _controller.GetUser(1);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result);
    Assert.Equal(user, okResult.Value);
}

[Fact]
public void GetUser_WithInvalidId_ReturnsNotFound()
{
    // Arrange
    var error = Error.New("Not found", ErrorKind.NotFound);
    _mockService.Setup(s => s.GetUser(999))
        .Returns(Result<User, Error>.Err(error));

    // Act
    var result = _controller.GetUser(999);

    // Assert
    var objectResult = Assert.IsType<ObjectResult>(result);
    Assert.Equal(404, objectResult.StatusCode);
}
```

---

## See Also

- [Esox.SharpAndRusty Core Library](../README.md)
- [Result Type Documentation](../RESULT_DOCUMENTATION.md)
- [Option Type Documentation](../OPTION_QUICK_REFERENCE.md)
- [Error Handling Guide](../ERROR_TYPE.md)
- [Validation Documentation](../VALIDATION_DOCUMENTATION.md)

---

## License

This library is licensed under the same terms as Esox.SharpAndRusty.
