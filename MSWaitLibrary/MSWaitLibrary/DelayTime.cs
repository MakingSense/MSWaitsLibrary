using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MSWaitLibrary
{
    public class DelayTime
    {
        private float DelayFactor { get; set; }

        public DelayTime()
        {
            DelayFactor = 1;
        }

        private void _Delay(int ms, bool ignoreDelayFactor, bool log)
        {
            if (!ignoreDelayFactor)
                ms = (int)(ms * DelayFactor); // adjust total delay by DelayFactor

            if (ms <= 0)
                return;

            if (log)
                Log.Debug($"Delay for {ms}ms (DelayFactor={DelayFactor})");

            if (ms > 10000)
                Log.Warn("Delay > 10s, replace with WaitFor?");

            Task.Delay(ms).Wait();
        }

        /// <summary>
        /// All delays are multiplied by the DelayFactor.
        /// 1.0f is default
        /// 0.5f will cut the delays in half
        /// 2.0f will double the delays
        /// </summary>
        public void SetDelayFactor(float delayFactor)
        {
            Log.Debug($"Delay speed factor now {delayFactor} from {DelayFactor}");
            DelayFactor = delayFactor;
        }

        public void Milliseconds(int milliseconds, bool ignoreDelayFactor = false, bool log = true) => _Delay(milliseconds, ignoreDelayFactor, log);
        public void Seconds(int seconds, bool ignoreDelayFactor = false, bool log = true) => _Delay(seconds * 1000, ignoreDelayFactor, log);
        public void Minutes(int minutes, bool ignoreDelayFactor = false, bool log = true) => _Delay(minutes * 60 * 1000, ignoreDelayFactor, log);
        public void ByTimeSpan(TimeSpan timeSpan, bool ignoreDelayFactor = false, bool log = true) => _Delay(timeSpan.Milliseconds, ignoreDelayFactor, log);
    }
}
