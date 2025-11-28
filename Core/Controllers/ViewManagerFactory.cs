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
        public void RenderArmyDetails(Army army) => Console.WriteLine($"[2D] Showing {army.ArmyName}");
        public void ShowMessage(string message) => Console.WriteLine($"[2D] {message}");
        public string GetUserInput() => Console.ReadLine() ?? "";
    }

    public class SimpleViewManager3D : IViewManager
    {
        public bool IsInitialized { get; private set; }
        public void Initialize() => IsInitialized = true;
        public void CleanScreen() => Console.WriteLine("[3D] Screen cleared");
        public void UpdateView() => Console.WriteLine("[3D] View updated");
        public void RenderMap(List<Region> regions) => Console.WriteLine($"[3D] Rendering {regions.Count} regions");
        public void RenderArmyDetails(Army army) => Console.WriteLine($"[3D] Showing {army.ArmyName}");
        public void ShowMessage(string message) => Console.WriteLine($"[3D] {message}");
        public string GetUserInput() => Console.ReadLine() ?? "";
    }
}