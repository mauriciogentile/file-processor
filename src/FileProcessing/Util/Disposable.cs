using System;

namespace BJSS.FileProcessing.Util
{
    public abstract class Disposable : IDisposable
    {
        protected bool Disposed { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose worker method.
        /// </summary>
        /// <param name="disposing">Are we disposing? Otherwise we're finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            Disposed = true;
        }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}
