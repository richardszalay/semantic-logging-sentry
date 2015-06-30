using System;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
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
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, IEventSourceRavenClient ravenClient, ISentryExceptionLocator exceptionLocator)
        {
            var sink = new SentrySink(ravenClient, exceptionLocator);
            var subscription = eventStream.Subscribe(sink);

            return new SinkSubscription<SentrySink>(subscription, sink);
        }

        /// <summary>
        /// Logs messages to the specified client, extracting formatted exceptions from the event's "exception" payload
        /// </summary>
        /// <param name="eventStream">The source event stream</param>
        /// <param name="ravenClient">The <see cref="IEventSourceRavenClient"/> to log events to</param>
        /// <param name="exceptionLocator">The <see cref="ISentryExceptionLocator"/> that will be used to extract exception information from events, or null to disable exception location</param>
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, IEventSourceRavenClient ravenClient)
        {
            return eventStream.LogToSentry(ravenClient, new FormattedExceptionLocator());
        }

        /// <summary>
        /// Logs messages to the specified Sentry DSN, extracting formatted exceptions from the event's "exception" payload
        /// </summary>
        /// <param name="eventStream">The source event stream</param>
        /// <param name="dsn">The Sentry DSN to write events to</param>
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, Dsn dsn)
        {
            return eventStream.LogToSentry(new EventSourceRavenClient(dsn), new FormattedExceptionLocator());
        }

        /// <summary>
        /// Logs messages to the specified Sentry DSN
        /// </summary>
        /// <param name="eventStream">The source event stream</param>
        /// <param name="dsn">The Sentry DSN to write events to</param>
        /// <param name="exceptionLocator">The <see cref="ISentryExceptionLocator"/> that will be used to extract exception information from events, or null to disable exception location</param>
        /// <returns>A SinkSubscription, exposing the SentrySink created</returns>
        public static SinkSubscription<SentrySink> LogToSentry(this IObservable<EventEntry> eventStream, Dsn dsn, ISentryExceptionLocator exceptionLocator)
        {
            return eventStream.LogToSentry(new EventSourceRavenClient(dsn), exceptionLocator);
        }
    }
}
