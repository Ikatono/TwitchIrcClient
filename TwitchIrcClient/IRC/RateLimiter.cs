using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchIrcClient.IRC
{
    /// <summary>
    /// Prevents sending too many messages in a time period. A single rate limiter can
    /// be shared between multiple connections. A <see cref="CancellationToken"/> can be
    /// passed with each request to track whether the requesting i
    /// </summary>
    public class RateLimiter : IDisposable
    {
        private SemaphoreSlim Semaphore;
        private System.Timers.Timer Timer;
        public int MessageLimit { get; }
        public int Seconds { get; }

        public RateLimiter(int messages, int seconds)
        {
            Semaphore = new(messages, messages);
            Timer = new(TimeSpan.FromSeconds(seconds));
            MessageLimit = messages;
            Seconds = seconds;
            Timer.AutoReset = true;
            Timer.Elapsed += ResetLimit;
            Timer.Start();
        }

        public bool WaitForAvailable(CancellationToken? token = null)
        {
            try
            {
                lock (Semaphore)
                {
                    if (token is CancellationToken actualToken)
                        Semaphore.Wait(actualToken);
                    else
                        Semaphore.Wait();
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
        public bool WaitForAvailable(TimeSpan timeout, CancellationToken? token = null)
        {
            try
            {
                lock (Semaphore)
                {
                    if (token is CancellationToken actualToken)
                        return Semaphore.Wait(timeout, actualToken);
                    else
                        return Semaphore.Wait(timeout);
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
        public bool WaitForAvailable(int millis, CancellationToken? token = null)
        {
            try
            {
                lock (Semaphore)
                {
                    if (token is CancellationToken actualToken)
                        return Semaphore.Wait(millis, actualToken);
                    else
                        return Semaphore.Wait(millis);
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private void ResetLimit(object? sender, EventArgs e)
        {
            try
            {
                lock (Semaphore)
                {
                    var count = MessageLimit - Semaphore.CurrentCount;
                    if (count > 0)
                        Semaphore.Release(count);
                }
            }
            catch (SemaphoreFullException) { }
            catch (ObjectDisposedException) { }
        }

        #region RateLimiter Dispose
        //https://stackoverflow.com/questions/8927878/what-is-the-correct-way-of-adding-thread-safety-to-an-idisposable-object
        private int _disposedCount;
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Increment(ref _disposedCount) == 1)
            {
                if (disposing)
                {
                    Semaphore?.Dispose();
                    Timer?.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion //RateLimiter Dispose
    }
}
