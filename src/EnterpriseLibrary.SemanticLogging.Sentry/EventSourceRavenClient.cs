using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using SharpRaven;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    public class EventSourceRavenClient : RavenClient, IEventSourceRavenClient
    {
        readonly IEventEntryJsonPacketFactory _packetFactory = new EventEntryJsonPacketFactory();

        public EventSourceRavenClient(Dsn dsn, IEventEntryJsonPacketFactory eventEntryJsonPacketFactory)
            : base(dsn)

        {
            _packetFactory = eventEntryJsonPacketFactory;
        }

        public EventSourceRavenClient(Dsn dsn)
            : this(dsn, new EventEntryJsonPacketFactory())
        {
        }

        public EventSourceRavenClient(string dsn)
            : this(new Dsn(dsn))
        {
        }


        public string CaptureEventEntry(EventEntry entry, ISentryExceptionLocator exceptionLocator)
        {
            var packet = _packetFactory.Create(CurrentDsn.ProjectID, entry, exceptionLocator);

            return base.Send(packet, CurrentDsn);
        }
    }
}