using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Xunit.Sdk;

namespace AspectInjector.Tests.Runtime
{
    internal static class AsyncVoid
    {
        /// <summary>
        /// Waits for an async-void operation and rethrows its failure on the caller's thread.
        /// </summary>
        public static void Run(Action action)
        {
            var previous = SynchronizationContext.Current;
            // Keep continuations off the caller's context while this thread waits.
            var context = new AsyncTestSyncContext(null);
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                action();
                var completion = context.WaitForCompletionAsync();
                if (!completion.Wait(TimeSpan.FromSeconds(30)))
                    throw new TimeoutException("The async void operation did not complete.");

                var error = completion.GetAwaiter().GetResult();
                if (error != null)
                    ExceptionDispatchInfo.Capture(error).Throw();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }
    }
}
