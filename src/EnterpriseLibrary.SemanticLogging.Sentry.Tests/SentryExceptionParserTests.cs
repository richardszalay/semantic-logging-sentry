using Xunit;

namespace EnterpriseLibrary.SemanticLogging.Sentry.Tests
{
    public class SentryExceptionParserTests
    {
        [Fact]
        public void Parses_basic_exceptions()
        {
            var exceptions = SentryExceptionParser.Parse(ExceptionWithoutInners);

            Assert.Equal(1, exceptions.Count);

            Assert.Equal("System.Exception", exceptions[0].Type);
            Assert.Equal("Third", exceptions[0].Value);
            Assert.Equal(4, exceptions[0].Stacktrace.Frames.Length);
            Assert.Equal("SampleProject.Controllers.StoreController.<GetStoreContent>d__8.MoveNext()", exceptions[0].Stacktrace.Frames[0].Function);
            Assert.Equal(48, exceptions[0].Stacktrace.Frames[0].LineNumber);
            Assert.Equal(@"d:\Dev\SampleProject\Controllers\StoreController.cs", exceptions[0].Stacktrace.Frames[0].AbsolutePath);
        }

        [Fact]
        public void Parses_inner_exceptions()
        {
            var exceptions = SentryExceptionParser.Parse(ExceptionWithInners);

            Assert.Equal(3, exceptions.Count);

            // Starts with inner-most exception
            Assert.Equal("System.Exception", exceptions[0].Type);
            Assert.Equal("First", exceptions[0].Value);
            Assert.Equal(17, exceptions[0].Stacktrace.Frames.Length);

            Assert.Equal("System.InvalidOperationException", exceptions[1].Type);
            Assert.Equal("Second", exceptions[1].Value);
            Assert.Equal(5, exceptions[1].Stacktrace.Frames.Length);

            Assert.Equal("System.Exception", exceptions[2].Type);
            Assert.Equal("Third", exceptions[2].Value);
            Assert.Equal(5, exceptions[2].Stacktrace.Frames.Length);
            Assert.Equal("SampleProject.Controllers.StoreController.<GetStoreContent>d__8.MoveNext()", exceptions[2].Stacktrace.Frames[0].Function);
            Assert.Equal(48, exceptions[2].Stacktrace.Frames[0].LineNumber);
            Assert.Equal(@"d:\Dev\SampleProject\Controllers\StoreController.cs", exceptions[2].Stacktrace.Frames[0].AbsolutePath);
        }

        const string ExceptionWithoutInners =
            @"System.Exception: Third
   at SampleProject.Controllers.StoreController.<GetStoreContent>d__8.MoveNext() in d:\Dev\SampleProject\Controllers\StoreController.cs:line 48
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at System.Web.Http.Dispatcher.HttpControllerDispatcher.<SendAsync>d__1.MoveNext()";

        const string ExceptionWithInners =
            @"System.Exception: Third ---> System.InvalidOperationException: Second ---> System.Exception: First
   at SampleProject.Controllers.StoreController.<GetStoreContent>d__8.MoveNext() in d:\Dev\SampleProject\Controllers\StoreController.cs:line 48
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at SampleProject.Controllers.StoreController.<GetTempStoreContent>d__5.MoveNext() in d:\Dev\SampleProject\Controllers\StoreController.cs:line 38
   --- End of inner exception stack trace ---
   at SampleProject.Controllers.StoreController.<GetTempStoreContent>d__5.MoveNext() in d:\Dev\SampleProject\Controllers\StoreController.cs:line 42
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at SampleProject.Controllers.StoreController.<GetStores>d__1.MoveNext() in d:\Dev\SampleProject\Controllers\StoreController.cs:line 21
   --- End of inner exception stack trace ---
   at SampleProject.Controllers.StoreController.<GetStores>d__1.MoveNext() in d:\Dev\SampleProject\Controllers\StoreController.cs:line 30
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at System.Threading.Tasks.TaskHelpersExtensions.<CastToObject>d__3`1.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at System.Web.Http.Controllers.ApiControllerActionInvoker.<InvokeActionAsyncCore>d__0.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at System.Web.Http.Controllers.ActionFilterResult.<ExecuteAsync>d__2.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
   at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
   at System.Runtime.CompilerServices.TaskAwaiter`1.GetResult()
   at System.Web.Http.Dispatcher.HttpControllerDispatcher.<SendAsync>d__1.MoveNext()";
    }
}
