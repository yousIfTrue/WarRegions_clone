
namespace WarRegions.Core.Interfaces
{
    public interface IViewManager
    {
        bool IsInitialized { get; }
        void Initialize();
        void RenderMap(List<Region> regions);
        void RenderArmyDetails(Army army);
        void ShowMessage(string message);
        void ClearScreen();
        string GetUserInput();
    }
}
