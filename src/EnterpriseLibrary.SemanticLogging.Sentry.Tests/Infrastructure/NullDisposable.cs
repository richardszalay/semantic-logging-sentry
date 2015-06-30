using System;

namespace EnterpriseLibrary.SemanticLogging.Sentry.Tests.Infrastructure
{
    public class NullDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
