// Core/Controllers/ViewManagerFactory.cs
using WarRegions.Core.Interfaces;

namespace WarRegions.Core.Controllers
{
    public static class ViewManagerFactory
    {
        public static IViewManager CreateViewManager(string viewType)
        {
            // implementation بسيط للبداية
            return viewType.ToLower() switch
            {
                "2d" => new SimpleViewManager2D(),
                "3d" => new SimpleViewManager3D(),
                _ => new SimpleViewManager2D()
            };
        }
    }

    // Fallback implementations داخل Core
    public class SimpleViewManager2D : IViewManager
    {
        public bool IsInitialized { get; private set; }
        public void Initialize() => IsInitialized = true;
        public void CleanScreen() => Console.WriteLine("[2D] Screen cleared");
        public void UpdateView() => Console.WriteLine("[2D] View updated");
        public void RenderMap(List<Region> regions) => Console.WriteLine($"[2D] Rendering {regions.Count} regions");
        public void ShowMessage(string message) => Console.WriteLine($"[2D] {message}");
        public string GetUserInput() => Console.ReadLine() ?? "";
        public class SimpleViewManager2D : IViewManager
{
    public bool IsInitialized { get; private set; }
    
    public void Initialize() 
    { 
        IsInitialized = true;
        Console.WriteLine("[Simple2D] Initialized - Ready for 2D rendering");
    }
    
    public void CleanScreen()
    {
        Console.Clear();
        Console.WriteLine("[Simple2D] Screen cleaned - 2D mode");
    }
    
    public void UpdateView()
    {
        Console.WriteLine("[Simple2D] View updated - Refreshing 2D display");
        // يمكن إضافة منطق تحديث الواجهة هنا
    }
    
    public void RenderMap(List<Region> regions)
    {
        Console.WriteLine($"[Simple2D] Rendering {regions.Count} regions in 2D:");
        foreach (var region in regions.Take(5)) // عرض أول 5 مناطق فقط للاختبار
        {
            Console.WriteLine($"  - {region.RegionName} at ({region.X},{region.Y})");
        }
    }
    
public void RenderArmyDetails(Army army)
{
    if (army == null)
    {
        Console.WriteLine("[Simple2D] No army to display");
        return;
    }
    
    Console.WriteLine($"[Simple2D] 2D Army Visualization:");
    Console.WriteLine($"  🎯 {army.ArmyName} - 2D Model View");
    Console.WriteLine($"  ⚔️  Combat Strength: {army.GetStrength():F1}");
    Console.WriteLine($"  🏃 Movement: {army.MovementPoints}/{army.MaxMovementPoints}");
    Console.WriteLine($"  ❤️  Units Alive: {army.GetAliveUnitCount()}");
}
    
    public void ShowMessage(string message)
    {
        Console.WriteLine($"[2D MSG] {message}");
    }
    
    public string GetUserInput()
    {
        Console.Write("[2D INPUT] Enter command: ");
        return Console.ReadLine()?.Trim() ?? "";
    }
}
    }

    public class SimpleViewManager3D : IViewManager
    {
        public bool IsInitialized { get; private set; }
        public void Initialize() => IsInitialized = true;
        public void CleanScreen() => Console.WriteLine("[3D] Screen cleared");
        public void UpdateView() => Console.WriteLine("[3D] View updated");
        public void RenderMap(List<Region> regions) => Console.WriteLine($"[3D] Rendering {regions.Count} regions");
        public void ShowMessage(string message) => Console.WriteLine($"[3D] {message}");
        public string GetUserInput() => Console.ReadLine() ?? "";
        public class SimpleViewManager3D : IViewManager
{
    public bool IsInitialized { get; private set; }
    
    public void Initialize() 
    { 
        IsInitialized = true;
        Console.WriteLine("[Simple3D] Initialized - Ready for 3D rendering");
    }
    
    public void CleanScreen()
    {
        Console.Clear();
        Console.WriteLine("[Simple3D] Screen cleaned - 3D mode");
    }
    
    public void UpdateView()
    {
        Console.WriteLine("[Simple3D] View updated - Refreshing 3D perspective");
        // منطق تحديث المشهد 3D
    }
    
    public void RenderMap(List<Region> regions)
    {
        Console.WriteLine($"[Simple3D] Rendering {regions.Count} regions in 3D:");
        Console.WriteLine("  [3D Visualization - Terrain and elevation]");
        foreach (var region in regions.Take(3)) // عرض أول 3 مناطق
        {
            Console.WriteLine($"  - {region.RegionName} (3D terrain: {region.Terrain})");
        }
    }
    
public void RenderArmyDetails(Army army)
{
    if (army == null)
    {
        Console.WriteLine("[Simple3D] No army to display in 3D");
        return;
    }
    
    Console.WriteLine($"[Simple3D] 3D Army Visualization:");
    Console.WriteLine($"  🎯 {army.ArmyName} - 3D Model View");
    Console.WriteLine($"  ⚔️  Combat Strength: {army.GetStrength():F1}");
    Console.WriteLine($"  🏃 Movement: {army.MovementPoints}/{army.MaxMovementPoints}");
    Console.WriteLine($"  ❤️  Units Alive: {army.GetAliveUnitCount()}");
}
    
    public void ShowMessage(string message)
    {
        Console.WriteLine($"[3D MSG] 🎮 {message}");
    }
    
    public string GetUserInput()
    {
        Console.Write("[3D INPUT] 🎯 Enter command: ");
        return Console.ReadLine()?.Trim() ?? "";
    }
}
    }
}