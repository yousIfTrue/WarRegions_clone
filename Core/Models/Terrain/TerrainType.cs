using System;
using System.Collections.Generic;
using System.Linq;

    namespace WarRegions.Core.Models.Terrain
    {
            // Core/Models/Terrain/TerrainType.cs
    // Dependencies:
    // - Units/MovementType.cs (for movement restrictions)
        public enum TerrainType
        {
            Plains,      // Open fields - normal movement
            Mountains,   // High peaks - blocks most movement
            Forest,      // Dense woods - slows movement
            River,       // Water flows - naval units only
            Desert,      // Sandy waste - slows movement
            Swamp,       // Marshy land - greatly slows movement
            Fortress     // Defensive structure - provides bonuses
        }
    
        public static class TerrainTypeExtensions
        {
            public static string GetDisplayName(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return "Plains";
                    case TerrainType.Mountains: return "Mountains";
                    case TerrainType.Forest: return "Forest";
                    case TerrainType.River: return "River";
                    case TerrainType.Desert: return "Desert";
                    case TerrainType.Swamp: return "Swamp";
                    case TerrainType.Fortress: return "Fortress";
                    default: return "Unknown";
                }
            }
    
            public static string GetDescription(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains:
                        return "Open fields ideal for cavalry and fast movement. No defensive bonuses.";
                    case TerrainType.Mountains:
                        return "Impassable peaks that block movement. Flying units can traverse them.";
                    case TerrainType.Forest:
                        return "Dense woodland that provides cover. Slows movement but offers defensive bonuses.";
                    case TerrainType.River:
                        return "Flowing water that can only be traversed by naval units. Blocks land movement.";
                    case TerrainType.Desert:
                        return "Sandy wasteland that slows movement and reduces combat effectiveness.";
                    case TerrainType.Swamp:
                        return "Marshy terrain that greatly slows movement and hampers heavy units.";
                    case TerrainType.Fortress:
                        return "Fortified structure that provides significant defensive advantages.";
                    default:
                        return "Unknown terrain type.";
                }
            }
    
            public static char GetMapSymbol(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return '▒';
                    case TerrainType.Mountains: return '▲';
                    case TerrainType.Forest: return '♣';
                    case TerrainType.River: return '≈';
                    case TerrainType.Desert: return '░';
                    case TerrainType.Swamp: return '~';
                    case TerrainType.Fortress: return '⌂';
                    default: return '?';
                }
            }
    
            public static string GetEmoji(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return "🟩";
                    case TerrainType.Mountains: return "⛰️";
                    case TerrainType.Forest: return "🌲";
                    case TerrainType.River: return "🌊";
                    case TerrainType.Desert: return "🏜️";
                    case TerrainType.Swamp: return "🐊";
                    case TerrainType.Fortress: return "🏰";
                    default: return "❓";
                }
            }
    
            public static int GetDefenseBonus(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return 0;
                    case TerrainType.Mountains: return 5;
                    case TerrainType.Forest: return 2;
                    case TerrainType.River: return -1;
                    case TerrainType.Desert: return -2;
                    case TerrainType.Swamp: return -3;
                    case TerrainType.Fortress: return 10;
                    default: return 0;
                }
            }
    
            public static int GetMovementCost(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return 1;
                    case TerrainType.Mountains: return 99; // Impassable
                    case TerrainType.Forest: return 2;
                    case TerrainType.River: return 3;
                    case TerrainType.Desert: return 2;
                    case TerrainType.Swamp: return 4;
                    case TerrainType.Fortress: return 1;
                    default: return 1;
                }
            }
    
            public static int GetSilverProduction(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return 10;
                    case TerrainType.Mountains: return 5;
                    case TerrainType.Forest: return 8;
                    case TerrainType.River: return 6;
                    case TerrainType.Desert: return 4;
                    case TerrainType.Swamp: return 3;
                    case TerrainType.Fortress: return 2;
                    default: return 5;
                }
            }
    
            public static int GetGoldProduction(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return 1;
                    case TerrainType.Mountains: return 3;
                    case TerrainType.Forest: return 2;
                    case TerrainType.River: return 1;
                    case TerrainType.Desert: return 0;
                    case TerrainType.Swamp: return 2;
                    case TerrainType.Fortress: return 5;
                    default: return 1;
                }
            }
    
            public static bool IsPassable(this TerrainType terrain, MovementType movementType)
            {
                switch (terrain)
                {
                    case TerrainType.Mountains:
                        return movementType == MovementType.Flying;
                    
                    case TerrainType.River:
                        return movementType == MovementType.Naval || movementType == MovementType.Flying;
                    
                    case TerrainType.Swamp:
                        return movementType != MovementType.Siege; // Siege units cannot traverse swamps
                    
                    default:
                        return true;
                }
            }
    
            public static ConsoleColor GetConsoleColor(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return ConsoleColor.Green;
                    case TerrainType.Mountains: return ConsoleColor.DarkGray;
                    case TerrainType.Forest: return ConsoleColor.DarkGreen;
                    case TerrainType.River: return ConsoleColor.Blue;
                    case TerrainType.Desert: return ConsoleColor.Yellow;
                    case TerrainType.Swamp: return ConsoleColor.DarkGreen;
                    case TerrainType.Fortress: return ConsoleColor.Red;
                    default: return ConsoleColor.White;
                }
            }
    
            public static bool CanBuildStructure(this TerrainType terrain)
            {
                switch (terrain)
                {
                    case TerrainType.Plains: return true;
                    case TerrainType.Forest: return true;
                    case TerrainType.Desert: return false;
                    case TerrainType.Swamp: return false;
                    case TerrainType.River: return false;
                    case TerrainType.Mountains: return false;
                    case TerrainType.Fortress: return true;
                    default: return false;
                }
            }
        }
    }
