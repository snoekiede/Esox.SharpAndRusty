using Esox.SharpAndRusty.Sync;
using Esox.SharpAndRusty.Types;
using Xunit;

namespace Esox.SharpAndRust.Tests.Async
{
    public class MutexTests
    {
        #region Basic Operations

        [Fact]
        public void Mutex_Creation_InitializesWithValue()
        {
            // Arrange & Act
            var mutex = new Mutex<int>(42);

            // Assert
            Assert.False(mutex.IsDisposed);
        }

        [Fact]
        public void Lock_AcquiresLockSuccessfully()
        {
            // Arrange
            var mutex = new Mutex<int>(42);

            // Act
            var result = mutex.Lock();

            // Assert
            Assert.True(result.IsSuccess);
            if (result.TryGetValue(out var guard))
            {
                Assert.Equal(42, guard.Value);
                guard.Dispose();
            }
        }

        [Fact]
        public void Lock_CanModifyValue()
        {
            // Arrange
            var mutex = new Mutex<int>(0);

            // Act
            var result = mutex.Lock();
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    guard.Value = 42;
                }
            }

            // Verify modification persisted
            var result2 = mutex.Lock();
            if (result2.TryGetValue(out var guard2))
            {
                using (guard2)
                {
                    Assert.Equal(42, guard2.Value);
                }
            }
        }

        [Fact]
        public void Lock_OnDisposedMutex_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            mutex.Dispose();

            // Act
            var result = mutex.Lock();

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
                Assert.Contains("disposed", error.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion

        #region TryLock Tests

        [Fact]
        public void TryLock_WhenUnlocked_AcquiresLock()
        {
            // Arrange
            var mutex = new Mutex<int>(42);

            // Act
            var result = mutex.TryLock();

            // Assert
            Assert.True(result.IsSuccess);
            if (result.TryGetValue(out var guard))
            {
                Assert.Equal(42, guard.Value);
                guard.Dispose();
            }
        }

        [Fact]
        public void TryLock_WhenLocked_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var guard1 = mutex.Lock();

            // Act
            var result = mutex.TryLock();

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.ResourceExhausted, error.Kind);
                Assert.Contains("locked", error.Message, StringComparison.OrdinalIgnoreCase);
            }

            // Cleanup
            if (guard1.TryGetValue(out var g1)) g1.Dispose();
        }

        [Fact]
        public void TryLock_OnDisposedMutex_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            mutex.Dispose();

            // Act
            var result = mutex.TryLock();

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
            }
        }

        #endregion

        #region TryLockTimeout Tests

        [Fact]
        public void TryLockTimeout_WhenUnlocked_AcquiresImmediately()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var timeout = TimeSpan.FromSeconds(1);

            // Act
            var result = mutex.TryLockTimeout(timeout);

            // Assert
            Assert.True(result.IsSuccess);
            if (result.TryGetValue(out var guard))
            {
                Assert.Equal(42, guard.Value);
                guard.Dispose();
            }
        }

        [Fact]
        public void TryLockTimeout_WhenLockedAndTimeoutExpires_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var guard1 = mutex.Lock();
            var timeout = TimeSpan.FromMilliseconds(100);

            // Act
            var result = mutex.TryLockTimeout(timeout);

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.Timeout, error.Kind);
                Assert.True(error.TryGetMetadata("timeout", out TimeSpan timeoutMeta));
                Assert.Equal(timeout, timeoutMeta);
            }

            // Cleanup
            if (guard1.TryGetValue(out var g1)) g1.Dispose();
        }

        [Fact]
        public void TryLockTimeout_WhenLockReleasedBeforeTimeout_AcquiresLock()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var timeout = TimeSpan.FromSeconds(2);

            var guard1Result = mutex.Lock();
            
            // Release lock after short delay
            Task.Run(async () =>
            {
                await Task.Delay(100);
                if (guard1Result.TryGetValue(out var g)) g.Dispose();
            });

            // Act
            var result = mutex.TryLockTimeout(timeout);

            // Assert
            Assert.True(result.IsSuccess);
            if (result.TryGetValue(out var guard))
            {
                guard.Dispose();
            }
        }

        #endregion

        #region Async Lock Tests

        [Fact]
        public async Task LockAsync_AcquiresLockSuccessfully()
        {
            // Arrange
            var mutex = new Mutex<int>(42);

            // Act
            var result = await mutex.LockAsync();

            // Assert
            Assert.True(result.IsSuccess);
            if (result.TryGetValue(out var guard))
            {
                Assert.Equal(42, guard.Value);
                guard.Dispose();
            }
        }

        [Fact]
        public async Task LockAsync_CanModifyValue()
        {
            // Arrange
            var mutex = new Mutex<int>(0);

            // Act
            var result = await mutex.LockAsync();
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    guard.Value = 99;
                }
            }

            // Verify
            var result2 = await mutex.LockAsync();
            if (result2.TryGetValue(out var guard2))
            {
                using (guard2)
                {
                    Assert.Equal(99, guard2.Value);
                }
            }
        }

        [Fact]
        public async Task LockAsync_WithCancellation_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await mutex.LockAsync(cts.Token);

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.Interrupted, error.Kind);
            }
        }

        [Fact]
        public async Task LockAsync_OnDisposedMutex_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            mutex.Dispose();

            // Act
            var result = await mutex.LockAsync();

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
            }
        }

        #endregion

        #region LockAsyncTimeout Tests

        [Fact]
        public async Task LockAsyncTimeout_WhenUnlocked_AcquiresImmediately()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var timeout = TimeSpan.FromSeconds(1);

            // Act
            var result = await mutex.LockAsyncTimeout(timeout);

            // Assert
            Assert.True(result.IsSuccess);
            if (result.TryGetValue(out var guard))
            {
                Assert.Equal(42, guard.Value);
                guard.Dispose();
            }
        }

        [Fact]
        public async Task LockAsyncTimeout_WhenLockedAndTimeoutExpires_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var guard1 = await mutex.LockAsync();
            var timeout = TimeSpan.FromMilliseconds(100);

            // Act
            var result = await mutex.LockAsyncTimeout(timeout);

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.Timeout, error.Kind);
            }

            // Cleanup
            if (guard1.TryGetValue(out var g1)) g1.Dispose();
        }

        [Fact]
        public async Task LockAsyncTimeout_WithCancellation_ReturnsError()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var timeout = TimeSpan.FromSeconds(10);

            // Act
            var result = await mutex.LockAsyncTimeout(timeout, cts.Token);

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.Interrupted, error.Kind);
            }
        }

        #endregion

        #region MutexGuard Tests

        [Fact]
        public void MutexGuard_Value_CanBeRead()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result = mutex.Lock();

            // Act & Assert
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    Assert.Equal(42, guard.Value);
                }
            }
        }

        [Fact]
        public void MutexGuard_Value_CanBeModified()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result = mutex.Lock();

            // Act
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    guard.Value = 100;
                    Assert.Equal(100, guard.Value);
                }
            }
        }

        [Fact]
        public void MutexGuard_Map_TransformsValue()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result = mutex.Lock();

            // Act & Assert
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    var mapped = guard.Map(x => $"Value is {x}");
                    Assert.Equal("Value is 42", mapped);
                }
            }
        }

        [Fact]
        public void MutexGuard_Update_ModifiesValue()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result = mutex.Lock();

            // Act
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    guard.Update(x => x * 2);
                    Assert.Equal(84, guard.Value);
                }
            }

            // Verify persistence
            var result2 = mutex.Lock();
            if (result2.TryGetValue(out var guard2))
            {
                using (guard2)
                {
                    Assert.Equal(84, guard2.Value);
                }
            }
        }

        [Fact]
        public void MutexGuard_Dispose_ReleasesLock()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result1 = mutex.Lock();

            // Act
            if (result1.TryGetValue(out var guard1))
            {
                guard1.Dispose();
            }

            // Assert - should be able to lock again immediately
            var result2 = mutex.TryLock();
            Assert.True(result2.IsSuccess);
            if (result2.TryGetValue(out var guard2))
            {
                guard2.Dispose();
            }
        }

        [Fact]
        public void MutexGuard_AccessAfterDispose_ThrowsException()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result = mutex.Lock();
            MutexGuard<int>? guard = null;
            
            if (result.TryGetValue(out var g))
            {
                guard = g;
                guard.Dispose();
            }

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => guard!.Value);
        }

        [Fact]
        public void MutexGuard_Map_WithNullMapper_ThrowsArgumentNullException()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result = mutex.Lock();

            // Act & Assert
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    Assert.Throws<ArgumentNullException>(() => guard.Map<string>(null!));
                }
            }
        }

        [Fact]
        public void MutexGuard_Update_WithNullUpdater_ThrowsArgumentNullException()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            var result = mutex.Lock();

            // Act & Assert
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    Assert.Throws<ArgumentNullException>(() => guard.Update(null!));
                }
            }
        }

        #endregion

        #region IntoInner Tests

        [Fact]
        public void IntoInner_ExtractsValueAndDisposesMutex()
        {
            // Arrange
            var mutex = new Mutex<int>(42);

            // Act
            var value = mutex.IntoInner();

            // Assert
            Assert.Equal(42, value);
            Assert.True(mutex.IsDisposed);
        }

        [Fact]
        public void IntoInner_OnDisposedMutex_ThrowsException()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            mutex.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => mutex.IntoInner());
        }

        [Fact]
        public void IntoInner_AfterCall_MutexCannotBeLocked()
        {
            // Arrange
            var mutex = new Mutex<int>(42);
            _ = mutex.IntoInner();

            // Act
            var result = mutex.Lock();

            // Assert
            Assert.True(result.IsFailure);
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task Mutex_MultipleTasks_SerialAccess()
        {
            // Arrange
            var mutex = new Mutex<int>(0);
            var taskCount = 100;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < taskCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var result = await mutex.LockAsync();
                    if (result.TryGetValue(out var guard))
                    {
                        using (guard)
                        {
                            var current = guard.Value;
                            await Task.Delay(1); // Simulate work
                            guard.Value = current + 1;
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            var finalResult = await mutex.LockAsync();
            if (finalResult.TryGetValue(out var finalGuard))
            {
                using (finalGuard)
                {
                    Assert.Equal(taskCount, finalGuard.Value);
                }
            }
        }

        [Fact]
        public async Task Mutex_ConcurrentTryLock_OnlyOneSucceeds()
        {
            // Arrange
            var mutex = new Mutex<int>(0);
            var successCount = 0;

            // Act
            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
            {
                var result = mutex.TryLock();
                if (result.IsSuccess)
                {
                    Interlocked.Increment(ref successCount);
                    if (result.TryGetValue(out var guard))
                    {
                        Thread.Sleep(100); // Hold lock
                        guard.Dispose();
                    }
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert - only one task should have acquired the lock
            Assert.Equal(1, successCount);
        }

        [Fact]
        public async Task Mutex_StressTest_MaintainsConsistency()
        {
            // Arrange
            var mutex = new Mutex<List<int>>(new List<int>());
            var iterations = 50;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var result = await mutex.LockAsync();
                    if (result.TryGetValue(out var guard))
                    {
                        using (guard)
                        {
                            var list = guard.Value;
                            list.Add(index);
                            guard.Value = list;
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            var finalResult = await mutex.LockAsync();
            if (finalResult.TryGetValue(out var finalGuard))
            {
                using (finalGuard)
                {
                    Assert.Equal(iterations, finalGuard.Value.Count);
                    Assert.Equal(iterations, finalGuard.Value.Distinct().Count());
                }
            }
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_DisposesSuccessfully()
        {
            // Arrange
            var mutex = new Mutex<int>(42);

            // Act
            mutex.Dispose();

            // Assert
            Assert.True(mutex.IsDisposed);
        }

        [Fact]
        public void Dispose_MultipleCalls_IsSafe()
        {
            // Arrange
            var mutex = new Mutex<int>(42);

            // Act & Assert - should not throw
            mutex.Dispose();
            mutex.Dispose();
            mutex.Dispose();

            Assert.True(mutex.IsDisposed);
        }

        [Fact]
        public void Dispose_WithUsingStatement_ReleasesResources()
        {
            // Arrange & Act
            Mutex<int>? mutex = null;
            using (mutex = new Mutex<int>(42))
            {
                Assert.False(mutex.IsDisposed);
            }

            // Assert
            Assert.True(mutex.IsDisposed);
        }

        #endregion

        #region Complex Scenarios

        [Fact]
        public void Mutex_WithComplexType_WorksCorrectly()
        {
            // Arrange
            var mutex = new Mutex<Dictionary<string, int>>(new Dictionary<string, int>());

            // Act
            var result = mutex.Lock();
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    guard.Value["key1"] = 42;
                    guard.Value["key2"] = 99;
                }
            }

            // Assert
            var result2 = mutex.Lock();
            if (result2.TryGetValue(out var guard2))
            {
                using (guard2)
                {
                    Assert.Equal(2, guard2.Value.Count);
                    Assert.Equal(42, guard2.Value["key1"]);
                    Assert.Equal(99, guard2.Value["key2"]);
                }
            }
        }

        [Fact]
        public async Task Mutex_MixedSyncAndAsync_WorksCorrectly()
        {
            // Arrange
            var mutex = new Mutex<int>(0);
            var syncResult = mutex.Lock();

            // Act & Assert - async attempt should wait
            var asyncTask = Task.Run(async () =>
            {
                var result = await mutex.LockAsync();
                if (result.TryGetValue(out var guard))
                {
                    using (guard)
                    {
                        guard.Value = 100;
                    }
                }
            });

            // Release sync lock after delay
            await Task.Delay(100);
            if (syncResult.TryGetValue(out var syncGuard))
            {
                syncGuard.Dispose();
            }

            await asyncTask;

            // Verify
            var finalResult = mutex.Lock();
            if (finalResult.TryGetValue(out var finalGuard))
            {
                using (finalGuard)
                {
                    Assert.Equal(100, finalGuard.Value);
                }
            }
        }

        #endregion
    }
}
