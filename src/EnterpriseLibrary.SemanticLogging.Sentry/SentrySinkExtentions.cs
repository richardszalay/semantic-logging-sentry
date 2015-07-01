using System;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using SharpRaven;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    public static class SentrySinkExtensions
    {
        /// <summary>
        /// Logs messages to the specified client
        /// </summary>
        /// <param name="eventStream">The source event stream</param>
        /// <param name="ravenClient">The <see cref="IEventSourceRavenClient"/> to log events to</param>
        /// <param name="exceptionLocator">The <see cref="ISentryExceptionLocator"/> that will be used to extract exception information from events, or null to disable exception location</param>
        /// <param name="bufferingInterval">The interval after which buffered events are flushed</param>
        /// <param name="bufferingCount">The number of buffered events that triggered a flush</param>
        /// <param name="onCompletedTimeout">The amount of time to wait for a graceful shutdown when shutdown</param>
        /// <param name="maxBufferSize">Max maximum number of events to keep buffered</param>
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, IEventSourceRavenClient ravenClient, ISentryExceptionLocator exceptionLocator, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? onCompletedTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize)
        {
            var sink = new SentrySink(
                ravenClient, 
                exceptionLocator, 
                bufferingInterval ?? Buffering.DefaultBufferingInterval, 
                bufferingCount, maxBufferSize, 
                onCompletedTimeout ?? Timeout.InfiniteTimeSpan);

            var subscription = eventStream.Subscribe(sink);

            return new SinkSubscription<SentrySink>(subscription, sink);
        }

        /// <summary>
        /// Logs messages to the specified client, extracting formatted exceptions from the event's "exception" payload
        /// </summary>
        /// <param name="eventStream">The source event stream</param>
        /// <param name="ravenClient">The <see cref="IEventSourceRavenClient"/> to log events to</param>
        /// <param name="bufferingInterval">The interval after which buffered events are flushed</param>
        /// <param name="bufferingCount">The number of buffered events that triggered a flush</param>
        /// <param name="onCompletedTimeout">The amount of time to wait for a graceful shutdown when shutdown</param>
        /// <param name="maxBufferSize">Max maximum number of events to keep buffered</param>
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, IEventSourceRavenClient ravenClient, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? onCompletedTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize)
        {
            return eventStream.LogToSentry(ravenClient, new FormattedExceptionLocator(), bufferingInterval, bufferingCount, onCompletedTimeout, maxBufferSize);
        }

        /// <summary>
        /// Logs messages to the specified Sentry DSN, extracting formatted exceptions from the event's "exception" payload
        /// </summary>
        /// <param name="eventStream">The source event stream</param>
        /// <param name="dsn">The Sentry DSN to write events to</param>
        /// <param name="bufferingInterval">The interval after which buffered events are flushed</param>
        /// <param name="bufferingCount">The number of buffered events that triggered a flush</param>
        /// <param name="onCompletedTimeout">The amount of time to wait for a graceful shutdown when shutdown</param>
        /// <param name="maxBufferSize">Max maximum number of events to keep buffered</param>
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, Dsn dsn, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? onCompletedTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize)
        {

            return eventStream.LogToSentry(new EventSourceRavenClient(dsn), new FormattedExceptionLocator(), bufferingInterval, bufferingCount, onCompletedTimeout, maxBufferSize);
        }

        /// <summary>
        /// Logs messages to the specified Sentry DSN
        /// </summary>
        /// <param name="eventStream">The source event stream</param>
        /// <param name="dsn">The Sentry DSN to write events to</param>
        /// <param name="exceptionLocator">The <see cref="ISentryExceptionLocator"/> that will be used to extract exception information from events, or null to disable exception location</param>
        /// <param name="bufferingInterval">The interval after which buffered events are flushed</param>
        /// <param name="bufferingCount">The number of buffered events that triggered a flush</param>
        /// <param name="onCompletedTimeout">The amount of time to wait for a graceful shutdown when shutdown</param>
        /// <param name="maxBufferSize">Max maximum number of events to keep buffered</param>
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, Dsn dsn, ISentryExceptionLocator exceptionLocator, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? onCompletedTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize)
        {
            return eventStream.LogToSentry(new EventSourceRavenClient(dsn), exceptionLocator, bufferingInterval, bufferingCount, onCompletedTimeout, maxBufferSize);
        }
    }
}
