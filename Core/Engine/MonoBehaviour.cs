
namespace Core.Engine
{
    public class MonoBehaviour : Component
    {
        private static readonly List<MonoBehaviour> s_instances = new List<MonoBehaviour>();
        private static readonly object s_lock = new object();

        private bool _awakeCalled;
        private bool _startCalled;

        // مشابه لسلوك Unity: يمكن تفعيل/تعطيل المونو
        public bool enabled { get; set; } = true;

        public MonoBehaviour()
        {
            Register(this);
        }

        ~MonoBehaviour()
        {
            Unregister(this);
        }

        internal static void Register(MonoBehaviour mb)
        {
            if (mb == null) return;
            lock (s_lock)
            {
                if (!s_instances.Contains(mb))
                    s_instances.Add(mb);
            }
        }

        internal static void Unregister(MonoBehaviour mb)
        {
            if (mb == null) return;
            lock (s_lock)
            {
                s_instances.Remove(mb);
            }
        }

        // Allow callers (مثل GameObject) لتغيير الحالة مع نداء OnEnable/OnDisable
        public void SetEnabled(bool value)
        {
            if (enabled == value) return;
            enabled = value;
            try
            {
                if (enabled) OnEnable();
                else OnDisable();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in OnEnable/OnDisable: {ex.Message}");
            }
        }

        // الدوال الافتراضية التي يمكن للـ user override
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void LateUpdate() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        // دوال ثابتة تستدعي كل الـ MonoBehaviour المسجلين
        public static void UpdateAll()
        {
            MonoBehaviour[] snapshot;
            lock (s_lock) { snapshot = s_instances.ToArray(); }

            foreach (var mb in snapshot)
            {
                if (mb == null) continue;
                if (!mb.enabled) continue;

                try
                {
                    if (!mb._awakeCalled)
                    {
                        mb._awakeCalled = true;
                        mb.Awake();
                    }

                    if (!mb._startCalled)
                    {
                        mb._startCalled = true;
                        mb.Start();
                    }

                    mb.Update();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in Update of {mb?.name ?? "MonoBehaviour"}: {ex.Message}");
                }
            }
        }

        public static void FixedUpdateAll()
        {
            MonoBehaviour[] snapshot;
            lock (s_lock) { snapshot = s_instances.ToArray(); }

            foreach (var mb in snapshot)
            {
                if (mb == null) continue;
                if (!mb.enabled) continue;

                try
                {
                    if (!mb._awakeCalled)
                    {
                        mb._awakeCalled = true;
                        mb.Awake();
                    }

                    if (!mb._startCalled)
                    {
                        // لا نستدعي Start هنا إذا لم يتم استدعاؤه في Update بعد — لكن لتجنب فقدان نداء Start
                        // نستدعي Start أيضاً هنا إذا لم تُدعَّ بعد
                        mb._startCalled = true;
                        mb.Start();
                    }

                    mb.FixedUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in FixedUpdate of {mb?.name ?? "MonoBehaviour"}: {ex.Message}");
                }
            }
        }

        public static void LateUpdateAll()
        {
            MonoBehaviour[] snapshot;
            lock (s_lock) { snapshot = s_instances.ToArray(); }

            foreach (var mb in snapshot)
            {
                if (mb == null) continue;
                if (!mb.enabled) continue;

                try
                {
                    mb.LateUpdate();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error in LateUpdate of {mb?.name ?? "MonoBehaviour"}: {ex.Message}");
                }
            }
        }

        // Optional helper: تنظيف كل النسخ (قد يفيد بالـ tests / إعادة تحميل المشاهد)
        internal static void ClearAll()
        {
            lock (s_lock) { s_instances.Clear(); }
        }
    }
}