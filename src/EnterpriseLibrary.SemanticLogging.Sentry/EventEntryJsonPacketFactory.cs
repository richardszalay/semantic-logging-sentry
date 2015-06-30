using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using SharpRaven.Data;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    public interface IEventEntryJsonPacketFactory
    {
        JsonPacket Create(string projectId, EventEntry entry,
            ISentryExceptionLocator exceptionLocator);
    }

    public class EventEntryJsonPacketFactory : IEventEntryJsonPacketFactory
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public JsonPacket Create(string projectId, EventEntry entry,
            ISentryExceptionLocator exceptionLocator)
        {
            var extras = GetExtras(entry);

            var packet = new JsonPacket(projectId)
            {
                Message = entry.FormattedMessage,
                Level = GetLogLevel(entry.Schema.Level),
                Tags = GetTags(entry),
                Extra = extras,
                TimeStamp = entry.Timestamp.UtcDateTime
            };

            if (exceptionLocator != null)
            {
                var exceptions = exceptionLocator.Locate(extras);

                if (exceptions != null)
                    packet.Exceptions = exceptions.ToList();
            }

            return packet;
        }

        private ErrorLevel GetLogLevel(EventLevel level)
        {
            ErrorLevel mappedLevel;
            if (LevelMap.TryGetValue(level, out mappedLevel))
                return mappedLevel;

            return ErrorLevel.Debug;
        }

        private static readonly Dictionary<EventLevel, ErrorLevel> LevelMap = new Dictionary<EventLevel, ErrorLevel>
        {
            {EventLevel.LogAlways, ErrorLevel.Debug},
            {EventLevel.Verbose, ErrorLevel.Debug},
            {EventLevel.Informational, ErrorLevel.Info},
            {EventLevel.Warning, ErrorLevel.Warning},
            {EventLevel.Error, ErrorLevel.Error},
            {EventLevel.Critical, ErrorLevel.Fatal}
        };

        private Dictionary<string, string> GetTags(EventEntry entry)
        {
            return new Dictionary<string, string>
            {
                {"EventKeywords", entry.Schema.KeywordsDescription},
                {"EventOpcode", entry.Schema.OpcodeName},
                {"EventTask", entry.Schema.TaskName},
                {"EventName", entry.Schema.EventName},
                {"EventVersion", entry.Schema.Version.ToString(CultureInfo.InvariantCulture)}
            };
        }

        private Dictionary<string, object> GetExtras(EventEntry entry)
        {
            IEnumerator<string> keysEnumerator = ((ICollection<string>) entry.Schema.Payload).GetEnumerator();
            IEnumerator<object> valuesEnumerator = entry.Payload.GetEnumerator();

            var extras = new Dictionary<string, object>();

            while (keysEnumerator.MoveNext() && valuesEnumerator.MoveNext())
                extras[keysEnumerator.Current] = valuesEnumerator.Current;

            return extras;
        }
    }
}
