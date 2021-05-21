using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace MSWaitLibrary
{
   public  class Metrics
    {
        private static readonly List<StopWatchData> StopWatches = new List<StopWatchData>();
        private static readonly PerformanceCounter CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private static readonly PerformanceCounter RamCounter = new PerformanceCounter("Memory", "Available MBytes");

        public static void StartStopWatch() => StopWatches.Add(new StopWatchData());

        public static void StopStopWatch() => StopWatches.First(x => x.Name.Equals(TestContext.CurrentContext.Test.Name)).Stop();

        public static string ElapsedSinceStart()
        {
            StopWatchData sw = StopWatches.FirstOrDefault(x => x.Name.Equals(TestContext.CurrentContext.Test.Name));
            return sw == null ? "--:--" : sw.ElapsedSinceStart();
        }

        public static string ElapsedSinceLast()
        {
            StopWatchData sw = StopWatches.FirstOrDefault(x => x.Name.Equals(TestContext.CurrentContext.Test.Name));
            return sw == null ? "--:--" : sw.ElapsedSinceLast();
        }

        public static void LogMachineLoad()
        {
            // two calls required with delay for calculation
            var cpu = CpuCounter.NextValue();
            var ram = RamCounter.NextValue();
            Thread.Sleep(1000);
            cpu = CpuCounter.NextValue();
            ram = RamCounter.NextValue();

            // output
            Log.Info($"Machine Load:" +
                    $"\n\tCPU: {Math.Round(cpu)}%" +
                    $"\n\tRAM: {ram} MB Available");
        }

        internal class StopWatchData
        {
            internal string Name { get; }
            private Stopwatch StopWatch { get; }
            internal TimeSpan PreviousElapsed { get; set; }
            internal TimeSpan TimeSinceLast { get; set; }

            internal StopWatchData()
            {
                Name = TestContext.CurrentContext.Test.Name;
                StopWatch = new Stopwatch();
                PreviousElapsed = TimeSpan.Zero;

                Log.Info("[Timer] Starting stopwatch");
                StopWatch.Start();
            }

            internal void Stop()
            {
                StopWatch.Stop();
                Log.Info($"[Timer] Stopwatch stopped");
            }

            internal string ElapsedSinceStart()
            {
                TimeSinceLast = StopWatch.Elapsed.Subtract(PreviousElapsed);
                TimeSpan elapsed = StopWatch.Elapsed;
                PreviousElapsed = elapsed;
                return elapsed.ToString(@"mm\:ss");
            }

            internal string ElapsedSinceLast() => TimeSinceLast.ToString(@"ss\.ff");
        }
    }
}
