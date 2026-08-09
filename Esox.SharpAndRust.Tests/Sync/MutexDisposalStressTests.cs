using System.Collections.Concurrent;
using Esox.SharpAndRusty.Sync;
using Esox.SharpAndRusty.Types;
using Xunit;

namespace Esox.SharpAndRusty.Tests.Sync;

public class MutexDisposalStressTests
{
    /// <summary>
    /// Repeatedly races Dispose() against a burst of concurrent LockAsync / LockAsyncTimeout
    /// calls on a fresh mutex each iteration. Verifies two things the fix is supposed to guarantee:
    ///   1. Dispose() never hangs, even if waiters are mid-acquisition when it's called.
    ///   2. Every waiter resolves to a Result (Ok or Err) — never an unhandled exception,
    ///      which would violate the library's "never throws" contract.
    /// </summary>
    [Fact]
    [Trait("Category", "Stress")]
    public async Task Dispose_ConcurrentWithLockAsyncAndTimeout_NeverHangsAndNeverThrowsUnwrapped()
    {
        const int iterations = 1_000;
        const int waitersPerIteration = 8;

        var unexpectedExceptions = new ConcurrentBag<Exception>();
        var hungIteration = -1;

        for (var i = 0; i < iterations; i++)
        {
            var mutex = new Mutex<int>(0);
            var waiterTasks = new List<Task>(waitersPerIteration);

            for (var w = 0; w < waitersPerIteration; w++)
            {
                var useTimeout = w % 2 == 0;
                waiterTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = useTimeout
                            ? await mutex.LockAsyncTimeout(TimeSpan.FromSeconds(2))
                            : await mutex.LockAsync();

                        // Success or failure is fine — an unhandled throw is not.
                        result.Match(
                            success: guard =>
                            {
                                guard.Dispose();
                                return Unit.Value;
                            },
                            failure: _ => Unit.Value
                        );
                    }
                    catch (Exception ex)
                    {
                        unexpectedExceptions.Add(ex);
                    }
                }));
            }

            // Dispose concurrently, deliberately not waiting for waiters to be "in position" —
            // the whole point is to hit every possible interleaving over enough iterations.
            var disposeTask = Task.Run(() => mutex.Dispose());

            var allWork = Task.WhenAll(waiterTasks.Append(disposeTask));
            var winner = await Task.WhenAny(allWork, Task.Delay(TimeSpan.FromSeconds(10)));

            if (winner != allWork)
            {
                hungIteration = i;
                break;
            }
        }

        Assert.True(hungIteration == -1,
            $"Dispose() hung instead of completing on iteration {hungIteration}.");
        Assert.Empty(unexpectedExceptions);
    }
}