using Esox.SharpAndRusty.Types;
using Xunit;

namespace Esox.SharpAndRust.Tests.Types
{
    public class ErrorTests
    {
        #region Basic Creation Tests

        [Fact]
        public void New_CreatesErrorWithMessage()
        {
            var error = Error.New("Something went wrong");

            Assert.Equal("Something went wrong", error.Message);
            Assert.Equal(ErrorKind.Other, error.Kind);
            Assert.False(error.HasSource);
            Assert.Null(error.Source);
        }

        [Fact]
        public void New_WithKind_CreatesErrorWithSpecifiedKind()
        {
            var error = Error.New("File not found", ErrorKind.NotFound);

            Assert.Equal("File not found", error.Message);
            Assert.Equal(ErrorKind.NotFound, error.Kind);
        }

        [Fact]
        public void New_WithNullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Error.New(null!));
        }

        #endregion

        #region FromException Tests

        [Fact]
        public void FromException_ConvertsExceptionToError()
        {
            Exception exception;
            try
            {
                throw new InvalidOperationException("Operation failed");
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }
            
            var error = Error.FromException(exception);

            Assert.Equal("Operation failed", error.Message);
            Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
        }

        [Fact]
        public void FromException_WithInnerException_CreatesErrorChain()
        {
            var innerException = new ArgumentException("Invalid argument");
            var outerException = new InvalidOperationException("Operation failed", innerException);
            var error = Error.FromException(outerException);

            Assert.Equal("Operation failed", error.Message);
            Assert.True(error.HasSource);
            Assert.NotNull(error.Source);
            Assert.Equal("Invalid argument", error.Source!.Message);
            Assert.Equal(ErrorKind.InvalidInput, error.Source.Kind);
        }

        [Fact]
        public void FromException_WithNullException_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Error.FromException(null!));
        }

        [Theory]
        [InlineData(typeof(ArgumentException), ErrorKind.InvalidInput)]
        [InlineData(typeof(ArgumentNullException), ErrorKind.InvalidInput)]
        [InlineData(typeof(InvalidOperationException), ErrorKind.InvalidOperation)]
        [InlineData(typeof(NotSupportedException), ErrorKind.NotSupported)]
        [InlineData(typeof(UnauthorizedAccessException), ErrorKind.PermissionDenied)]
        [InlineData(typeof(TimeoutException), ErrorKind.Timeout)]
        [InlineData(typeof(OperationCanceledException), ErrorKind.Interrupted)]
        [InlineData(typeof(IOException), ErrorKind.Io)]
        [InlineData(typeof(Exception), ErrorKind.Other)]
        public void FromException_MapsExceptionTypeToErrorKind(Type exceptionType, ErrorKind expectedKind)
        {
            var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;
            var error = Error.FromException(exception);

            Assert.Equal(expectedKind, error.Kind);
        }

        [Fact]
        public void FromException_MapsFileNotFoundException_ToNotFound()
        {
            var exception = new FileNotFoundException("File not found");
            var error = Error.FromException(exception);

            Assert.Equal(ErrorKind.NotFound, error.Kind);
            Assert.Equal("File not found", error.Message);
        }

        [Fact]
        public void FromException_MapsTaskCanceledException_ToInterrupted()
        {
            var exception = new TaskCanceledException("Task was cancelled");
            var error = Error.FromException(exception);

            Assert.Equal(ErrorKind.Interrupted, error.Kind);
        }

        [Fact]
        public void FromException_MapsFormatException_ToParseError()
        {
            var exception = new FormatException("Invalid format");
            var error = Error.FromException(exception);

            Assert.Equal(ErrorKind.ParseError, error.Kind);
        }

        [Fact]
        public void FromException_MapsOutOfMemoryException_ToResourceExhausted()
        {
            var exception = new OutOfMemoryException("Out of memory");
            var error = Error.FromException(exception);

            Assert.Equal(ErrorKind.ResourceExhausted, error.Kind);
        }

        #endregion

        #region WithContext Tests

        [Fact]
        public void WithContext_AddsContextToError()
        {
            var original = Error.New("File not found", ErrorKind.NotFound);
            var withContext = original.WithContext("Failed to load configuration");

            Assert.Equal("Failed to load configuration", withContext.Message);
            Assert.True(withContext.HasSource);
            Assert.Equal(original, withContext.Source);
            Assert.Equal("File not found", withContext.Source!.Message);
        }

        [Fact]
        public void WithContext_CanChainMultipleTimes()
        {
            var error = Error.New("File not found", ErrorKind.NotFound)
                .WithContext("Failed to read file")
                .WithContext("Failed to load configuration")
                .WithContext("Application startup failed");

            Assert.Equal("Application startup failed", error.Message);
            Assert.Equal("Failed to load configuration", error.Source!.Message);
            Assert.Equal("Failed to read file", error.Source.Source!.Message);
            Assert.Equal("File not found", error.Source.Source.Source!.Message);
        }

        [Fact]
        public void WithContext_WithNullMessage_ThrowsArgumentNullException()
        {
            var error = Error.New("Test");
            Assert.Throws<ArgumentNullException>(() => error.WithContext(null!));
        }

        #endregion

        #region WithMetadata Tests

        [Fact]
        public void WithMetadata_AttachesMetadataToError()
        {
            var error = Error.New("Operation failed")
                .WithMetadata("userId", 123)
                .WithMetadata("timestamp", DateTime.UtcNow);

            Assert.True(error.TryGetMetadata("userId", out var userId));
            Assert.Equal(123, userId);

            Assert.True(error.TryGetMetadata("timestamp", out var timestamp));
            Assert.IsType<DateTime>(timestamp);
        }

        [Fact]
        public void WithMetadata_WithNullKey_ThrowsArgumentNullException()
        {
            var error = Error.New("Test");
            Assert.Throws<ArgumentNullException>(() => error.WithMetadata(null!, "value"));
        }

        [Fact]
        public void WithMetadata_WithNullValue_ThrowsArgumentNullException()
        {
            var error = Error.New("Test");
            Assert.Throws<ArgumentNullException>(() => error.WithMetadata("key", null!));
        }

        [Fact]
        public void WithMetadata_WithInvalidType_ThrowsArgumentException()
        {
            var error = Error.New("Test");
            var invalidValue = new System.Net.Http.HttpClient();
            
            var exception = Assert.Throws<ArgumentException>(() => error.WithMetadata("key", invalidValue));
            Assert.Contains("not suitable for metadata", exception.Message);
        }

        [Theory]
        [InlineData(42)]
        [InlineData("test")]
        [InlineData(3.14)]
        [InlineData(true)]
        public void WithMetadata_WithValidPrimitiveTypes_Succeeds(object value)
        {
            var error = Error.New("Test").WithMetadata("key", value);

            Assert.True(error.TryGetMetadata("key", out var retrievedValue));
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void WithMetadata_WithDateTime_Succeeds()
        {
            var now = DateTime.UtcNow;
            var error = Error.New("Test").WithMetadata("timestamp", now);

            Assert.True(error.TryGetMetadata("timestamp", out var value));
            Assert.Equal(now, value);
        }

        [Fact]
        public void WithMetadata_WithGuid_Succeeds()
        {
            var guid = Guid.NewGuid();
            var error = Error.New("Test").WithMetadata("id", guid);

            Assert.True(error.TryGetMetadata("id", out var value));
            Assert.Equal(guid, value);
        }

        [Fact]
        public void WithMetadata_WithEnum_Succeeds()
        {
            var error = Error.New("Test").WithMetadata("kind", ErrorKind.NotFound);

            Assert.True(error.TryGetMetadata("kind", out var value));
            Assert.Equal(ErrorKind.NotFound, value);
        }

        #endregion

        #region Type-Safe Metadata Tests

        [Fact]
        public void WithMetadata_TypeSafeOverload_WorksWithValueTypes()
        {
            var error = Error.New("Test")
                .WithMetadata("count", 42)
                .WithMetadata("isActive", true)
                .WithMetadata("percentage", 95.5);

            Assert.True(error.TryGetMetadata("count", out int count));
            Assert.Equal(42, count);

            Assert.True(error.TryGetMetadata("isActive", out bool isActive));
            Assert.True(isActive);

            Assert.True(error.TryGetMetadata("percentage", out double percentage));
            Assert.Equal(95.5, percentage);
        }

        [Fact]
        public void TryGetMetadata_TypeSafe_WithCorrectType_ReturnsValue()
        {
            var error = Error.New("Test").WithMetadata("userId", 123);

            Assert.True(error.TryGetMetadata("userId", out int userId));
            Assert.Equal(123, userId);
        }

        [Fact]
        public void TryGetMetadata_TypeSafe_WithIncorrectType_ReturnsFalse()
        {
            var error = Error.New("Test").WithMetadata("userId", 123);

            Assert.False(error.TryGetMetadata("userId", out string? userId));
            Assert.Null(userId);
        }

        [Fact]
        public void TryGetMetadata_TypeSafe_WithNonExistentKey_ReturnsFalse()
        {
            var error = Error.New("Test");

            Assert.False(error.TryGetMetadata("nonexistent", out int value));
            Assert.Equal(0, value);
        }

        [Fact]
        public void WithMetadata_TypeSafe_WithDateTime_Succeeds()
        {
            var now = DateTime.UtcNow;
            var error = Error.New("Test").WithMetadata("timestamp", now);

            Assert.True(error.TryGetMetadata("timestamp", out DateTime timestamp));
            Assert.Equal(now, timestamp);
        }

        [Fact]
        public void WithMetadata_TypeSafe_WithGuid_Succeeds()
        {
            var guid = Guid.NewGuid();
            var error = Error.New("Test").WithMetadata("requestId", guid);

            Assert.True(error.TryGetMetadata("requestId", out Guid requestId));
            Assert.Equal(guid, requestId);
        }

        #endregion

        #region WithKind Tests

        [Fact]
        public void WithKind_ChangesErrorKind()
        {
            var error = Error.New("Error", ErrorKind.Other)
                .WithKind(ErrorKind.Timeout);

            Assert.Equal(ErrorKind.Timeout, error.Kind);
            Assert.Equal("Error", error.Message);
        }

        [Fact]
        public void WithKind_PreservesOtherProperties()
        {
            var error = Error.New("Error")
                .WithMetadata("key", "value")
                .WithKind(ErrorKind.NotFound);

            Assert.Equal(ErrorKind.NotFound, error.Kind);
            Assert.True(error.TryGetMetadata("key", out var value));
            Assert.Equal("value", value);
        }

        #endregion

        #region CaptureStackTrace Tests

        [Fact]
        public void CaptureStackTrace_AttachesStackTrace()
        {
            var error = Error.New("Test").CaptureStackTrace(includeFileInfo: false);

            Assert.NotNull(error.StackTrace);
            Assert.Contains("ErrorTests", error.StackTrace);
        }

        [Fact]
        public void CaptureStackTrace_WithFileInfo_AttachesDetailedStackTrace()
        {
            var error = Error.New("Test").CaptureStackTrace(includeFileInfo: true);

            Assert.NotNull(error.StackTrace);
            Assert.Contains("ErrorTests", error.StackTrace);
        }

        [Fact]
        public void CaptureStackTrace_DefaultsToNoFileInfo()
        {
            var error = Error.New("Test").CaptureStackTrace();

            Assert.NotNull(error.StackTrace);
            Assert.Contains("ErrorTests", error.StackTrace);
        }

        #endregion

        #region TryGetMetadata Tests

        [Fact]
        public void TryGetMetadata_WithExistingKey_ReturnsTrue()
        {
            var error = Error.New("Test").WithMetadata("key", "value");

            Assert.True(error.TryGetMetadata("key", out var value));
            Assert.Equal("value", value);
        }

        [Fact]
        public void TryGetMetadata_WithNonExistentKey_ReturnsFalse()
        {
            var error = Error.New("Test");

            Assert.False(error.TryGetMetadata("key", out var value));
            Assert.Null(value);
        }

        [Fact]
        public void TryGetMetadata_WithNullKey_ThrowsArgumentNullException()
        {
            var error = Error.New("Test");
            Assert.Throws<ArgumentNullException>(() => error.TryGetMetadata(null!, out var value));
        }

        #endregion

        #region GetFullMessage Tests

        [Fact]
        public void GetFullMessage_WithoutSource_ReturnsSimpleMessage()
        {
            var error = Error.New("Test error", ErrorKind.NotFound);

            var message = error.GetFullMessage();

            Assert.Contains("NotFound: Test error", message);
        }

        [Fact]
        public void GetFullMessage_WithSource_ReturnsChainedMessage()
        {
            var error = Error.New("Base error")
                .WithContext("Step 1 failed")
                .WithContext("Step 2 failed");

            var message = error.GetFullMessage();

            Assert.Contains("Step 2 failed", message);
            Assert.Contains("Step 1 failed", message);
            Assert.Contains("Base error", message);
            Assert.Contains("Caused by:", message);
        }

        [Fact]
        public void GetFullMessage_WithMetadata_IncludesMetadata()
        {
            var error = Error.New("Error")
                .WithMetadata("userId", 123)
                .WithMetadata("action", "delete");

            var message = error.GetFullMessage();

            Assert.Contains("userId=123", message);
            Assert.Contains("action=delete", message);
        }

        [Fact]
        public void GetFullMessage_WithDeepErrorChain_TruncatesAtMaxDepth()
        {
            var error = Error.New("Base error");
            
            for (int i = 0; i < 55; i++)
            {
                error = error.WithContext($"Context level {i}");
            }

            var fullMessage = error.GetFullMessage();

            Assert.Contains("error chain truncated at depth 50", fullMessage);
            Assert.Contains("Context level 54", fullMessage);
            Assert.DoesNotContain("Base error", fullMessage);
        }

        [Fact]
        public void GetFullMessage_ProtectsAgainstCircularReferences()
        {
            var error = Error.New("Base error");
            for (int i = 0; i < 10; i++)
            {
                error = error.WithContext($"Level {i}");
            }

            var fullMessage = error.GetFullMessage();
            
            Assert.Contains("Base error", fullMessage);
            Assert.Contains("Level 0", fullMessage);
            Assert.Contains("Level 9", fullMessage);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_WithoutSource_ReturnsSimpleFormat()
        {
            var error = Error.New("Test error", ErrorKind.NotFound);

            var str = error.ToString();

            Assert.Equal("NotFound: Test error", str);
        }

        [Fact]
        public void ToString_WithSource_ReturnsFullMessage()
        {
            var error = Error.New("Base error")
                .WithContext("Context error");

            var str = error.ToString();

            Assert.Contains("Context error", str);
            Assert.Contains("Base error", str);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameMessageAndKind_ReturnsTrue()
        {
            var error1 = Error.New("Test", ErrorKind.NotFound);
            var error2 = Error.New("Test", ErrorKind.NotFound);

            Assert.Equal(error1, error2);
            Assert.True(error1.Equals(error2));
            Assert.True(error1 == error2);
            Assert.False(error1 != error2);
        }

        [Fact]
        public void Equals_WithDifferentMessage_ReturnsFalse()
        {
            var error1 = Error.New("Test 1");
            var error2 = Error.New("Test 2");

            Assert.NotEqual(error1, error2);
            Assert.False(error1.Equals(error2));
        }

        [Fact]
        public void Equals_WithDifferentKind_ReturnsFalse()
        {
            var error1 = Error.New("Test", ErrorKind.NotFound);
            var error2 = Error.New("Test", ErrorKind.Timeout);

            Assert.NotEqual(error1, error2);
        }

        [Fact]
        public void Equals_WithNull_ReturnsFalse()
        {
            var error = Error.New("Test");

            Assert.False(error.Equals(null));
            Assert.True(error != null);
        }

        [Fact]
        public void Equals_WithSameReference_ReturnsTrue()
        {
            var error = Error.New("Test");

            Assert.True(error.Equals(error));
            Assert.True(error == error);
        }

        #endregion

        #region GetHashCode Tests

        [Fact]
        public void GetHashCode_IsConsistentForEqualErrors()
        {
            var error1 = Error.New("Test", ErrorKind.NotFound);
            var error2 = Error.New("Test", ErrorKind.NotFound);

            Assert.Equal(error1.GetHashCode(), error2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_IsDifferentForDifferentErrors()
        {
            var error1 = Error.New("Test 1");
            var error2 = Error.New("Test 2");

            Assert.NotEqual(error1.GetHashCode(), error2.GetHashCode());
        }

        #endregion

        #region Implicit Conversion Tests

        [Fact]
        public void ImplicitConversion_FromString_CreatesError()
        {
            Error error = "Test error";

            Assert.Equal("Test error", error.Message);
            Assert.Equal(ErrorKind.Other, error.Kind);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ComplexErrorScenario_WorksCorrectly()
        {
            var error = Error.FromException(new FileNotFoundException("config.json not found"))
                .WithContext("Failed to load application configuration")
                .WithMetadata("configPath", "/etc/myapp/config.json")
                .WithMetadata("attemptCount", 3)
                .WithKind(ErrorKind.NotFound)
                .CaptureStackTrace(includeFileInfo: false);

            Assert.Equal(ErrorKind.NotFound, error.Kind);
            Assert.Contains("Failed to load application configuration", error.Message);
            Assert.NotNull(error.Source);
            Assert.NotNull(error.StackTrace);

            Assert.True(error.TryGetMetadata("configPath", out var path));
            Assert.Equal("/etc/myapp/config.json", path);

            var fullMessage = error.GetFullMessage();
            Assert.Contains("configPath=/etc/myapp/config.json", fullMessage);
            Assert.Contains("attemptCount=3", fullMessage);
        }

        [Fact]
        public void WithMetadata_MultipleCalls_UsesImmutableDictionary()
        {
            var error = Error.New("Test");
            
            for (int i = 0; i < 10; i++)
            {
                error = error.WithMetadata($"key{i}", i);
            }

            for (int i = 0; i < 10; i++)
            {
                Assert.True(error.TryGetMetadata($"key{i}", out var value));
                Assert.Equal(i, value);
            }
        }

        #endregion
    }
}
