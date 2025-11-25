// مثال لإعادة إنشاء ViewManager2D.cs
public class ViewManager2D : MonoBehaviour
{
    public static ViewManager2D Instance;
    
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
        Debug.Log("ViewManager2D Initialized");
    }
}