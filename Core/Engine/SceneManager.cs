
namespace WarRegions.Core.Engine
{
    // Simple Scene container
    public class Scene
    {
        private readonly List<GameObject> _rootObjects = new List<GameObject>();
        private readonly object _lock = new object();

        public string name { get; }

        internal Scene(string name)
        {
            this.name = name ?? "Scene";
            Debug.Log($"Scene created: {this.name}");
        }

        public GameObject[] GetRootGameObjects()
        {
            lock (_lock)
            {
                return _rootObjects.ToArray();
            }
        }

        public void AddRootGameObject(GameObject go)
        {
            if (go == null) throw new ArgumentNullException(nameof(go));
            lock (_lock)
            {
                if (!_rootObjects.Contains(go))
                    _rootObjects.Add(go);
            }
        }

        public bool RemoveRootGameObject(GameObject go)
        {
            if (go == null) return false;
            lock (_lock)
            {
                return _rootObjects.Remove(go);
            }
        }

        public GameObject Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            lock (_lock)
            {
                return _rootObjects.FirstOrDefault(g => string.Equals(g.name, name, StringComparison.Ordinal));
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _rootObjects.Clear();
            }
        }

        public override string ToString() => $"Scene({name})";
    }

    // Lightweight SceneManager similar to Unity's API surface used in simple simulations.
    public static class SceneManager
    {
        private static readonly object s_lock = new object();
        private static readonly Dictionary<string, Scene> s_scenes = new Dictionary<string, Scene>(StringComparer.Ordinal);
        private static Scene s_activeScene;

        // Events
        public static event Action<Scene> SceneLoaded;
        public static event Action<Scene> SceneUnloaded;
        public static event Action<Scene, Scene> ActiveSceneChanged;

        // Create a new scene (does not set active unless specified)
        public static Scene CreateScene(string name, bool setActive = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            lock (s_lock)
            {
                if (s_scenes.TryGetValue(name, out var existing))
                {
                    Debug.LogWarning($"Scene '{name}' already exists. Returning existing scene.");
                    if (setActive) SetActiveScene(existing);
                    return existing;
                }

                var scene = new Scene(name);
                s_scenes.Add(name, scene);
                if (setActive) SetActiveScene(scene);
                SceneLoaded?.Invoke(scene);
                return scene;
            }
        }

        // Load scene by name (create if missing). For this simple runtime there's no disk I/O.
        public static Scene LoadScene(string name, bool setActive = true)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            lock (s_lock)
            {
                if (!s_scenes.TryGetValue(name, out var scene))
                {
                    scene = new Scene(name);
                    s_scenes.Add(name, scene);
                    SceneLoaded?.Invoke(scene);
                }

                if (setActive) SetActiveScene(scene);
                return scene;
            }
        }

        // Unload scene (removes and clears root objects)
        public static bool UnloadScene(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            lock (s_lock)
            {
                if (!s_scenes.TryGetValue(name, out var scene)) return false;
                // If active, clear active scene first
                if (s_activeScene == scene)
                {
                    SetActiveScene(null);
                }

                scene.Clear();
                s_scenes.Remove(name);
                SceneUnloaded?.Invoke(scene);
                return true;
            }
        }

        public static Scene GetSceneByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            lock (s_lock)
            {
                s_scenes.TryGetValue(name, out var scene);
                return scene;
            }
        }

        public static Scene[] GetAllScenes()
        {
            lock (s_lock)
            {
                return s_scenes.Values.ToArray();
            }
        }

        public static Scene GetActiveScene()
        {
            lock (s_lock)
            {
                return s_activeScene;
            }
        }

        public static void SetActiveScene(Scene scene)
        {
            lock (s_lock)
            {
                var previous = s_activeScene;
                if (previous == scene) return;
                s_activeScene = scene;
                ActiveSceneChanged?.Invoke(previous, scene);
            }
        }

        // Convenience: instantiate a GameObject into the active scene (or returns object with no scene if none)
        public static GameObject Instantiate(string name = "GameObject")
        {
            var go = GameObject.Create(name);
            var scene = GetActiveScene();
            if (scene != null)
            {
                scene.AddRootGameObject(go);
            }
            return go;
        }

        // Destroy a GameObject: removes it from any scene root lists and tries to remove its components
        public static void Destroy(GameObject go)
        {
            if (go == null) return;

            // Remove from any scene that contains it as root
            lock (s_lock)
            {
                foreach (var kv in s_scenes.Values)
                {
                    kv.RemoveRootGameObject(go);
                }
            }

            // Try to remove all components (best-effort)
            // Note: GameObject.RemoveComponent removes ownership and is safe to call
            // We'll attempt to remove until no components left
            try
            {
                var comps = go.GetComponents<Component>();
                foreach (var c in comps)
                {
                    try { go.RemoveComponent(c); } catch { /* ignore */ }
                }
            }
            catch { /* ignore */ }
        }

        // Move GameObject to a specific scene (as root)
        public static bool MoveGameObjectToScene(GameObject go, Scene scene)
        {
            if (go == null) return false;
            lock (s_lock)
            {
                // remove from all scenes first
                foreach (var s in s_scenes.Values)
                {
                    s.RemoveRootGameObject(go);
                }

                if (scene != null)
                {
                    scene.AddRootGameObject(go);
                }
                return true;
            }
        }
    }
}
