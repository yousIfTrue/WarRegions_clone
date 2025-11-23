// Core/Models/Development/DevConfig.cs
// Dependencies: None - this is a configuration class

using System;

namespace WarRegions.Models.Development
{
    public static class DevConfig
    {
        // Development flags - enable/disable features during development
        public static bool EnableShopSystem = false;
        public static bool EnableWorkshopSystem = false;
        public static bool EnableDailyOffers = false;
        public static bool EnableMultiplayer = false;
        public static bool EnableAchievements = false;
        
        // Debug and testing options
        public static bool DebugMode = true;
        public static bool InfiniteResources = true;
        public static bool AllUnitsUnlocked = true;
        public static bool SkipTutorial = true;
        public static bool GodMode = false;
        
        // Game balance overrides for testing
        public static int StartingSilver = 5000;
        public static int StartingGold = 500;
        public static int UnitUpgradeCostMultiplier = 1;
        public static double BattleDamageMultiplier = 1.0;
        
        // Performance and display options
        public static bool ShowDebugInfo = true;
        public static bool ShowPathfindingDebug = false;
        public static bool ShowAIDecisions = false;
        public static int MaxLogMessages = 100;
        
        // Game speed and automation
        public static double GameSpeedMultiplier = 1.0;
        public static bool AutoResolveBattles = false;
        public static bool AutoEndTurn = false;
        
        // Map generation options
        public static bool UseFixedSeed = true;
        public static int MapGenerationSeed = 12345;
        public static bool GenerateSimpleMaps = true;
        
        public static void PrintConfig()
        {
            if (!DebugMode) return;
            
            Console.WriteLine("=== Development Configuration ===");
            Console.WriteLine($"Shop System: {EnableShopSystem}");
            Console.WriteLine($"Workshop System: {EnableWorkshopSystem}");
            Console.WriteLine($"Infinite Resources: {InfiniteResources}");
            Console.WriteLine($"All Units Unlocked: {AllUnitsUnlocked}");
            Console.WriteLine($"Debug Mode: {DebugMode}");
            Console.WriteLine($"God Mode: {GodMode}");
            Console.WriteLine("=================================");
        }
        
        public static void ApplyDevelopmentCheats(Player player)
        {
            if (!DebugMode) return;
            
            if (InfiniteResources)
            {
                player.SilverCoins = Math.Max(player.SilverCoins, StartingSilver);
                player.GoldCoins = Math.Max(player.GoldCoins, StartingGold);
            }
            
            if (GodMode)
            {
                player.SilverCoins = 99999;
                player.GoldCoins = 9999;
                // Additional god mode effects can be added here
            }
            
            if (ShowDebugInfo)
            {
                Console.WriteLine($"[DEBUG] Player resources: {player.SilverCoins}S {player.GoldCoins}G");
            }
        }
        
        public static void ToggleGodMode()
        {
            GodMode = !GodMode;
            Console.WriteLine($"[DEBUG] God Mode: {(GodMode ? "ENABLED" : "DISABLED")}");
        }
        
        public static void ToggleInfiniteResources()
        {
            InfiniteResources = !InfiniteResources;
            Console.WriteLine($"[DEBUG] Infinite Resources: {(InfiniteResources ? "ENABLED" : "DISABLED")}");
        }
        
        public static void SetGameSpeed(double multiplier)
        {
            GameSpeedMultiplier = Math.Max(0.1, Math.Min(5.0, multiplier));
            Console.WriteLine($"[DEBUG] Game Speed set to: {GameSpeedMultiplier}x");
        }
        
        public static void EnableAllFeatures()
        {
            EnableShopSystem = true;
            EnableWorkshopSystem = true;
            EnableDailyOffers = true;
            EnableMultiplayer = true;
            EnableAchievements = true;
            Console.WriteLine("[DEBUG] All features enabled");
        }
        
        public static void DisableAllFeatures()
        {
            EnableShopSystem = false;
            EnableWorkshopSystem = false;
            EnableDailyOffers = false;
            EnableMultiplayer = false;
            EnableAchievements = false;
            Console.WriteLine("[DEBUG] All features disabled");
        }
        
        public static string GetConfigSummary()
        {
            return $"""
            Development Configuration:
            - Debug: {DebugMode}
            - God Mode: {GodMode}
            - Infinite Resources: {InfiniteResources}
            - All Units: {AllUnitsUnlocked}
            - Game Speed: {GameSpeedMultiplier}x
            - Shop: {EnableShopSystem}
            - Workshop: {EnableWorkshopSystem}
            """;
        }
        
        public static void ResetToDefault()
        {
            EnableShopSystem = false;
            EnableWorkshopSystem = false;
            EnableDailyOffers = false;
            EnableMultiplayer = false;
            EnableAchievements = false;
            DebugMode = true;
            InfiniteResources = true;
            AllUnitsUnlocked = true;
            SkipTutorial = true;
            GodMode = false;
            StartingSilver = 5000;
            StartingGold = 500;
            UnitUpgradeCostMultiplier = 1;
            BattleDamageMultiplier = 1.0;
            ShowDebugInfo = true;
            ShowPathfindingDebug = false;
            ShowAIDecisions = false;
            MaxLogMessages = 100;
            GameSpeedMultiplier = 1.0;
            AutoResolveBattles = false;
            AutoEndTurn = false;
            UseFixedSeed = true;
            MapGenerationSeed = 12345;
            GenerateSimpleMaps = true;
            
            Console.WriteLine("[DEBUG] Configuration reset to defaults");
        }
    }
}