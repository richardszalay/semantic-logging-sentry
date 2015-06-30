using System.Collections.Generic;
using System.Linq;
using EnterpriseLibrary.SemanticLogging.Sentry.Tests.Infrastructure;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Xunit;

namespace EnterpriseLibrary.SemanticLogging.Sentry.Tests
{
    public class SentrySinkExtentionsTests
    {
        [Fact]
        public void Default_exception_locator_uses_exception_payload()
        {
            var client = new EventSourceRavenClient("http://public:secret@example.com/project-id");

            var events = new BasicSubject<EventEntry>();

            var sinkSubscription = events.LogToSentry(client);

            var results = sinkSubscription.Sink.ExceptionLocator.Locate(new Dictionary<string, object>
            {
                { "first", 1},
                { "second", 2},
                {"exception", "Exception : text"}
            });

            Assert.Equal("text", results.First().Value);
        }
    }
}
