using System;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    public class SentrySink : IObserver<EventEntry>
    {
        private readonly IEventSourceRavenClient _ravenClient;

        public ISentryExceptionLocator ExceptionLocator { get; private set; }

        public SentrySink(IEventSourceRavenClient ravenClient)
            : this(ravenClient, new FormattedExceptionLocator("exception"))
        {
        }

        public SentrySink(IEventSourceRavenClient ravenClient, ISentryExceptionLocator exceptionLocator)
        {
            _ravenClient = ravenClient;
            ExceptionLocator = exceptionLocator;
        }

        public void OnNext(EventEntry entry)
        {
            _ravenClient.CaptureEventEntry(entry, ExceptionLocator);
        }

        public void OnError(Exception error)
        {
            _ravenClient.CaptureException(error);
        }

        public void OnCompleted()
        {
        }
    }
}