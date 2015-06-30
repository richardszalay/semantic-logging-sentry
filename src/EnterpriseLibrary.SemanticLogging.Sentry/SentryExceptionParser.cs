using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SharpRaven.Data;

namespace EnterpriseLibrary.SemanticLogging.Sentry
{
    internal class SentryExceptionParser
    {
        /// <summary>
        /// Parses a formatted exception (Exception.ToString()) into a list of SentryException objects
        /// </summary>
        /// <remarks>
        /// SemanticLogging / EventSource does not support payload values of type Exception, 
        /// so this is the only way to retain proper exception information
        /// </remarks>
        /// <param name="formattedException"></param>
        /// <returns></returns>
        public static List<SentryException> Parse(string formattedException)
        {
            var reader = new StringReader(formattedException);

            string summaryLine = reader.ReadLine();

            var exceptions = ParseSummaries(summaryLine);

            foreach (var exception in exceptions)
            {
                exception.Stacktrace = ReadStackTrace(reader);
            }

            exceptions.Reverse();

            return exceptions;
        }

        private static List<SentryException> ParseSummaries(string summaryLine)
        {
            if (String.IsNullOrWhiteSpace(summaryLine))
                return new List<SentryException>();

            string[] exceptionSummaries = summaryLine.Split(new[] { " ---> " }, StringSplitOptions.RemoveEmptyEntries);

            var exceptions = new List<SentryException>(exceptionSummaries.Length);

            foreach (string exceptionSummary in exceptionSummaries)
            {
                string[] exceptionSummaryParts = exceptionSummary.Split(new[] { ':' }, 2);

                if (exceptionSummaryParts.Length != 2)
                    return new List<SentryException>();

                exceptions.Add(new SentryException(null)
                {
                    Type = exceptionSummaryParts[0],
                    Value = exceptionSummaryParts[1].TrimStart(' ')
                });
            }

            return exceptions;
        }

        private static SentryStacktrace ReadStackTrace(TextReader reader)
        {
            var frames = new List<ExceptionFrame>();

            string line = reader.ReadLine();

            while (line != null && !line.StartsWith("   ---"))
            {
                ExceptionFrame frame = ParseExceptionFrame(line);

                if (frame != null)
                {
                    frames.Add(frame);
                }

                line = reader.ReadLine();
            }

            return new SentryStacktrace(null)
            {
                Frames = frames.ToArray()
            };
        }

        readonly static Regex FramePattern = new Regex(@"^\s*at (.*?)(?: in (.*):line (\d+))?$", RegexOptions.Compiled);

        private static ExceptionFrame ParseExceptionFrame(string line)
        {
            var match = FramePattern.Match(line);

            if (!match.Success)
                return null;

            var frame = new ExceptionFrame(null);

            frame.Function = match.Groups[1].Value;

            if (match.Groups[2].Success)
                frame.AbsolutePath = match.Groups[2].Value;

            if (match.Groups[3].Success)
                frame.LineNumber = Int32.Parse(match.Groups[3].Value);

            return frame;
        }
    }
}