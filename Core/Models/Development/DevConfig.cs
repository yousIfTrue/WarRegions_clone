// Core/Models/Development/DevConfig.cs
namespace WarRegions.Core.Models.Development
{
    public static class DevConfig
    {
        public static bool DebugMode => false;
        public static bool EnableCheats => false;
        public static bool SkipTutorial => true;
        
        // ✅ إضافة جميع الخصائص المطلوبة
        public static bool EnableShopSystem => true;
        public static bool EnableWorkshopSystem => true;
        public static bool EnablePathfindingSystem => true;
        public static bool EnableEconomySystem => true;
        public static bool EnableBattleSystem => true;
        public static bool EnableDeckSystem => true;
        
        public static double UnitUpgradeCostMultiplier => 1.0;
        public static double BuildingUpgradeCostMultiplier => 1.0;
        public static double UnitRecruitmentCostMultiplier => 1.0;
        public static double BattleDamageMultiplier => 1.0;
        
        public static int BaseMovementRange => 3;
        public static double TerrainCostMultiplier => 1.0;
        
        public static double SilverProductionMultiplier => 1.0;
        public static double GoldProductionMultiplier => 1.0;
        
        public static bool UnlockAllUnits => false;
        public static bool InfiniteResources => false;
        
        public static void Log(string message)
        {
            if (DebugMode) Console.WriteLine($"[DEV] {message}");
        }
    }
}


/*
namespace WarRegions.Core.Models.Development
{
    public static class DevConfig
    {
        // إعدادات التصحيح
        public static bool DebugMode => false;
        public static bool EnableCheats => false;
        public static bool SkipTutorial => true;
        public static bool LogBattleDetails => false;

        // مضاعفات التكلفة
        public static double UnitUpgradeCostMultiplier => 1.0;
        public static double BuildingUpgradeCostMultiplier => 1.0;
        public static double UnitRecruitmentCostMultiplier => 1.0;

        // إعدادات المعارك
        public static double BattleDamageMultiplier => 1.0;
        public static double DefenseBonusMultiplier => 1.0;
        public static double AttackBonusMultiplier => 1.0;

        // إعدادات الحركة
        public static int BaseMovementRange => 3;
        public static double TerrainCostMultiplier => 1.0;

        // إعدادات الاقتصاد
        public static double SilverProductionMultiplier => 1.0;
        public static double GoldProductionMultiplier => 1.0;
        public static double ResourceGatheringRate => 1.0;

        // إعدادات التطوير
        public static bool UnlockAllUnits => false;
        public static bool InfiniteResources => false;
        public static bool InstantConstruction => false;

        // طرق مساعدة
        public static void Log(string message)
        {
            if (DebugMode)
            {
                Console.WriteLine($"[DEV] {message}");
            }
        }

        public static void LogWarning(string message)
        {
            if (DebugMode)
            {
                Console.WriteLine($"[DEV-WARNING] {message}");
            }
        }

        public static void LogError(string message)
        {
            Console.WriteLine($"[DEV-ERROR] {message}");
        }
    }
}
*/