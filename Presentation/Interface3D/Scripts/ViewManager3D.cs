
namespace WarRegions.Presentation.Interface3D.Scripts
{
    public class ViewManager3D : MonoBehaviour, IViewManager
    {
        public static ViewManager3D Instance { get; private set; }
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
            IsInitialized = true;
            Debug.Log("ViewManager3D Initialized");
        }

        // ✅ تطبيق كل دوال الواجهة
        public void RenderMap(List<Region> regions)
        {
            Debug.Log($"Rendering {regions.Count} regions in 3D");
        }

        public void RenderArmyDetails(Army army)
        {
            Debug.Log($"Showing 3D details for army: {army.ArmyName}");
        }

        public void ShowMessage(string message)
        {
            Debug.Log($"[3D] {message}");
        }

        public void ClearScreen()
        {
            Debug.Log("Clearing 3D screen");
        }

        public string GetUserInput()
        {
            return "user_input_3d";
        }
    }
}