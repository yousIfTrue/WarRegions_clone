namespace Core.Controllers
{
    public class EngineController
    {
        private readonly object _lock = new object();
        private bool _initialized;
        private bool _running;

        // Default scene name created/used by the controller
        public string DefaultSceneName { get; set; } = "Main";

        public EngineController() { }

        // Initialize minimal runtime state (create default scene if missing)
        public void Initialize()
        {
            lock (_lock)
            {
                if (_initialized)
                {
                    Debug.Log("EngineController: already initialized.");
                    return;
                }

                Debug.Log("EngineController: initializing...");
                // Ensure default scene exists and set it active
                var scene = SceneManager.GetSceneByName(DefaultSceneName) ?? SceneManager.CreateScene(DefaultSceneName, setActive: true);

                _initialized = true;
                Debug.Log($"EngineController: initialized. Active scene = {scene?.name ?? "null"}");
            }
        }

        // Start the GameEngine (safe to call multiple times)
        public void StartEngine()
        {
            lock (_lock)
            {
                if (!_initialized) Initialize();

                if (_running)
                {
                    Debug.Log("EngineController: engine already running.");
                    return;
                }

                try
                {
                    GameEngine.Instance.Start();
                    _running = true;
                    Debug.Log("EngineController: engine started.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"EngineController: failed to start engine: {ex.Message}");
                    throw;
                }
            }
        }

        // Stop the GameEngine
        public void StopEngine()
        {
            lock (_lock)
            {
                if (!_running)
                {
                    Debug.Log("EngineController: engine is not running.");
                    return;
                }

                try
                {
                    GameEngine.Instance.Stop();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"EngineController: error while stopping engine: {ex.Message}");
                }
                finally
                {
                    _running = false;
                    Debug.Log("EngineController: engine stopped.");
                }
            }
        }

        // Create a GameObject and add it to the active scene (if any)
        public GameObject CreateGameObject(string name = "GameObject")
        {
            var go = GameObject.Create(name);
            var active = SceneManager.GetActiveScene();
            if (active != null)
            {
                active.AddRootGameObject(go);
            }
            return go;
        }

        // Convenience wrapper for SceneManager.Instantiate
        public GameObject Instantiate(string name = "GameObject")
        {
            return SceneManager.Instantiate(name);
        }

        // Scene operations
        public Scene LoadScene(string name, bool setActive = true)
        {
            return SceneManager.LoadScene(name, setActive);
        }

        public bool UnloadScene(string name)
        {
            return SceneManager.UnloadScene(name);
        }

        public Scene GetActiveScene()
        {
            return SceneManager.GetActiveScene();
        }

        // Move GameObject to specified scene (root)
        public bool MoveGameObjectToScene(GameObject go, Scene scene)
        {
            return SceneManager.MoveGameObjectToScene(go, scene);
        }

        // Destroy GameObject via SceneManager
        public void DestroyGameObject(GameObject go)
        {
            SceneManager.Destroy(go);
        }

        // Quit runtime (stops engine and performs minimal cleanup)
        public void Quit()
        {
            try
            {
                StopEngine();
            }
            catch { /* swallow */ }

            Debug.Log("EngineController: quit requested.");
        }

        // Status helpers
        public bool IsInitialized()
        {
            lock (_lock) { return _initialized; }
        }

        public bool IsRunning()
        {
            lock (_lock) { return _running; }
        }
    }
}
