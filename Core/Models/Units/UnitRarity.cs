using System;
using System.Collections.Generic;
using System.Linq;

    namespace WarRegions.Core.Models.Units
    {
    // Core/Models/Units/UnitRarity.cs
    // Dependencies: None - this is a basic enum

        public enum UnitRarity
        {
            Common,      // Basic units - easy to obtain
            Rare,        // Better stats - moderate rarity
            Epic,        // Strong units - hard to obtain
            Legendary    // Best units - very rare
        }
    
        public static class UnitRarityExtensions
        {
            public static string GetDisplayName(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return "Common";
                    case UnitRarity.Rare: return "Rare";
                    case UnitRarity.Epic: return "Epic";
                    case UnitRarity.Legendary: return "Legendary";
                    default: return "Unknown";
                }
            }
    
            public static string GetColorCode(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return "#FFFFFF";      // White
                    case UnitRarity.Rare: return "#0070DD";        // Blue
                    case UnitRarity.Epic: return "#A335EE";        // Purple
                    case UnitRarity.Legendary: return "#FF8000";   // Orange
                    default: return "#FFFFFF";
                }
            }
    
            public static int GetBaseCostMultiplier(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return 1;
                    case UnitRarity.Rare: return 3;
                    case UnitRarity.Epic: return 8;
                    case UnitRarity.Legendary: return 20;
                    default: return 1;
                }
            }
    
            public static int GetUpgradeCostMultiplier(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return 1;
                    case UnitRarity.Rare: return 2;
                    case UnitRarity.Epic: return 4;
                    case UnitRarity.Legendary: return 8;
                    default: return 1;
                }
            }
    
            public static double GetStatBonusMultiplier(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return 1.0;
                    case UnitRarity.Rare: return 1.3;
                    case UnitRarity.Epic: return 1.7;
                    case UnitRarity.Legendary: return 2.2;
                    default: return 1.0;
                }
            }
    
            public static int GetMaxLevel(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return 5;
                    case UnitRarity.Rare: return 7;
                    case UnitRarity.Epic: return 9;
                    case UnitRarity.Legendary: return 12;
                    default: return 5;
                }
            }
    
            public static string GetIcon(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return "⚪";
                    case UnitRarity.Rare: return "🔵";
                    case UnitRarity.Epic: return "🟣";
                    case UnitRarity.Legendary: return "🟠";
                    default: return "⚪";
                }
            }
    
            public static string GetDescription(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common:
                        return "Basic units that form the backbone of any army. Easy to recruit and maintain.";
                    case UnitRarity.Rare:
                        return "Specialized units with improved capabilities. Require more resources but offer better performance.";
                    case UnitRarity.Epic:
                        return "Elite units with exceptional combat abilities. Difficult to obtain but can turn the tide of battle.";
                    case UnitRarity.Legendary:
                        return "Legendary units of immense power. Extremely rare and can single-handedly dominate the battlefield.";
                    default:
                        return "Unknown unit rarity.";
                }
            }
    
            public static UnitRarity GetNextRarity(this UnitRarity rarity)
            {
                switch (rarity)
                {
                    case UnitRarity.Common: return UnitRarity.Rare;
                    case UnitRarity.Rare: return UnitRarity.Epic;
                    case UnitRarity.Epic: return UnitRarity.Legendary;
                    case UnitRarity.Legendary: return UnitRarity.Legendary; // Cannot upgrade beyond legendary
                    default: return UnitRarity.Common;
                }
            }
    
            public static bool CanUpgradeRarity(this UnitRarity currentRarity, int unitLevel)
            {
                switch (currentRarity)
                {
                    case UnitRarity.Common:
                        return unitLevel >= 3; // Common units can upgrade to rare at level 3
                    case UnitRarity.Rare:
                        return unitLevel >= 5; // Rare units can upgrade to epic at level 5
                    case UnitRarity.Epic:
                        return unitLevel >= 8; // Epic units can upgrade to legendary at level 8
                    case UnitRarity.Legendary:
                        return false; // Legendary units cannot upgrade further
                    default:
                        return false;
                }
            }
    
            public static int GetRarityUpgradeCost(this UnitRarity currentRarity)
            {
                switch (currentRarity)
                {
                    case UnitRarity.Common: return 500; // Silver cost to upgrade from common to rare
                    case UnitRarity.Rare: return 1000;  // Silver cost to upgrade from rare to epic
                    case UnitRarity.Epic: return 2000;  // Silver cost to upgrade from epic to legendary
                    default: return 0;
                }
            }
        }
    }
