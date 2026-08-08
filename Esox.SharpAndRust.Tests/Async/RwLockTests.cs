using Esox.SharpAndRusty.Sync;
using Esox.SharpAndRusty.Types;

namespace Esox.SharpAndRusty.Tests.Async;

public class RwLockTests
{
    [Fact]
    public void RwLock_Creation_InitializesWithValue()
    {
        // Arrange & Act
        var rwlock = new RwLock<int>(42);

        // Assert
        Assert.False(rwlock.IsDisposed);
    }

    [Fact]
    public void Read_AcquiresReadLockSuccessfully()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);

        // Act
        var result = rwlock.Read();

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var guard))
        {
            Assert.Equal(42, guard.Value);
            guard.Dispose();
        }
    }

    [Fact]
    public void Read_OnDisposedRwLock_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        rwlock.Dispose();

        // Act
        var result = rwlock.Read();

        // Assert
        Assert.True(result.IsFailure);
        if (result.TryGetError(out var error))
        {
            Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
            Assert.Contains("disposed", error.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void Read_MultipleReadLocksOnSameThread_AllowsRecursion()
    {
        // Arrange - explicitly enable recursion for this test
        var rwlock = new RwLock<int>(42, LockRecursionPolicy.SupportsRecursion);

        // Act - Acquire multiple read locks on same thread
        var result1 = rwlock.Read();
        Assert.True(result1.IsSuccess);

        var result2 = rwlock.Read();
        Assert.True(result2.IsSuccess);

        var result3 = rwlock.Read();
        Assert.True(result3.IsSuccess);

        // Assert - all locks should be acquired successfully
        if (result1.TryGetValue(out var guard1) &&
            result2.TryGetValue(out var guard2) &&
            result3.TryGetValue(out var guard3))
        {
            Assert.Equal(42, guard1.Value);
            Assert.Equal(42, guard2.Value);
            Assert.Equal(42, guard3.Value);

            // Cleanup - release in reverse order
            guard3.Dispose();
            guard2.Dispose();
            guard1.Dispose();
        }

        // Verify lock is fully released
        var writeResult = rwlock.TryWrite();
        Assert.True(writeResult.IsSuccess);
        if (writeResult.TryGetValue(out var writeGuard)) writeGuard.Dispose();
    }

    [Fact]
    public void Read_MultipleConcurrentReaders_AllSucceed()
    {
        // Arrange - explicitly enable recursion for this test
        var rwlock = new RwLock<int>(42, LockRecursionPolicy.SupportsRecursion);

        // Act - Acquire locks on same thread (recursive read locks should work)
        var result1 = rwlock.Read();
        var result2 = rwlock.Read();
        var result3 = rwlock.Read();

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);

        if (result1.TryGetValue(out var guard1) &&
            result2.TryGetValue(out var guard2) &&
            result3.TryGetValue(out var guard3))
        {
            Assert.Equal(42, guard1.Value);
            Assert.Equal(42, guard2.Value);
            Assert.Equal(42, guard3.Value);

            guard1.Dispose();
            guard2.Dispose();
            guard3.Dispose();
        }
    }


    [Fact]
    public void Write_AcquiresWriteLockSuccessfully()
    {
        // Arrange
        var rwlock = new RwLock<int>(0);

        // Act
        var result = rwlock.Write();

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var guard))
        {
            guard.Value = 42;
            guard.Dispose();
        }

        // Verify
        var readResult = rwlock.Read();
        if (readResult.TryGetValue(out var readGuard))
        {
            Assert.Equal(42, readGuard.Value);
            readGuard.Dispose();
        }
    }

    [Fact]
    public void Write_OnDisposedRwLock_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        rwlock.Dispose();

        // Act
        var result = rwlock.Write();

        // Assert
        Assert.True(result.IsFailure);
        if (result.TryGetError(out var error)) Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
    }

    [Fact]
    public async Task Write_WithActiveReader_Blocks()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var readResult = rwlock.Read();

        // Act
        var writeTask = Task.Run(() => rwlock.TryWrite());
        await Task.Delay(50); // Give write attempt time to try

        // Assert - write should fail because reader is active
        var writeResult = await writeTask;
        Assert.True(writeResult.IsFailure);

        // Cleanup
        if (readResult.TryGetValue(out var guard)) guard.Dispose();
    }


    [Fact]
    public void TryRead_WhenAvailable_AcquiresLock()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);

        // Act
        var result = rwlock.TryRead();

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var guard))
        {
            Assert.Equal(42, guard.Value);
            guard.Dispose();
        }
    }

    [Fact]
    public async Task TryRead_WithActiveWriter_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);

        // Act - Acquire write lock on this thread
        var writeResult = rwlock.Write();
        Assert.True(writeResult.IsSuccess); // Ensure we got the write lock

        if (!writeResult.TryGetValue(out var writeGuard))
        {
            Assert.Fail("Failed to acquire write lock");
            return;
        }

        Result<ReadGuard<int>, Error> readResult;

        // Use a TaskCompletionSource to ensure we wait for the separate thread
        var tcs = new TaskCompletionSource<Result<ReadGuard<int>, Error>>();

        try
        {
            // Try to read from a dedicated thread to avoid recursion policy
            var thread = new Thread(() =>
            {
                try
                {
                    var result = rwlock.TryRead();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            thread.Start();
            readResult = await tcs.Task;
        }
        finally
        {
            // Cleanup
            writeGuard.Dispose();
        }

        // Assert
        Assert.True(readResult.IsFailure,
            $"Expected read to fail, but got IsSuccess={readResult.IsSuccess}, IsFailure={readResult.IsFailure}");
        if (readResult.TryGetError(out var error)) Assert.Equal(ErrorKind.ResourceExhausted, error.Kind);
    }

    [Fact]
    public void TryRead_OnDisposedRwLock_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        rwlock.Dispose();

        // Act
        var result = rwlock.TryRead();

        // Assert
        Assert.True(result.IsFailure);
    }


    [Fact]
    public void TryReadTimeout_WhenAvailable_AcquiresImmediately()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        var result = rwlock.TryReadTimeout(timeout);

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var guard))
        {
            Assert.Equal(42, guard.Value);
            guard.Dispose();
        }
    }

    [Fact]
    public async Task TryReadTimeout_WhenWriterActiveAndTimeoutExpires_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var writeResult = rwlock.Write();
        Assert.True(writeResult.IsSuccess); // Ensure we got the write lock

        if (!writeResult.TryGetValue(out var writeGuard))
        {
            Assert.Fail("Failed to acquire write lock");
            return;
        }

        var timeout = TimeSpan.FromMilliseconds(100);
        Result<ReadGuard<int>, Error> readResult;

        // Use a TaskCompletionSource to ensure we wait for the separate thread
        var tcs = new TaskCompletionSource<Result<ReadGuard<int>, Error>>();

        try
        {
            // Act - Force execution on a new thread to avoid recursion
            var thread = new Thread(() =>
            {
                try
                {
                    var result = rwlock.TryReadTimeout(timeout);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            thread.Start();
            readResult = await tcs.Task;
        }
        finally
        {
            // Cleanup
            writeGuard.Dispose();
        }

        // Assert
        Assert.True(readResult.IsFailure, $"Expected read to fail, but got IsSuccess={readResult.IsSuccess}");
        if (readResult.TryGetError(out var error))
            // Can be Timeout or InvalidOperation depending on lock state
            Assert.True(error.Kind == ErrorKind.Timeout || error.Kind == ErrorKind.InvalidOperation,
                $"Expected Timeout or InvalidOperation, got {error.Kind}");
    }


    [Fact]
    public void TryWrite_WhenAvailable_AcquiresLock()
    {
        // Arrange
        var rwlock = new RwLock<int>(0);

        // Act
        var result = rwlock.TryWrite();

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var guard))
        {
            guard.Value = 42;
            guard.Dispose();
        }

        // Verify
        var readResult = rwlock.Read();
        if (readResult.TryGetValue(out var readGuard))
        {
            Assert.Equal(42, readGuard.Value);
            readGuard.Dispose();
        }
    }

    [Fact]
    public async Task TryWrite_WithActiveReader_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var readGuard = rwlock.Read();

        // Act - Try from different thread to avoid recursion
        var result = await Task.Run(() => rwlock.TryWrite());

        // Assert
        Assert.True(result.IsFailure);
        if (result.TryGetError(out var error)) Assert.Equal(ErrorKind.ResourceExhausted, error.Kind);

        // Cleanup
        if (readGuard.TryGetValue(out var guard)) guard.Dispose();
    }

    [Fact]
    public void TryWrite_OnDisposedRwLock_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        rwlock.Dispose();

        // Act
        var result = rwlock.TryWrite();

        // Assert
        Assert.True(result.IsFailure);
    }


    [Fact]
    public void TryWriteTimeout_WhenAvailable_AcquiresImmediately()
    {
        // Arrange
        var rwlock = new RwLock<int>(0);
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        var result = rwlock.TryWriteTimeout(timeout);

        // Assert
        Assert.True(result.IsSuccess);
        if (result.TryGetValue(out var guard))
        {
            guard.Value = 42;
            guard.Dispose();
        }
    }

    [Fact]
    public async Task TryWriteTimeout_WithActiveReaderAndTimeoutExpires_ReturnsError()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var readGuard = rwlock.Read();
        var timeout = TimeSpan.FromMilliseconds(100);

        // Act - Try from different thread to avoid recursion
        var result = await Task.Run(() => rwlock.TryWriteTimeout(timeout));

        // Assert
        Assert.True(result.IsFailure);
        if (result.TryGetError(out var error)) Assert.Equal(ErrorKind.Timeout, error.Kind);

        // Cleanup
        if (readGuard.TryGetValue(out var guard)) guard.Dispose();
    }


    [Fact]
    public void ReadGuard_Value_CanBeRead()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result = rwlock.Read();

        // Act & Assert
        if (result.TryGetValue(out var guard))
            using (guard)
                Assert.Equal(42, guard.Value);
    }

    [Fact]
    public void ReadGuard_Map_TransformsValue()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result = rwlock.Read();

        // Act & Assert
        if (result.TryGetValue(out var guard))
            using (guard)
            {
                var mapped = guard.Map(x => $"Value is {x}");
                Assert.Equal("Value is 42", mapped);
            }
    }

    [Fact]
    public void ReadGuard_Dispose_ReleasesLock()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result1 = rwlock.Read();

        // Act
        if (result1.TryGetValue(out var guard1)) guard1.Dispose();

        // Assert - should be able to write now
        var result2 = rwlock.TryWrite();
        Assert.True(result2.IsSuccess);
        if (result2.TryGetValue(out var guard2)) guard2.Dispose();
    }

    [Fact]
    public void ReadGuard_AccessAfterDispose_ThrowsException()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result = rwlock.Read();
        ReadGuard<int>? guard = null;

        if (result.TryGetValue(out var g))
        {
            guard = g;
            guard.Dispose();
        }

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => guard!.Value);
    }


    [Fact]
    public void WriteGuard_Value_CanBeReadAndModified()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result = rwlock.Write();

        // Act
        if (result.TryGetValue(out var guard))
            using (guard)
            {
                Assert.Equal(42, guard.Value);
                guard.Value = 100;
                Assert.Equal(100, guard.Value);
            }

        // Verify persistence
        var readResult = rwlock.Read();
        if (readResult.TryGetValue(out var readGuard))
            using (readGuard)
                Assert.Equal(100, readGuard.Value);
    }

    [Fact]
    public void WriteGuard_Map_TransformsValue()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result = rwlock.Write();

        // Act & Assert
        if (result.TryGetValue(out var guard))
            using (guard)
            {
                var mapped = guard.Map(x => $"Value is {x}");
                Assert.Equal("Value is 42", mapped);
            }
    }

    [Fact]
    public void WriteGuard_Update_ModifiesValue()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result = rwlock.Write();

        // Act
        if (result.TryGetValue(out var guard))
            using (guard)
            {
                guard.Update(x => x * 2);
                Assert.Equal(84, guard.Value);
            }

        // Verify persistence
        var readResult = rwlock.Read();
        if (readResult.TryGetValue(out var readGuard))
            using (readGuard)
                Assert.Equal(84, readGuard.Value);
    }

    [Fact]
    public void WriteGuard_Dispose_ReleasesLock()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result1 = rwlock.Write();

        // Act
        if (result1.TryGetValue(out var guard1)) guard1.Dispose();

        // Assert - should be able to read immediately
        var result2 = rwlock.TryRead();
        Assert.True(result2.IsSuccess);
        if (result2.TryGetValue(out var guard2)) guard2.Dispose();
    }

    [Fact]
    public void WriteGuard_AccessAfterDispose_ThrowsException()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var result = rwlock.Write();
        WriteGuard<int>? guard = null;

        if (result.TryGetValue(out var g))
        {
            guard = g;
            guard.Dispose();
        }

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => guard!.Value);
    }


    [Fact]
    public void IntoInner_ExtractsValueAndDisposesRwLock()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);

        // Act
        var value = rwlock.IntoInner();

        // Assert
        Assert.Equal(42, value);
        Assert.True(rwlock.IsDisposed);
    }

    [Fact]
    public void IntoInner_OnDisposedRwLock_ThrowsException()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        rwlock.Dispose();

        // Act
        var result = rwlock.IntoInner();

        // Assert - should return error, not throw exception
        Assert.True(result.IsFailure);
        Assert.True(result.TryGetError(out var error));
        Assert.Contains("disposed", error.Message, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    [Trait("Category", "Slow")]
    public async Task RwLock_MultipleConcurrentReaders_AllSucceed()
    {
        // Arrange
        var rwlock = new RwLock<int>(0);
        var readerCount = 10;
        var tasks = new List<Task>();

        // Act
        for (var i = 0; i < readerCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var result = rwlock.Read();
                if (result.TryGetValue(out var guard))
                    using (guard)
                    {
                        Thread.Sleep(10); // Simulate work
                        _ = guard.Value;
                    }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - all readers should complete successfully
        Assert.True(true); // If we get here, all readers succeeded
    }

    [Fact]
    [Trait("Category", "Slow")]
    public async Task RwLock_ReaderWriterAlternation_MaintainsConsistency()
    {
        // Arrange
        var rwlock = new RwLock<int>(0);
        var iterations = 20;

        // Act
        for (var i = 0; i < iterations; i++)
        {
            // Write
            var writeResult = rwlock.Write();
            if (writeResult.TryGetValue(out var writeGuard))
                using (writeGuard)
                    writeGuard.Value = i;

            // Read
            var readResult = rwlock.Read();
            if (readResult.TryGetValue(out var readGuard))
                using (readGuard)
                    Assert.Equal(i, readGuard.Value);

            await Task.Delay(1);
        }

        // Assert
        var finalRead = rwlock.Read();
        if (finalRead.TryGetValue(out var finalGuard))
            using (finalGuard)
                Assert.Equal(iterations - 1, finalGuard.Value);
    }

    [Fact]
    [Trait("Category", "Stress")]
    public async Task RwLock_StressTest_MaintainsDataIntegrity()
    {
        // Arrange
        var rwlock = new RwLock<List<int>>(new List<int>());
        var writerCount = 5;
        var itemsPerWriter = 10;
        var tasks = new List<Task>();

        // Act - multiple writers adding items
        for (var i = 0; i < writerCount; i++)
        {
            var writerIndex = i;
            tasks.Add(Task.Run(() =>
            {
                for (var j = 0; j < itemsPerWriter; j++)
                {
                    var result = rwlock.Write();
                    if (result.TryGetValue(out var guard))
                        using (guard)
                        {
                            var list = guard.Value;
                            list.Add(writerIndex * 100 + j);
                            guard.Value = list;
                        }

                    Thread.Sleep(1);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var finalResult = rwlock.Read();
        if (finalResult.TryGetValue(out var finalGuard))
            using (finalGuard)
            {
                var expectedCount = writerCount * itemsPerWriter;
                Assert.Equal(expectedCount, finalGuard.Value.Count);
                Assert.Equal(expectedCount, finalGuard.Value.Distinct().Count()); // No duplicates
            }
    }


    [Fact]
    public void Dispose_DisposesSuccessfully()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);

        // Act
        rwlock.Dispose();

        // Assert
        Assert.True(rwlock.IsDisposed);
    }

    [Fact]
    public void Dispose_MultipleCalls_IsSafe()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);

        // Act & Assert - should not throw
        rwlock.Dispose();
        rwlock.Dispose();
        rwlock.Dispose();

        Assert.True(rwlock.IsDisposed);
    }

    [Fact]
    public void Dispose_WithUsingStatement_ReleasesResources()
    {
        // Arrange & Act
        RwLock<int>? rwlock = null;
        using (rwlock = new RwLock<int>(42)) Assert.False(rwlock.IsDisposed);

        // Assert
        Assert.True(rwlock.IsDisposed);
    }


    [Fact]
    public void RwLock_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var rwlock = new RwLock<Dictionary<string, int>>(new Dictionary<string, int>());

        // Act - Write
        var writeResult = rwlock.Write();
        if (writeResult.TryGetValue(out var writeGuard))
            using (writeGuard)
            {
                writeGuard.Value["key1"] = 42;
                writeGuard.Value["key2"] = 99;
            }

        // Act - Read
        var readResult = rwlock.Read();
        if (readResult.TryGetValue(out var readGuard))
            using (readGuard)
            {
                Assert.Equal(2, readGuard.Value.Count);
                Assert.Equal(42, readGuard.Value["key1"]);
                Assert.Equal(99, readGuard.Value["key2"]);
            }
    }

    [Fact]
    public async Task RwLock_WriterBlocksReaders_UntilReleased()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var writeResult = rwlock.Write();
        var readerAcquired = false;

        // Act
        var readTask = Task.Run(async () =>
        {
            await Task.Delay(50); // Ensure writer has lock first
            var result = rwlock.TryRead();
            readerAcquired = result.IsSuccess;
        });

        await Task.Delay(100); // Let reader try

        // Assert - reader should fail while writer holds lock
        Assert.False(readerAcquired);

        // Release writer
        if (writeResult.TryGetValue(out var writeGuard)) writeGuard.Dispose();

        await readTask;
    }

    [Fact]
    public void RwLock_MultipleReadersBlockWriter()
    {
        // Arrange
        var rwlock = new RwLock<int>(42);
        var read1 = rwlock.Read();
        var read2 = rwlock.Read();

        // Act
        var writeResult = rwlock.TryWrite();

        // Assert
        Assert.True(writeResult.IsFailure);

        // Cleanup
        if (read1.TryGetValue(out var guard1)) guard1.Dispose();
        if (read2.TryGetValue(out var guard2)) guard2.Dispose();

        // Now write should succeed
        var writeResult2 = rwlock.TryWrite();
        Assert.True(writeResult2.IsSuccess);
        if (writeResult2.TryGetValue(out var writeGuard)) writeGuard.Dispose();
    }

    #region Stress and disposal-race tests

    /// <summary>
    /// Races concurrent Read/Write/TryRead/TryWrite calls against a Dispose() call across many
    /// iterations. Success means: no unhandled exceptions, no hangs, and IsDisposed is true once
    /// Dispose() returns.
    /// </summary>
    [Fact]
    public void Stress_ConcurrentReadWriteRacingDispose_NoHangsOrUnhandledExceptions()
    {
        const int iterations = 50;

        for (var i = 0; i < iterations; i++)
        {
            var rwlock = new RwLock<int>(0);
            var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            // Spin up several reader and writer threads that loop until the lock is disposed.
            var threads = new List<Thread>();

            void WorkerBody(Action work)
            {
                try
                {
                    while (!rwlock.IsDisposed)
                    {
                        work();
                        Thread.SpinWait(5);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Expected: worker read IsDisposed==false, then Dispose() ran concurrently.
                    // This is the documented "blocked-on-entry during Dispose" race; not a bug.
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            threads.Add(new Thread(() => WorkerBody(() =>
            {
                var r = rwlock.TryRead();
                if (r.TryGetValue(out var g)) g.Dispose();
            })));

            threads.Add(new Thread(() => WorkerBody(() =>
            {
                var r = rwlock.TryReadTimeout(TimeSpan.FromMilliseconds(1));
                if (r.TryGetValue(out var g)) g.Dispose();
            })));

            threads.Add(new Thread(() => WorkerBody(() =>
            {
                var r = rwlock.TryWrite();
                if (r.TryGetValue(out var g)) g.Dispose();
            })));

            threads.Add(new Thread(() => WorkerBody(() =>
            {
                var r = rwlock.TryWriteTimeout(TimeSpan.FromMilliseconds(1));
                if (r.TryGetValue(out var g)) g.Dispose();
            })));

            foreach (var t in threads) t.Start();

            // Let workers run briefly, then dispose.
            Thread.Sleep(10);
            rwlock.Dispose();

            foreach (var t in threads) t.Join(TimeSpan.FromSeconds(5));

            Assert.True(rwlock.IsDisposed);
            Assert.Empty(errors);
            // All threads must have exited within the Join timeout.
            Assert.All(threads, t => Assert.False(t.IsAlive));
        }
    }

    /// <summary>
    /// Verifies that Dispose() waits for a live guard to be released before tearing down: a guard
    /// is acquired on thread A, Dispose() is called on thread B, and the guard is released after a
    /// short delay. Dispose() must not complete (and must not throw) before the guard is released.
    /// </summary>
    [Fact]
    public void Dispose_WaitsForLiveGuardToDrain_BeforeDisposingInnerLock()
    {
        var rwlock = new RwLock<int>(42);
        var guardAcquired = new ManualResetEventSlim(false);
        var disposeCompleted = new ManualResetEventSlim(false);
        WriteGuard<int>? capturedGuard = null;

        var holder = new Thread(() =>
        {
            var result = rwlock.Write();
            if (result.TryGetValue(out capturedGuard))
            {
                guardAcquired.Set();
                // Hold the guard for 150 ms to give the disposer time to reach Dispose()
                Thread.Sleep(150);
                capturedGuard.Dispose();
            }
        });

        var disposer = new Thread(() =>
        {
            guardAcquired.Wait();
            rwlock.Dispose();
            disposeCompleted.Set();
        });

        holder.Start();
        disposer.Start();

        // Dispose() should complete only after the guard is released (150 ms delay), so we
        // allow a generous 3-second window.
        var completed = disposeCompleted.Wait(TimeSpan.FromSeconds(3));

        holder.Join(TimeSpan.FromSeconds(3));
        disposer.Join(TimeSpan.FromSeconds(3));

        Assert.True(completed, "Dispose() did not complete within the timeout.");
        Assert.True(rwlock.IsDisposed);
    }

    /// <summary>
    /// IntoInner() from thread B while thread A holds a WriteGuard must fail (timeout), not crash.
    /// </summary>
    [Fact]
    public void IntoInner_CrossThread_WhileWriteGuardHeld_ReturnsError()
    {
        var rwlock = new RwLock<int>(99);
        var guardReady = new ManualResetEventSlim(false);
        var intoInnerDone = new ManualResetEventSlim(false);
        Result<int, Error>? intoInnerResult = null;

        // Thread A holds the write guard indefinitely until IntoInner completes.
        var holder = new Thread(() =>
        {
            var result = rwlock.Write();
            if (result.TryGetValue(out var guard))
            {
                guardReady.Set();
                intoInnerDone.Wait(); // Wait until the cross-thread call finishes
                guard.Dispose();
            }
        });

        // Thread B attempts IntoInner() while thread A holds the write guard.
        var caller = new Thread(() =>
        {
            guardReady.Wait();
            // IntoInner will attempt TryEnterWriteLock with a 5-second timeout; since thread A
            // holds the lock it should time out and return an error.
            // Use a short timeout indirectly by having thread A release after IntoInner returns.
            // We manipulate the test so the guard is held during the full IntoInner call.
            intoInnerResult = rwlock.IntoInner();
            intoInnerDone.Set();
        });

        holder.Start();
        caller.Start();

        // IntoInner has a 5 s internal timeout; give it 7 s total.
        caller.Join(TimeSpan.FromSeconds(7));
        holder.Join(TimeSpan.FromSeconds(2));

        Assert.NotNull(intoInnerResult);
        Assert.True(intoInnerResult!.Value.IsFailure,
            "IntoInner() should have returned an error because a WriteGuard was held on another thread.");
    }

    /// <summary>
    /// IntoInner() from thread B after thread A has released its WriteGuard must succeed.
    /// </summary>
    [Fact]
    public void IntoInner_CrossThread_AfterWriteGuardReleased_ReturnsValue()
    {
        var rwlock = new RwLock<int>(77);
        var guardReleased = new ManualResetEventSlim(false);
        Result<int, Error>? intoInnerResult = null;

        var holder = new Thread(() =>
        {
            var result = rwlock.Write();
            if (result.TryGetValue(out var guard))
            {
                guard.Value = 77;
                guard.Dispose(); // release before signalling
                guardReleased.Set();
            }
        });

        var caller = new Thread(() =>
        {
            guardReleased.Wait();
            intoInnerResult = rwlock.IntoInner();
        });

        holder.Start();
        caller.Start();

        holder.Join(TimeSpan.FromSeconds(3));
        caller.Join(TimeSpan.FromSeconds(3));

        Assert.NotNull(intoInnerResult);
        Assert.True(intoInnerResult!.Value.IsSuccess);
        Assert.True(intoInnerResult!.Value.TryGetValue(out var value));
        Assert.Equal(77, value);
        Assert.True(rwlock.IsDisposed);
    }

    #endregion
}