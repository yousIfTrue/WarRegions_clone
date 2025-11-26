
namespace WarRegions.Core.Engine
{
    public class GameEngine
    {
        private static GameEngine _instance;
        private static readonly object _lock = new object();
        public static GameEngine Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new GameEngine();
                }
            }
        }

        private volatile bool _isRunning;
        private Thread _gameThread;
        private float _targetFrameRate = 60f;
        private float _fixedTimeStep = 0.02f;
        private double _accumulatedTime;

        private readonly List<DelayedCall> _delayedCalls = new List<DelayedCall>();
        private readonly List<DelayedCall> _pendingCalls = new List<DelayedCall>();
        private readonly object _callsLock = new object();

        // Protect engine state that may be read from other threads
        private readonly object _stateLock = new object();
        private float _deltaTime;
        private float _time;
        private int _frameCount;

        // Public API (keep the original names)
        public float DeltaTime
        {
            get { lock (_stateLock) { return _deltaTime; } }
            private set { lock (_stateLock) { _deltaTime = value; } }
        }

        public float FixedDeltaTime => _fixedTimeStep;

        public float Time
        {
            get { lock (_stateLock) { return _time; } }
            private set { lock (_stateLock) { _time = value; } }
        }

        public int FrameCount
        {
            get { lock (_stateLock) { return _frameCount; } }
            private set { lock (_stateLock) { _frameCount = value; } }
        }

        private GameEngine() { }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;

            // initialize timing state
            Time = 0f;
            FrameCount = 0;
            DeltaTime = 0f;
            _accumulatedTime = 0.0;

            Debug.Log("Game Engine Started");

            _gameThread = new Thread(GameLoop) { IsBackground = true };
            _gameThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;

            // Avoid joining from the same thread
            if (_gameThread != null && Thread.CurrentThread != _gameThread)
            {
                _gameThread.Join(1000); // timeout to avoid blocking forever
            }

            Debug.Log("Game Engine Stopped");
        }

        public void DelayedCall(Action action, float delay)
        {
            if (action == null) return;

            if (delay <= 0f)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in delayed call: {ex.Message}");
                }
            }
            else
            {
                var call = new DelayedCall(action, delay);
                lock (_callsLock)
                {
                    _pendingCalls.Add(call);
                }
            }
        }

        private void GameLoop()
        {
            var stopwatch = Stopwatch.StartNew();
            double previousTime = stopwatch.Elapsed.TotalSeconds;

            while (_isRunning)
            {
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                float delta = (float)(currentTime - previousTime);
                previousTime = currentTime;

                // Protect against negative or extremely large delta
                if (float.IsNaN(delta) || delta < 0f) delta = 0f;
                if (delta > 1f) delta = 1f; // clamp huge pauses

                // Update engine state (thread-safe)
                DeltaTime = delta;
                Time += DeltaTime;
                FrameCount++;

                // Update delayed calls
                UpdateDelayedCalls();

                // Fixed Update
                _accumulatedTime += delta;
                while (_accumulatedTime >= _fixedTimeStep)
                {
                    try
                    {
                        MonoBehaviour.FixedUpdateAll();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error in FixedUpdateAll: {ex.Message}");
                    }
                    _accumulatedTime -= _fixedTimeStep;
                }

                // Regular Update
                try
                {
                    MonoBehaviour.UpdateAll();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in UpdateAll: {ex.Message}");
                }

                // Late Update
                try
                {
                    MonoBehaviour.LateUpdateAll();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in LateUpdateAll: {ex.Message}");
                }

                // Frame-rate control
                double frameTime = stopwatch.Elapsed.TotalSeconds - currentTime;
                double targetFrameTime = 1.0 / Math.Max(0.0001, _targetFrameRate);
                double sleepTime = targetFrameTime - frameTime;

                if (sleepTime > 0)
                {
                    double msDouble = sleepTime * 1000.0;
                    int ms = (int)Math.Floor(msDouble);
                    if (ms > 0)
                    {
                        Thread.Sleep(ms);
                    }
                    else
                    {
                        // Very short wait — yield to scheduler
                        Thread.Yield();
                    }
                }
            }

            // Clear thread reference when exiting loop
            _gameThread = null;
        }

        private void UpdateDelayedCalls()
        {
            // Merge pending into active
            lock (_callsLock)
            {
                if (_pendingCalls.Count > 0)
                {
                    _delayedCalls.AddRange(_pendingCalls);
                    _pendingCalls.Clear();
                }
            }

            // read delta once
            float delta;
            lock (_stateLock)
            {
                delta = _deltaTime;
            }

            for (int i = _delayedCalls.Count - 1; i >= 0; i--)
            {
                try
                {
                    if (_delayedCalls[i].Update(delta))
                    {
                        _delayedCalls.RemoveAt(i);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while updating delayed call: {ex.Message}");
                    _delayedCalls.RemoveAt(i);
                }
            }
        }

        private class DelayedCall
        {
            private readonly Action _action;
            private float _remainingTime;

            public DelayedCall(Action action, float delay)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
                _remainingTime = delay;
            }

            public bool Update(float deltaTime)
            {
                _remainingTime -= deltaTime;
                if (_remainingTime <= 0f)
                {
                    try
                    {
                        _action();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error in delayed call: {ex.Message}");
                    }
                    return true;
                }
                return false;
            }
        }
    }
}