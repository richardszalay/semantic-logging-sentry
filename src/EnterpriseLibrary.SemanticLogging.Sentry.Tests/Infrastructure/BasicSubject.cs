using System;

namespace EnterpriseLibrary.SemanticLogging.Sentry.Tests.Infrastructure
{
    // Easier than including Rx as a dependency
    public class BasicSubject<T> : IObservable<T>
    {
        private IObserver<T> _observer;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            this._observer = observer;

            return new NullDisposable();
        }

        public void OnNext(T value)
        {
            if (_observer != null)
                _observer.OnNext(value);
        }
    }
}
