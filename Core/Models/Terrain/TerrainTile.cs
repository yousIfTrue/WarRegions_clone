// Core/Models/Terrain/TerrainTile.cs
// Dependencies:
// - TerrainType.cs (for Terrain property)
// - Region.cs (for linked region)

using System;

namespace WarRegionsClone.Models.Terrain
{
    public class TerrainTile
    {
        public string TileId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public TerrainType Terrain { get; set; }
        
        // Tile properties
        public double Height { get; set; } // For 3D visualization
        public bool IsExplored { get; set; }
        public bool IsVisible { get; set; }
        
        // Resource information
        public int ResourceAmount { get; set; }
        public string ResourceType { get; set; }
        
        // Structures and improvements
        public bool HasRoad { get; set; }
        public bool HasBridge { get; set; }
        public string StructureType { get; set; }
        
        // Combat modifiers
        public int CoverBonus { get; set; }
        public int VisibilityModifier { get; set; }
        
        // Linked game objects
        public Region LinkedRegion { get; set; }
        
        public TerrainTile()
        {
            TileId = Guid.NewGuid().ToString();
            IsExplored = false;
            IsVisible = false;
            ResourceAmount = 0;
            CoverBonus = 0;
            VisibilityModifier = 0;
        }
        
        public TerrainTile(int x, int y, TerrainType terrain) : this()
        {
            X = x;
            Y = y;
            Terrain = terrain;
            InitializeTileProperties();
        }
        
        private void InitializeTileProperties()
        {
            // Set properties based on terrain type
            switch (Terrain)
            {
                case TerrainType.Plains:
                    Height = 0.1;
                    CoverBonus = 0;
                    VisibilityModifier = 0;
                    HasRoad = true; // Plains often have natural paths
                    break;
                    
                case TerrainType.Mountains:
                    Height = 0.8 + (new Random().NextDouble() * 0.4); // Random height variation
                    CoverBonus = 3;
                    VisibilityModifier = 2; // High ground advantage
                    HasRoad = false;
                    break;
                    
                case TerrainType.Forest:
                    Height = 0.3;
                    CoverBonus = 2;
                    VisibilityModifier = -1; // Reduced visibility in forests
                    HasRoad = false;
                    break;
                    
                case TerrainType.River:
                    Height = -0.2;
                    CoverBonus = 0;
                    VisibilityModifier = 0;
                    HasRoad = false;
                    break;
                    
                case TerrainType.Desert:
                    Height = 0.2;
                    CoverBonus = 0;
                    VisibilityModifier = 1; // Better visibility in open desert
                    HasRoad = false;
                    break;
                    
                case TerrainType.Swamp:
                    Height = -0.1;
                    CoverBonus = 1;
                    VisibilityModifier = -1;
                    HasRoad = false;
                    break;
                    
                case TerrainType.Fortress:
                    Height = 0.4;
                    CoverBonus = 5;
                    VisibilityModifier = 3;
                    HasRoad = true;
                    StructureType = "Fortress";
                    break;
            }
            
            // Initialize resources based on terrain
            InitializeResources();
        }
        
        private void InitializeResources()
        {
            var random = new Random(X * 1000 + Y);
            
            switch (Terrain)
            {
                case TerrainType.Mountains:
                    if (random.NextDouble() < 0.3) // 30% chance of resources
                    {
                        ResourceType = "Gold";
                        ResourceAmount = random.Next(1, 5);
                    }
                    break;
                    
                case TerrainType.Forest:
                    if (random.NextDouble() < 0.4) // 40% chance of resources
                    {
                        ResourceType = "Wood";
                        ResourceAmount = random.Next(2, 8);
                    }
                    break;
                    
                case TerrainType.River:
                    if (random.NextDouble() < 0.2) // 20% chance of resources
                    {
                        ResourceType = "Fish";
                        ResourceAmount = random.Next(1, 4);
                    }
                    break;
                    
                case TerrainType.Plains:
                    if (random.NextDouble() < 0.5) // 50% chance of resources
                    {
                        ResourceType = "Food";
                        ResourceAmount = random.Next(3, 10);
                    }
                    break;
            }
        }
        
        public int GetMovementCost(MovementType movementType)
        {
            int baseCost = Terrain.GetMovementCost();
            
            // Adjust cost based on movement type
            if (!Terrain.IsPassable(movementType))
                return 99; // Impassable
            
            // Apply movement type modifiers
            double modifier = movementType.GetTerrainSpeedModifier(Terrain);
            int adjustedCost = (int)(baseCost / modifier);
            
            // Road bonus - reduces movement cost
            if (HasRoad && movementType != MovementType.Naval && movementType != MovementType.Flying)
            {
                adjustedCost = Math.Max(1, adjustedCost / 2);
            }
            
            return Math.Max(1, adjustedCost);
        }
        
        public int GetCombatBonus()
        {
            int bonus = Terrain.GetDefenseBonus() + CoverBonus;
            
            // Structure bonuses
            if (!string.IsNullOrEmpty(StructureType))
            {
                switch (StructureType)
                {
                    case "Fortress": bonus += 5; break;
                    case "Tower": bonus += 3; break;
                    case "Wall": bonus += 2; break;
                }
            }
            
            return bonus;
        }
        
        public bool CanBuildRoad()
        {
            return Terrain != TerrainType.Mountains && 
                   Terrain != TerrainType.River &&
                   Terrain != TerrainType.Swamp;
        }
        
        public bool CanBuildBridge()
        {
            return Terrain == TerrainType.River;
        }
        
        public void BuildRoad()
        {
            if (CanBuildRoad())
            {
                HasRoad = true;
                Console.WriteLine($"Road built at tile ({X}, {Y})");
            }
            else
            {
                Console.WriteLine($"Cannot build road on {Terrain} terrain at ({X}, {Y})");
            }
        }
        
        public void BuildBridge()
        {
            if (CanBuildBridge())
            {
                HasBridge = true;
                HasRoad = true; // Bridge implies road connection
                Console.WriteLine($"Bridge built at tile ({X}, {Y})");
            }
            else
            {
                Console.WriteLine($"Cannot build bridge on {Terrain} terrain at ({X}, {Y})");
            }
        }
        
        public void Explore()
        {
            IsExplored = true;
            IsVisible = true;
        }
        
        public void SetVisibility(bool visible)
        {
            IsVisible = visible && IsExplored;
        }
        
        public override string ToString()
        {
            string exploredStatus = IsExplored ? "Explored" : "Unexplored";
            string visibleStatus = IsVisible ? "Visible" : "Hidden";
            string resourceInfo = ResourceAmount > 0 ? $", {ResourceAmount} {ResourceType}" : "";
            string structureInfo = !string.IsNullOrEmpty(StructureType) ? $", {StructureType}" : "";
            
            return $"{Terrain} Tile ({X},{Y}) - {exploredStatus}, {visibleStatus}{resourceInfo}{structureInfo}";
        }
        
        public string ToShortString()
        {
            return $"{Terrain.GetEmoji()} ({X},{Y})";
        }
    }
}