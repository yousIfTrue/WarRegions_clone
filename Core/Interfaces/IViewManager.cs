using System;
using System.Collections.Generic;
using WarRegions.Core.Models;  // إذا كانت الinterface تحتاج أنواع الموديلز
// namespace: WarRegions.Core.Interfaces

namespace WarRegions.Core.Interfaces
{
    public interface IViewManager
    {
        void RenderMap(List<Region> regions);
        void RenderArmyDetails(Army army);
        void ShowMessage(string message);
        void ClearScreen();
        string GetUserInput();
    }
}
