
namespace Core.Engine
{
    public class GameObject
    {
        private readonly List<Component> _components = new List<Component>();

        public string name { get; set; }
        public string tag { get; set; } = "Untagged";

        private bool _activeSelf = true;
        public bool activeSelf
        {
            get => _activeSelf;
            set => _activeSelf = value;
        }

        public bool activeInHierarchy
        {
            get
            {
                if (!activeSelf) return false;
                var parent = transform?.parent;
                while (parent != null)
                {
                    if (!parent.gameObject.activeSelf) return false;
                    parent = parent.parent;
                }
                return true;
            }
        }

        public Transform transform { get; }

        public GameObject(string name = "GameObject")
        {
            this.name = name ?? "GameObject";
            this.transform = new Transform(this);
            Debug.Log($"GameObject created: {this.name}");
        }

        // AddComponent: creates, sets references and stores the component.
        public T AddComponent<T>() where T : Component, new()
        {
            var comp = new T();
            AttachComponent(comp);
            return comp;
        }

        // General AddComponent for types without generic constraint (uses Activator)
        public Component AddComponent(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!typeof(Component).IsAssignableFrom(type))
                throw new ArgumentException("Type must derive from Component", nameof(type));

            var instance = (Component)Activator.CreateInstance(type);
            AttachComponent(instance);
            return instance;
        }

        private void AttachComponent(Component comp)
        {
            if (comp == null) throw new ArgumentNullException(nameof(comp));

            // set ownership references (internal set in Component)
            comp.gameObject = this;
            comp.transform = this.transform;

            lock (_components)
            {
                if (!_components.Contains(comp))
                    _components.Add(comp);
            }
        }

        public T GetComponent<T>() where T : Component
        {
            lock (_components)
            {
                for (int i = 0; i < _components.Count; i++)
                {
                    if (_components[i] is T t) return t;
                }
            }
            return null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            lock (_components)
            {
                var list = new List<T>();
                foreach (var c in _components)
                {
                    if (c is T t) list.Add(t);
                }
                return list.ToArray();
            }
        }

        public Component GetComponent(Type type)
        {
            if (type == null) return null;
            lock (_components)
            {
                foreach (var c in _components)
                {
                    if (type.IsAssignableFrom(c.GetType())) return c;
                }
            }
            return null;
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            component = GetComponent<T>();
            return component != null;
        }

        public bool CompareTag(string tag)
        {
            return string.Equals(this.tag, tag, StringComparison.Ordinal);
        }

        public bool SetActive(bool value)
        {
            if (activeSelf == value) return false;
            activeSelf = value;

            // propagate enable/disable to components that implement OnEnable/OnDisable via MonoBehaviour.SetEnabled
            lock (_components)
            {
                foreach (var c in _components)
                {
                    if (c is MonoBehaviour mb)
                    {
                        try
                        {
                            mb.SetEnabled(value);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error while changing enabled state on {mb.name}: {ex.Message}");
                        }
                    }
                }
            }

            return true;
        }

        // Remove a component instance
        public bool RemoveComponent(Component component)
        {
            if (component == null) return false;
            lock (_components)
            {
                var removed = _components.Remove(component);
                if (removed)
                {
                    // clear ownership
                    component.gameObject = null;
                    component.transform = null;
                }
                return removed;
            }
        }

        // Convenience static factory
        public static GameObject Create(string name = "GameObject")
        {
            return new GameObject(name);
        }
    }
}
