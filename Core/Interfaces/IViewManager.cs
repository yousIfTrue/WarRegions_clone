
namespace WarRegions.Core.Interfaces
{
    public interface IViewManager
    {
        bool IsInitialized { get; }
        void Initialize();
        void RenderMap(List<Region> regions);
        void RenderArmyDetails(Army army);
        void ShowMessage(string message);
        void CleanScreen();  // ✅ غيرت من ClearScreen
        void UpdateView();   // ✅ أضف هذه الدالة المطلوبة
        string GetUserInput();
    }
}
