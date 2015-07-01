using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Schema;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Moq;
using Xunit;

namespace EnterpriseLibrary.SemanticLogging.Sentry.Tests
{
    public class SentrySinkTests
    {
        [Fact]
        public async Task Flushes_messages_to_raven_client()
        {
            // Arrange
            var client = new Mock<IEventSourceRavenClient>();

            client.Setup(x => x.CaptureEventEntry(It.IsAny<EventEntry>(), It.IsAny<ISentryExceptionLocator>())).Verifiable();

            var sink = new SentrySink(client.Object, Buffering.DefaultBufferingInterval, 
                Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, Timeout.InfiniteTimeSpan);

            // Act
            sink.OnNext(CreateTestEntry());
            sink.OnNext(CreateTestEntry());
            sink.OnNext(CreateTestEntry());

            await sink.FlushAsync();

            // Assert
            client.Verify(
                x => x.CaptureEventEntry(It.IsAny<EventEntry>(), It.IsAny<ISentryExceptionLocator>()), 
                Times.Exactly(3)
                );
        }

        [Fact]
        public void Stops_flushing_messages_on_first_exception()
        {
            // Arrange
            var client = new Mock<IEventSourceRavenClient>();

            var breakingEntry = CreateTestEntry();

            client.Setup(x => x.CaptureEventEntry(It.Is<EventEntry>(e => e != breakingEntry), It.IsAny<ISentryExceptionLocator>()))
                .Verifiable();

            client.Setup(x => x.CaptureEventEntry(breakingEntry, It.IsAny<ISentryExceptionLocator>()))
                .Throws(new WebException());

            var sink = new SentrySink(client.Object, Buffering.DefaultBufferingInterval,
                Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, Timeout.InfiniteTimeSpan);

            // Act
            sink.OnNext(CreateTestEntry());
            sink.OnNext(breakingEntry);
            sink.OnNext(CreateTestEntry());
            sink.OnCompleted();

            // Assert
            client.Verify(
                x => x.CaptureEventEntry(It.Is<EventEntry>(e => e != breakingEntry), It.IsAny<ISentryExceptionLocator>()),
                Times.Exactly(1)
                );
        }

        private EventEntry CreateTestEntry(EventLevel level = EventLevel.Error)
        {
            var schema = new EventSchema(
                1, Guid.Empty, "Test", level,
                (EventTask)2, "Task",
                (EventOpcode)3, "Opcode",
                (EventKeywords)4, "Keywords",
                5, new[] { "Field1", "Field2", "Field3" }
                );

            var payload = new ReadOnlyCollection<object>(new object[]
            {
                "value1",
                2,
                "value3"
            });

            return new EventEntry(
                Guid.Empty, 1, "Test message",
                payload, DateTimeOffset.Now,
                Guid.Empty, Guid.Empty, schema
                );
        }

    }
}
