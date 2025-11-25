
namespace Core.Engine
{
    public class Component
    {
        public GameObject gameObject { get; internal set; }
        public Transform transform { get; internal set; }
        public string name { get; set; }

        public Component()
        {
            name = GetType().Name;
            Debug.Log($"Component created: {name}");
        }

        public T GetComponent<T>() where T : Component
        {
            return gameObject != null ? gameObject.GetComponent<T>() : null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            return gameObject != null ? gameObject.GetComponents<T>() : Array.Empty<T>();
        }

        public Component GetComponent(Type type)
        {
            return gameObject != null ? gameObject.GetComponent(type) : null;
        }

        public bool CompareTag(string tag)
        {
            return gameObject?.CompareTag(tag) ?? false;
        }
    }
}