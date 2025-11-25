
    namespace WarRegions.Core.Models.Units
    {
            // Core/Models/Units/MovementType.cs
    // Dependencies:
    // - Terrain/TerrainType.cs (for terrain interaction)
        public enum MovementType
        {
            Infantry,    // Ground units - good in forests
            Cavalry,     // Fast ground units - good on plains
            Archer,      // Ranged units - standard movement
            Siege,       // Slow heavy units - poor in rough terrain
            Naval,       // Water units - can only move on water
            Flying       // Air units - ignores terrain obstacles
        }
    
        public static class MovementTypeExtensions
        {
            public static string GetDescription(this MovementType movementType)
            {
                switch (movementType)
                {
                    case MovementType.Infantry:
                        return "Standard ground unit. Good in forests, average elsewhere.";
                    case MovementType.Cavalry:
                        return "Fast moving unit. Excellent on plains, poor in rough terrain.";
                    case MovementType.Archer:
                        return "Ranged combat unit. Standard movement capabilities.";
                    case MovementType.Siege:
                        return "Heavy weaponry unit. Very slow but powerful attacks.";
                    case MovementType.Naval:
                        return "Water-based unit. Can only traverse rivers and coastal regions.";
                    case MovementType.Flying:
                        return "Air unit. Ignores terrain obstacles and moves freely.";
                    default:
                        return "Unknown movement type.";
                }
            }
    
            public static int GetBaseSpeed(this MovementType movementType)
            {
                switch (movementType)
                {
                    case MovementType.Infantry: return 100;
                    case MovementType.Cavalry: return 150;
                    case MovementType.Archer: return 110;
                    case MovementType.Siege: return 60;
                    case MovementType.Naval: return 120;
                    case MovementType.Flying: return 180;
                    default: return 100;
                }
            }
    
            public static bool CanTraverseTerrain(this MovementType movementType, TerrainType terrain)
            {
                switch (movementType)
                {
                    case MovementType.Naval:
                        return terrain == TerrainType.River; // Naval units can only move on rivers
                    
                    case MovementType.Flying:
                        return true; // Flying units can traverse any terrain
                    
                    case MovementType.Siege:
                        return terrain != TerrainType.Mountains && 
                               terrain != TerrainType.Swamp; // Siege units cannot traverse mountains/swamps
                    
                    default:
                        return terrain != TerrainType.Mountains; // Other units cannot traverse mountains
                }
            }
    
            public static double GetTerrainSpeedModifier(this MovementType movementType, TerrainType terrain)
            {
                if (!movementType.CanTraverseTerrain(terrain))
                    return 0.0; // Cannot move on this terrain
    
                switch (movementType)
                {
                    case MovementType.Infantry:
                        switch (terrain)
                        {
                            case TerrainType.Forest: return 0.8;  // Slower in forests
                            case TerrainType.Plains: return 1.0;  // Normal on plains
                            case TerrainType.Desert: return 0.7;  // Slower in desert
                            case TerrainType.Swamp: return 0.5;   // Much slower in swamp
                            case TerrainType.River: return 0.3;   // Very slow in rivers
                            default: return 1.0;
                        }
    
                    case MovementType.Cavalry:
                        switch (terrain)
                        {
                            case TerrainType.Plains: return 1.5;  // Faster on plains
                            case TerrainType.Forest: return 0.6;  // Slower in forests
                            case TerrainType.Desert: return 0.8;  // Slower in desert
                            case TerrainType.Swamp: return 0.3;   // Very slow in swamp
                            case TerrainType.River: return 0.2;   // Extremely slow in rivers
                            default: return 1.0;
                        }
    
                    case MovementType.Archer:
                        switch (terrain)
                        {
                            case TerrainType.Forest: return 0.9;  // Slightly slower in forests
                            case TerrainType.Plains: return 1.0;  // Normal on plains
                            case TerrainType.Desert: return 0.8;  // Slower in desert
                            case TerrainType.Swamp: return 0.6;   // Slower in swamp
                            case TerrainType.River: return 0.4;   // Slow in rivers
                            default: return 1.0;
                        }
    
                    case MovementType.Siege:
                        switch (terrain)
                        {
                            case TerrainType.Plains: return 0.8;  // Slow even on plains
                            case TerrainType.Forest: return 0.4;  // Very slow in forests
                            case TerrainType.Desert: return 0.5;  // Slow in desert
                            case TerrainType.River: return 0.1;   // Barely moves in rivers
                            default: return 0.8;
                        }
    
                    case MovementType.Naval:
                        switch (terrain)
                        {
                            case TerrainType.River: return 1.2;   // Fast in rivers
                            default: return 0.0;                  // Cannot move on other terrain
                        }
    
                    case MovementType.Flying:
                        return 1.0; // Consistent speed everywhere
    
                    default:
                        return 1.0;
                }
            }
    
            public static string GetIcon(this MovementType movementType)
            {
                switch (movementType)
                {
                    case MovementType.Infantry: return "⚔️";
                    case MovementType.Cavalry: return "🐎";
                    case MovementType.Archer: return "🏹";
                    case MovementType.Siege: return "🗡️";
                    case MovementType.Naval: return "⛵";
                    case MovementType.Flying: return "🦅";
                    default: return "❓";
                }
            }
    
            public static string GetName(this MovementType movementType)
            {
                return movementType.ToString();
            }
        }
    }
