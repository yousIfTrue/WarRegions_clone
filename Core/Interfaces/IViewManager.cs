using System;
using WarRegions.Core.Models;
using System.Collections.Generic;

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
