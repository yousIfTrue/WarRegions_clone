// مثال لإعادة إنشاء ViewManager2D.cs
namespace WarRegions.Presentation.Interface2D.Scripts
{
    public class ViewManager2D : MonoBehaviour, IViewManager
    {
        public static ViewManager2D Instance;
        public bool IsInitialized { get; private set; }
    
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        public void Initialize()
        {
            // تهيئة المدير
            IsInitialized = true;
            Debug.Log("ViewManager2D Initialized");
        }
        // في كل من ViewManager2D و ViewManager3D، أضف:

        public void RenderMap(List<Region> regions)
        {
            // TODO: تنفيذ عرض الخريطة
            Debug.Log($"Rendering {regions.Count} regions in 2D");
        }

        public void RenderArmyDetails(Army army)
        {
                    // TODO: تنفيذ عرض تفاصيل الجيش
            Debug.Log($"Showing details for army: {army.ArmyName}");
        }

        public void ShowMessage(string message)
        {
            Debug.Log($"[2D] {message}");
        }

        public void ClearScreen()
        {
            Debug.Log("Clearing 2D screen");
        }

        public string GetUserInput()
        {
            // TODO: تنفيذ الحصول على مدخلات المستخدم
            return "user_input";
        }
    }
}