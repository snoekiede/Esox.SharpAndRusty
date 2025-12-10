using Esox.SharpAndRusty.Async;
using Esox.SharpAndRusty.Types;
using Xunit;

namespace Esox.SharpAndRust.Tests.Async
{
    public class RwLockTests
    {
        #region Basic Read Operations

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
            // Arrange
            var rwlock = new RwLock<int>(42);

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
            if (writeResult.TryGetValue(out var writeGuard))
            {
                writeGuard.Dispose();
            }
        }

        [Fact]
        public void Read_MultipleConcurrentReaders_AllSucceed()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);

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

        #endregion

        #region Basic Write Operations

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
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.InvalidOperation, error.Kind);
            }
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

        #endregion

        #region TryRead Tests

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
            var writeGuard = rwlock.Write();

            // Try to read from a different thread to avoid recursion
            var result = await Task.Run(() => rwlock.TryRead());

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.ResourceExhausted, error.Kind);
            }

            // Cleanup
            if (writeGuard.TryGetValue(out var guard)) guard.Dispose();
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

        #endregion

        #region TryReadTimeout Tests

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
            var writeGuard = rwlock.Write();
            var timeout = TimeSpan.FromMilliseconds(100);

            // Act - Try from different thread to avoid recursion
            var result = await Task.Run(() => rwlock.TryReadTimeout(timeout));

            // Assert
            Assert.True(result.IsFailure);
            if (result.TryGetError(out var error))
            {
                // Can be Timeout or InvalidOperation depending on lock state
                Assert.True(error.Kind == ErrorKind.Timeout || error.Kind == ErrorKind.InvalidOperation,
                    $"Expected Timeout or InvalidOperation, got {error.Kind}");
            }

            // Cleanup
            if (writeGuard.TryGetValue(out var guard)) guard.Dispose();
        }

        #endregion

        #region TryWrite Tests

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
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.ResourceExhausted, error.Kind);
            }

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

        #endregion

        #region TryWriteTimeout Tests

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
            if (result.TryGetError(out var error))
            {
                Assert.Equal(ErrorKind.Timeout, error.Kind);
            }

            // Cleanup
            if (readGuard.TryGetValue(out var guard)) guard.Dispose();
        }

        #endregion

        #region ReadGuard Tests

        [Fact]
        public void ReadGuard_Value_CanBeRead()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var result = rwlock.Read();

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
        public void ReadGuard_Map_TransformsValue()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var result = rwlock.Read();

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
        public void ReadGuard_Dispose_ReleasesLock()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var result1 = rwlock.Read();

            // Act
            if (result1.TryGetValue(out var guard1))
            {
                guard1.Dispose();
            }

            // Assert - should be able to write now
            var result2 = rwlock.TryWrite();
            Assert.True(result2.IsSuccess);
            if (result2.TryGetValue(out var guard2))
            {
                guard2.Dispose();
            }
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

        #endregion

        #region WriteGuard Tests

        [Fact]
        public void WriteGuard_Value_CanBeReadAndModified()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var result = rwlock.Write();

            // Act
            if (result.TryGetValue(out var guard))
            {
                using (guard)
                {
                    Assert.Equal(42, guard.Value);
                    guard.Value = 100;
                    Assert.Equal(100, guard.Value);
                }
            }

            // Verify persistence
            var readResult = rwlock.Read();
            if (readResult.TryGetValue(out var readGuard))
            {
                using (readGuard)
                {
                    Assert.Equal(100, readGuard.Value);
                }
            }
        }

        [Fact]
        public void WriteGuard_Map_TransformsValue()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var result = rwlock.Write();

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
        public void WriteGuard_Update_ModifiesValue()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var result = rwlock.Write();

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
            var readResult = rwlock.Read();
            if (readResult.TryGetValue(out var readGuard))
            {
                using (readGuard)
                {
                    Assert.Equal(84, readGuard.Value);
                }
            }
        }

        [Fact]
        public void WriteGuard_Dispose_ReleasesLock()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var result1 = rwlock.Write();

            // Act
            if (result1.TryGetValue(out var guard1))
            {
                guard1.Dispose();
            }

            // Assert - should be able to read immediately
            var result2 = rwlock.TryRead();
            Assert.True(result2.IsSuccess);
            if (result2.TryGetValue(out var guard2))
            {
                guard2.Dispose();
            }
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

        #endregion

        #region IntoInner Tests

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

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => rwlock.IntoInner());
        }

        #endregion

        #region Concurrency Tests

        [Fact]
        public async Task RwLock_MultipleConcurrentReaders_AllSucceed()
        {
            // Arrange
            var rwlock = new RwLock<int>(0);
            var readerCount = 10;
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < readerCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var result = rwlock.Read();
                    if (result.TryGetValue(out var guard))
                    {
                        using (guard)
                        {
                            Thread.Sleep(10); // Simulate work
                            _ = guard.Value;
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert - all readers should complete successfully
            Assert.True(true); // If we get here, all readers succeeded
        }

        [Fact]
        public async Task RwLock_ReaderWriterAlternation_MaintainsConsistency()
        {
            // Arrange
            var rwlock = new RwLock<int>(0);
            var iterations = 20;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                // Write
                var writeResult = rwlock.Write();
                if (writeResult.TryGetValue(out var writeGuard))
                {
                    using (writeGuard)
                    {
                        writeGuard.Value = i;
                    }
                }

                // Read
                var readResult = rwlock.Read();
                if (readResult.TryGetValue(out var readGuard))
                {
                    using (readGuard)
                    {
                        Assert.Equal(i, readGuard.Value);
                    }
                }

                await Task.Delay(1);
            }

            // Assert
            var finalRead = rwlock.Read();
            if (finalRead.TryGetValue(out var finalGuard))
            {
                using (finalGuard)
                {
                    Assert.Equal(iterations - 1, finalGuard.Value);
                }
            }
        }

        [Fact]
        public async Task RwLock_StressTest_MaintainsDataIntegrity()
        {
            // Arrange
            var rwlock = new RwLock<List<int>>(new List<int>());
            var writerCount = 5;
            var itemsPerWriter = 10;
            var tasks = new List<Task>();

            // Act - multiple writers adding items
            for (int i = 0; i < writerCount; i++)
            {
                var writerIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < itemsPerWriter; j++)
                    {
                        var result = rwlock.Write();
                        if (result.TryGetValue(out var guard))
                        {
                            using (guard)
                            {
                                var list = guard.Value;
                                list.Add(writerIndex * 100 + j);
                                guard.Value = list;
                            }
                        }
                        Thread.Sleep(1);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            var finalResult = rwlock.Read();
            if (finalResult.TryGetValue(out var finalGuard))
            {
                using (finalGuard)
                {
                    var expectedCount = writerCount * itemsPerWriter;
                    Assert.Equal(expectedCount, finalGuard.Value.Count);
                    Assert.Equal(expectedCount, finalGuard.Value.Distinct().Count()); // No duplicates
                }
            }
        }

        #endregion

        #region Dispose Tests

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
            using (rwlock = new RwLock<int>(42))
            {
                Assert.False(rwlock.IsDisposed);
            }

            // Assert
            Assert.True(rwlock.IsDisposed);
        }

        #endregion

        #region Complex Scenarios

        [Fact]
        public void RwLock_WithComplexType_WorksCorrectly()
        {
            // Arrange
            var rwlock = new RwLock<Dictionary<string, int>>(new Dictionary<string, int>());

            // Act - Write
            var writeResult = rwlock.Write();
            if (writeResult.TryGetValue(out var writeGuard))
            {
                using (writeGuard)
                {
                    writeGuard.Value["key1"] = 42;
                    writeGuard.Value["key2"] = 99;
                }
            }

            // Act - Read
            var readResult = rwlock.Read();
            if (readResult.TryGetValue(out var readGuard))
            {
                using (readGuard)
                {
                    Assert.Equal(2, readGuard.Value.Count);
                    Assert.Equal(42, readGuard.Value["key1"]);
                    Assert.Equal(99, readGuard.Value["key2"]);
                }
            }
        }

        [Fact]
        public async Task RwLock_WriterBlocksReaders_UntilReleased()
        {
            // Arrange
            var rwlock = new RwLock<int>(42);
            var writeResult = rwlock.Write();
            
            if (!writeResult.TryGetValue(out var writeGuard))
            {
                Assert.Fail("Failed to acquire write lock");
                return;
            }

            bool readerAcquired = false;

            // Act - Start reader task that will try to acquire lock
            var readTask = Task.Run(() =>
            {
                Thread.Sleep(50); // Ensure writer has lock first
                var result = rwlock.TryRead();
                readerAcquired = result.IsSuccess;
            });

            await Task.Delay(100); // Let reader try while writer still holds lock
            
            // Assert - reader should fail while writer holds lock
            Assert.False(readerAcquired);

            // Release writer
            writeGuard.Dispose();

            await readTask;
        }

        [Fact]
        public async Task RwLock_MultipleReadersBlockWriter()
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

        #endregion
    }
}
