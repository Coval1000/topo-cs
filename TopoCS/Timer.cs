using System.Diagnostics;

namespace TopoCS
{
    class Timer
    {
        private Stopwatch _StopWatch;
        private float m_ticksPerMs;
        private long m_LastFrameTime = 0;

        public float FrameTime { get; private set; }
        public float CumulativeFrameTime { get; private set; }

        public bool Initialize()
        { 
            if (!Stopwatch.IsHighResolution)
                return false;
            if (Stopwatch.Frequency == 0)
                return false;

            m_ticksPerMs = (float)(Stopwatch.Frequency / 1000.0f);

            _StopWatch = Stopwatch.StartNew();
            return true;
        }
        public void Frame()
        {
            long currentTime = _StopWatch.ElapsedTicks;

            float timeDifference = currentTime - m_LastFrameTime;

            FrameTime = timeDifference / m_ticksPerMs;
            CumulativeFrameTime += FrameTime;
            m_LastFrameTime = currentTime;
        }
    }
}
