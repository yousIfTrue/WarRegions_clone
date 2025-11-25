// Core/Models/Development/DevConfig.cs
namespace WarRegions.Core.Models.Development
{
    public static class DevConfig
    {
        public static bool DebugMode => false;
        public static bool EnableCheats => true;
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
        
        public static bool UnlockAllUnits => true;
        public static bool InfiniteResources => false;
        
        public static void ApplyDevelopmentCheats(GameState gameState)
        {
            if (EnableCheats)
            {
                Console.WriteLine("[DEV] Applying development cheats...");
        
                // كود الغش هنا
                if (InfiniteResources)
                {
                    // تعيين موارد لا نهائية لكل لاعب
                    foreach (var player in gameState.Players)
                    {
                        player.Silver = 99999;
                        player.Gold = 9999;
                        player.Morale = 100;
                    }
                    Console.WriteLine("[DEV] Infinite resources activated");
                }
        
                if (UnlockAllUnits)
                {
                    // فتح جميع الوحدات لكل لاعب
                    var allUnitTypes = new[] { "infantry", "archer", "cavalry", "siege", "naval", "flying" };
                    foreach (var player in gameState.Players)
                    {
                        foreach (var unitType in allUnitTypes)
                        {
                            if (!player.UnlockedUnits.Contains(unitType))
                            {
                                player.UnlockedUnits.Add(unitType);
                            }
                        }
                    }
                    Console.WriteLine("[DEV] All units unlocked");
                }
        
                if (DebugMode)
                {
                    // إعدادات التصحيح الإضافية
                    gameState.TurnNumber = 1; // بداية من الدور الأول
                    Console.WriteLine("[DEV] Debug mode enhancements applied");
                }
            }
        }
    }
}
