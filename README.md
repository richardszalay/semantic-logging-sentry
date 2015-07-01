Sentry sink for Semantic Logging
=====================

This project provides a [Sentry](https://github.com/getsentry/sentry) sink for [Semantic Logging](https://github.com/mspnp/semantic-logging). It is currently at the proof-of-concept stage and is not fit for production environments.

The initial implementation has the following caveats:

- Events are buffered and sent asynchronously, but are still sent serially and unbatched due to [Sentry not having support for event batching]()
- Requires a custom, sub-classed, EventSourceRavenClient as SharpRaven's RavenClient does not accept Exceptions in any other format (and Exceptions cannot be created with a custom stack trace)

## Installation

Coming soon

## Usage

```csharp
// using EnterpriseLibrary.SemanticLogging.Sentry;

var applicationLog = new ObservableEventListener();
var sink = applicationLog.LogToSentry(new Dsn("https://username:password@app.getsentry.com/12345"));
applicationLog.EnableEvents(SampleEventSource.Log, EventLevel.Error, Keywords.All);
```



`LogToSentry` also accepts a number of additional arguments:

| Parameter | Type | Default | Usage |
| :-------: | :--: | :-----: | :---- |
| exceptionLocator | ISentryExceptionLocator | FormattedExceptionLocator | See below
| bufferingInterval | TimeSpan | 30 seconds |  The interval after which buffered events are flushed |
| bufferingCount | int | 1000 | The number of buffered events that triggered a flush |
| onCompletedTimeout | TimeSpan | Infinite | The amount of grace time granted during the shutdown process |
| maxBufferSize | int | 30000 | The total number of events that can be buffered in memory

### Exception Locator

The `ISentryExceptionLocator` interface solves a disconnect between Sentry, which supports structured exceptions, and Semantic Logging / Etw, which does not.

`LogToSentry` overloads that do not accept `ISentryExceptionLocator` will use `FormattedExceptionLocator`, which extracts out a payload entry named "exception" (by default) if it exists and parses it. It is assumed that the exception payload contains the string returned by `Exception.ToString()`. If found and successfully parsed, the value will be removed from the payload but sent to Sentry as an exception. Inner exceptions are also included.

A `null` value can be provided to disable the feature entirely.

## Target Frameworks

Currently only .NET 4.5 is supported, but it's planned to support everything that SharpSentry supports.

## License

The MIT License (MIT)

Copyright (c) 2015 Richard Szalay

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.