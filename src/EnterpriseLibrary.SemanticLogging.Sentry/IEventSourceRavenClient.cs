using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using SharpRaven;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    /// <remarks>
    /// The default IRavenClient provides no way to provide an Exception without an instance does not allow custom packets
    /// </remarks>
    public interface IEventSourceRavenClient : IRavenClient
    {
        string CaptureEventEntry(EventEntry entry, ISentryExceptionLocator exceptionLocator);
    }
}
