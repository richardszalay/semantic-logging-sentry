using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EnterpriseLibrary.SemanticLogging.Sentry.Tests
{
    public class FormattedExceptionLocatorTests
    {
        [Fact]
        public void Locate_does_not_remove_data_if_not_successfully_parsed()
        {
            // Arrange
            var payload = new Dictionary<string, object> { {"exception", "Testing"} };
            var sut = new FormattedExceptionLocator();

            // Act
            sut.Locate(payload);

            // Assert
            Assert.True(payload.ContainsKey("exception"));
        }
    }
}
