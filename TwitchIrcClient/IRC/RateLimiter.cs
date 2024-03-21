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

        public void WaitForAvailable(CancellationToken? token = null)
        {
            try
            {
                if (token is CancellationToken actualToken)
                    Semaphore.Wait(actualToken);
                else
                    Semaphore.Wait();
            }
            catch (OperationCanceledException)
            {
                //caller is responsible for checking whether connection is cancelled before trying to send
            }
        }
        public bool WaitForAvailable(TimeSpan timeout, CancellationToken? token = null)
        {
            try
            {
                if (token is CancellationToken actualToken)
                    return Semaphore.Wait(timeout, actualToken);
                else
                    return Semaphore.Wait(timeout);
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
                if (token is CancellationToken actualToken)
                    return Semaphore.Wait(millis, actualToken);
                else
                    return Semaphore.Wait(millis);
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
                Semaphore.Release(MessageLimit);
            }
            catch (SemaphoreFullException)
            {

            }
            catch (ObjectDisposedException)
            {

            }
        }

        #region RateLimiter Dispose
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Semaphore?.Dispose();
                    Timer?.Dispose();
                }
                disposedValue = true;
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
