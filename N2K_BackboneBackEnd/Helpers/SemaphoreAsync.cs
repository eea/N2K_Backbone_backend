using System.Security.AccessControl;
using System.Threading;
namespace N2K_BackboneBackEnd.Helpers
{
    public sealed class SemaphoreAsync : IDisposable
    {
        Semaphore _semaphore;

        private SemaphoreAsync(Semaphore sem) => _semaphore = sem;
        public SemaphoreAsync(int initialCount, int maximumCount) => _semaphore = new Semaphore(initialCount, maximumCount);
        public SemaphoreAsync(int initialCount, int maximumCount, string name) => _semaphore = new Semaphore(initialCount, maximumCount, name);


        // public SemaphoreAsync(int initialCount, int maximumCount, string name, out bool createdNew, SemaphoreSecurity semaphoreSecurity) => _semaphore = new Semaphore(initialCount, maximumCount, name, out createdNew, semaphoreSecurity);

#pragma warning disable CA1416 // Validar la compatibilidad de la plataforma
        public static SemaphoreAsync OpenExisting(string name) => new SemaphoreAsync(Semaphore.OpenExisting(name));
#pragma warning restore CA1416 // Validar la compatibilidad de la plataforma


        private Task AwaitWaitHandle(WaitHandle handle, CancellationToken cancellationToken, TimeSpan timeout)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            var reg = ThreadPool.RegisterWaitForSingleObject(handle,
                (state, timedOut) =>
                {
                    // Handle timeout
                    if (timedOut)
                        taskCompletionSource.TrySetCanceled();

                    taskCompletionSource.TrySetResult(true);
                }, null, timeout, true);

            // Handle cancellation
            cancellationToken.Register(() =>
            {
                reg.Unregister(handle);
                taskCompletionSource.TrySetCanceled();
            });

            return taskCompletionSource.Task;
        }

        public async Task<bool> WaitOne(TimeSpan timeout, CancellationToken ct)
        {
            var success = await Task.Run(() =>
            {
                return WaitHandle.WaitTimeout
                    != WaitHandle.WaitAny(new[] { _semaphore, ct.WaitHandle }, timeout);
            });
            ct.ThrowIfCancellationRequested();
            return success;
        }

        public async Task<bool> WaitOne(CancellationToken ct)
        {
            while (!_semaphore.WaitOne(0))
            {
                ct.ThrowIfCancellationRequested();
                await Task.Delay(100, ct);
            }
            return true;
        }

        public async Task<bool> WaitOne()
        {
            await AwaitWaitHandle(_semaphore, CancellationToken.None, TimeSpan.FromMilliseconds(-1));
            return true;
        }

        public int Release() => _semaphore.Release();

        public int Release(int releaseCount) => _semaphore.Release(releaseCount);


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_semaphore != null)
                    {
                        _semaphore.Dispose();
                        //_semaphore = null;
                    }
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        #endregion

    }
}
