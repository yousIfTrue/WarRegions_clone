
namespace Core.Engine
{
    // Simple Time façade that reads from GameEngine and provides timeScale, scaled/unscaled values and realtime.
    public static class Time
    {
        private static readonly Stopwatch s_realtimeStopwatch;
        private static float s_timeScale = 1f;

        static Time()
        {
            s_realtimeStopwatch = Stopwatch.StartNew();
        }

        // Global time scale (must be >= 0)
        public static float timeScale
        {
            get => s_timeScale;
            set
            {
                if (float.IsNaN(value) || value < 0f) throw new ArgumentOutOfRangeException(nameof(value), "timeScale must be >= 0");
                s_timeScale = value;
            }
        }

        // Scaled delta time (affected by timeScale)
        public static float deltaTime
        {
            get
            {
                var engine = GameEngine.Instance;
                return engine != null ? engine.DeltaTime * s_timeScale : 0f;
            }
        }

        // Unscaled delta time (raw engine delta)
        public static float unscaledDeltaTime
        {
            get
            {
                var engine = GameEngine.Instance;
                return engine != null ? engine.DeltaTime : 0f;
            }
        }

        // Scaled fixed delta time (note: Unity treats fixedDeltaTime as independent; here we expose engine's fixed step)
        public static float fixedDeltaTime
        {
            get
            {
                var engine = GameEngine.Instance;
                return engine != null ? engine.FixedDeltaTime : 0f;
            }
            set
            {
                // Not supported to change engine step here because GameEngine owns it.
                // Intentionally left empty to preserve the simple facade.
                // Could forward to GameEngine if a setter is desired.
                throw new InvalidOperationException("Setting fixedDeltaTime is not supported. Change GameEngine.FixedDeltaTime internally if needed.");
            }
        }

        // Scaled time since engine start (approximate: derived from engine Time * timeScale)
        public static float time
        {
            get
            {
                var engine = GameEngine.Instance;
                return engine != null ? engine.Time * s_timeScale : 0f;
            }
        }

        // Unscaled time since engine start (raw)
        public static float unscaledTime
        {
            get
            {
                var engine = GameEngine.Instance;
                return engine != null ? engine.Time : 0f;
            }
        }

        // Frame count from engine
        public static int frameCount
        {
            get
            {
                var engine = GameEngine.Instance;
                return engine != null ? engine.FrameCount : 0;
            }
        }

        // Real time since application start (not affected by timeScale)
        public static double realtimeSinceStartup => s_realtimeStopwatch.Elapsed.TotalSeconds;
    }
}
