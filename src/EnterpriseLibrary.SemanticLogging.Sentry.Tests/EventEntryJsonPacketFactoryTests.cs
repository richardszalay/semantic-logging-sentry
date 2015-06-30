using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Schema;
using SharpRaven.Data;
using Xunit;

namespace EnterpriseLibrary.SemanticLogging.Sentry.Tests
{
    public class EventEntryJsonPacketFactoryTests
    {
        [Fact]
        public void Maps_basic_properties()
        {
            var entry = CreateTestEntry(EventLevel.Informational);
            var sut = new EventEntryJsonPacketFactory();

            var packet = sut.Create("prjid", entry, null);

            Assert.Equal("prjid", packet.Project);
            Assert.Null(packet.Exceptions);
            Assert.Equal(entry.FormattedMessage, packet.Message);
            Assert.Equal(entry.Timestamp.UtcDateTime, packet.TimeStamp);
        }

        [Fact]
        public void Maps_tags_from_properties()
        {
            var entry = CreateTestEntry(EventLevel.Informational);
            var sut = new EventEntryJsonPacketFactory();

            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(new Dictionary<string, string>
            {
                {"EventKeywords", entry.Schema.KeywordsDescription},
                {"EventOpcode", entry.Schema.OpcodeName},
                {"EventTask", entry.Schema.TaskName},
                {"EventName", entry.Schema.EventName},
                {"EventVersion", entry.Schema.Version.ToString(CultureInfo.InvariantCulture)}
            }, packet.Tags);
        }

        [Fact]
        public void Maps_extras_from_payload()
        {
            var entry = CreateTestEntry(EventLevel.Informational);
            var sut = new EventEntryJsonPacketFactory();

            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(new Dictionary<string, object>
            {
                {"Field1", "value1"},
                {"Field2", 2},
                {"Field3", "value3"}
            }, packet.Extra);
        }

        [Fact]
        public void Maps_always_as_debug()
        {
            var entry = CreateTestEntry(EventLevel.LogAlways);
            var sut = new EventEntryJsonPacketFactory();
            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(ErrorLevel.Debug, packet.Level);
        }

        [Fact]
        public void Maps_verbose_as_debug()
        {
            var entry = CreateTestEntry(EventLevel.Verbose);
            var sut = new EventEntryJsonPacketFactory();
            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(ErrorLevel.Debug, packet.Level);
        }

        [Fact]
        public void Maps_informational_as_info()
        {
            var entry = CreateTestEntry(EventLevel.Informational);
            var sut = new EventEntryJsonPacketFactory();
            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(ErrorLevel.Info, packet.Level);
        }

        [Fact]
        public void Maps_warning_as_warning()
        {
            var entry = CreateTestEntry(EventLevel.Warning);
            var sut = new EventEntryJsonPacketFactory();
            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(ErrorLevel.Warning, packet.Level);
        }

        [Fact]
        public void Maps_error_as_error()
        {
            var entry = CreateTestEntry(EventLevel.Error);
            var sut = new EventEntryJsonPacketFactory();
            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(ErrorLevel.Error, packet.Level);
        }

        [Fact]
        public void Maps_critical_as_fatal()
        {
            var entry = CreateTestEntry(EventLevel.Critical);
            var sut = new EventEntryJsonPacketFactory();
            var packet = sut.Create("prjid", entry, null);

            Assert.Equal(ErrorLevel.Fatal, packet.Level);
        }

        private EventEntry CreateTestEntry(EventLevel level)
        {
            var schema = new EventSchema(
                1, Guid.Empty, "Test", level,
                (EventTask) 2, "Task",
                (EventOpcode) 3, "Opcode",
                (EventKeywords) 4, "Keywords",
                5, new[] {"Field1", "Field2", "Field3"}
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
