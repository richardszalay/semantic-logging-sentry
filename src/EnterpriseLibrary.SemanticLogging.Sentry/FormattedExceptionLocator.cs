using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using SharpRaven.Data;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    public class FormattedExceptionLocator : ISentryExceptionLocator
    {
        private readonly string _payloadKey;

        /// <summary>
        /// Initializes a new FormattedExceptionLocator using the "exception" payload key
        /// </summary>
        public FormattedExceptionLocator()
            : this("exception")
        {
        }

        /// <summary>
        /// Initializes a new FormattedExceptionLocator using the specified payload key
        /// </summary>
        /// <param name="payloadKey">The key used to extract formatted exception data from an event's payload</param>
        public FormattedExceptionLocator(string payloadKey)
        {
            Contract.Requires(payloadKey != null);

            _payloadKey = payloadKey;
        }

        public ICollection<SentryException> Locate(Dictionary<string, object> payload)
        {
            if (payload == null)
                return new SentryException[0];

            string formattedExceptionPayload = LocateExceptionPayload(payload, _payloadKey);

            if (formattedExceptionPayload == null)
                return new SentryException[0];

            try
            {
                var parsedExceptions = SentryExceptionParser.Parse(formattedExceptionPayload);

                if (parsedExceptions.Count > 0)
                    payload.Remove(_payloadKey);

                return parsedExceptions;
            }
            catch (Exception)
            {
                return new[]
                {
                    new SentryException(null)
                    {
                        Type = "FormattedExceptionLocator",
                        Value = "Failed to parse formatted exception"
                    }, 
                };
            }
        }

        static string LocateExceptionPayload(IDictionary<string, object> payload, string key)
        {
            if (!payload.ContainsKey(key))
                return null;

            object exceptionPayload = payload[key];

            return exceptionPayload as string;
        }
    }
}