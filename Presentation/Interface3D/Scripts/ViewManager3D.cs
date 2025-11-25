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

    // ✅ الكود المعدل للـ 3D:
    public void RenderMap(List<Region> regions)
    {
        Debug.Log($"Rendering {regions.Count} regions in 3D"); // ✅ "3D"
    }

    public void RenderArmyDetails(Army army)
    {
        Debug.Log($"Showing 3D details for army: {army.ArmyName}"); // ✅ "3D details"
    }

    public void ShowMessage(string message)
    {
        Debug.Log($"[3D] {message}"); // ✅ "[3D]"
    }

    public void ClearScreen()
    {
        Debug.Log("Clearing 3D screen"); // ✅ "3D screen"
    }

    public string GetUserInput()
    {
        return "user_input_3d"; // ✅ يمكنك تغييرها إذا أردت
    }
}
}