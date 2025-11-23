#!/bin/bash
echo "🔧 إصلاح هيكل الملفات التالفة..."

# إصلاح IViewManager.cs
cat > Core/Interfaces/IViewManager.cs << 'IEOF'
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
IEOF

# إصلاح DefaultUnits.cs
cat > Core/Models/Development/DefaultUnits.cs << 'DEOF'
using System.Collections.Generic;
using WarRegions.Core.Models.Units;

namespace WarRegions.Core.Models.Development
{
    public static class DefaultUnits
    {
        public static List<UnitCard> CreateStarterUnits()
        {
            return new List<UnitCard>
            {
                // سيتم إضافة الوحدات الأساسية هنا لاحقاً
            };
        }
        
        public static List<UnitCard> CreateEnemyUnits()
        {
            return new List<UnitCard>
            {
                // سيتم إضافة وحدات العدو هنا لاحقاً
            };
        }
    }
}
DEOF

echo "✅ تم إصلاح هيكل الملفات!"
