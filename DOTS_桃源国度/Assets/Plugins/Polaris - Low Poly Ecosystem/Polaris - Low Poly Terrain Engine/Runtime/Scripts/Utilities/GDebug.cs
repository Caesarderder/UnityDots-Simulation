#if GRIFFIN
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Pinwheel.Griffin
{
    public static class GDebug
    {
        private static Stopwatch st = new Stopwatch();

        public static void StartStopwatch()
        {
            st.Start();
        }

        public static void EndStopwatch()
        {
            st.Stop();
        }

        public static void LogStopwatchTimeMilis(string label = "")
        {
            Debug.Log(label + ": " + st.ElapsedMilliseconds);
            st.Reset();
        }

        public static void LogTick()
        {
            Debug.Log(st.ElapsedTicks);
            st.Reset();
        }
    }
}
#endif
