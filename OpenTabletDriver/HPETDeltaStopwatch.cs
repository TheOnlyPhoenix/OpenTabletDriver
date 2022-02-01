using System;
using System.Diagnostics;

namespace OpenTabletDriver
{
    public sealed class HPETDeltaStopwatch
    {
        private static Stopwatch internalWatch = Stopwatch.StartNew();
        private TimeSpan start;
        private TimeSpan end;
        private bool isRunning;

        public HPETDeltaStopwatch(bool startRunning = true)
        {
            isRunning = startRunning;
            start = isRunning ? internalWatch.Elapsed : default;
        }

        public static TimeSpan RuntimeElapsed => internalWatch.Elapsed;

        public TimeSpan Elapsed => isRunning ? internalWatch.Elapsed - start : end - start;

        public void Start()
        {
            if (!isRunning)
            {
                isRunning = true;
                start = internalWatch.Elapsed;
            }
        }

        public TimeSpan Stop()
        {
            if (isRunning)
            {
                isRunning = false;
                end = internalWatch.Elapsed;
            }
            return end - start;
        }

        public TimeSpan Restart()
        {
            if (isRunning)
            {
                var current = internalWatch.Elapsed;
                var delta = current - start;
                start = current;
                return delta;
            }
            else
            {
                var delta = end - start;
                Start();
                return delta;
            }
        }

        public TimeSpan Reset()
        {
            var delta = Stop();
            start = end = default;
            return delta;
        }
    }
}
