using System.Collections.Generic;
using SharpRaven.Data;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    public interface ISentryExceptionLocator
    {
        ICollection<SentryException> Locate(Dictionary<string, object> payload);
    }
}
