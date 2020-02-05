using System;
using System.Threading;

namespace Orleans.Runtime
{
    /// <summary>
    /// A convenience API for interacting with System.Threading.Timer in a way
    /// that doesn't capture the ExecutionContext. We should be using this (or equivalent)
    /// everywhere we use timers to avoid rooting any values stored in AsyncLocals.
    /// </summary>
    /// <see href="https://github.com/dotnet/extensions/blob/a1389576a3bbc85a48bdcadce4f16bcf7cdfa088/src/Shared/src/NonCapturingTimer/NonCapturingTimer.cs"/>
    internal static class NonCapturingTimer
    {
        public static Timer Create(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
            bool restoreFlow = false;
            try
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    ExecutionContext.SuppressFlow();
                    restoreFlow = true;
                }

                return new Timer(callback, state, dueTime, period);
            }
            finally
            {
                // Restore the current ExecutionContext
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }
    }
}