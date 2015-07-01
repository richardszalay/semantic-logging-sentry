using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    public class SentrySink : IObserver<EventEntry>
    {
        readonly IEventSourceRavenClient _ravenClient;
        readonly TimeSpan _onCompletedTimeout;
        readonly CancellationTokenSource _cancellationTokenSource;
        readonly BufferedEventPublisher<EventEntry> _bufferedPublisher;

        public ISentryExceptionLocator ExceptionLocator { get; private set; }

        public SentrySink(IEventSourceRavenClient ravenClient, TimeSpan bufferingInterval, int bufferingCount, int maxBufferSize, TimeSpan onCompletedTimeout)
            : this(ravenClient, new FormattedExceptionLocator("exception"), bufferingInterval, bufferingCount, maxBufferSize, onCompletedTimeout)
        {
        }

        public SentrySink(IEventSourceRavenClient ravenClient, ISentryExceptionLocator exceptionLocator, TimeSpan bufferingInterval, int bufferingCount, int maxBufferSize, TimeSpan onCompletedTimeout)
        {
            _ravenClient = ravenClient;
            _onCompletedTimeout = onCompletedTimeout;
            ExceptionLocator = exceptionLocator;

            string sinkId = "SentrySink-" + Guid.NewGuid().ToString("N");

            _cancellationTokenSource = new CancellationTokenSource();

            _bufferedPublisher = BufferedEventPublisher<EventEntry>.CreateAndStart(sinkId, WriteBufferedEvents,bufferingInterval, bufferingCount, maxBufferSize, _cancellationTokenSource.Token);
        }

        private async Task<int> WriteBufferedEvents(IList<EventEntry> collection)
        {
            try
            {
                int result = await Task.Run(() =>
                {
                    int publishedEvents = 0;

                    var exceptionLocator = ExceptionLocator;

                    foreach (var entry in collection)
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            return 0;
                        }

                        try
                        {
                            _ravenClient.CaptureEventEntry(entry, exceptionLocator);

                            publishedEvents++;
                        }
                        catch (WebException ex)
                        {
                            var response = ex.Response as HttpWebResponse;

                            if (response != null && response.StatusCode == (HttpStatusCode) 429)
                            {
                                SemanticLoggingEventSource.Log.CustomSinkUnhandledFault("Exceeded Sentry burst quota");
                            }
                            else
                            {
                                int status = response == null ? 0 : (int) response.StatusCode;

                                SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(
                                    "Unknown error from Sentry, status: " + status);
                            }

                            throw;
                        }
                        catch (Exception)
                        {
                            SemanticLoggingEventSource.Log.CustomSinkUnhandledFault("Unknown exception flush to Sentry");

                            throw;
                        }
                    }

                    return publishedEvents;
                });


                return result;
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    return 0;
                }

                SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(ex.ToString());
                throw;
            }
        }

        public void OnNext(EventEntry entry)
        {
            _bufferedPublisher.TryPost(entry);
        }

        public void OnError(Exception error)
        {
            this.FlushSafe();
            this.Dispose();
        }

        public void OnCompleted()
        {
            this.FlushSafe();
            this.Dispose();
        }

        ~SentrySink()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._cancellationTokenSource.Cancel();
                this._bufferedPublisher.Dispose();
            }
        }

        public Task FlushAsync()
        {
            return _bufferedPublisher.FlushAsync();
        }

        private void FlushSafe()
        {
            try
            {
                FlushAsync().Wait(_onCompletedTimeout);
            }
            catch (AggregateException ex)
            {
                // Flush operation will already log errors. Never expose this exception to the observable.
                ex.Handle(e => e is FlushFailedException);
            }
        }
    }
}