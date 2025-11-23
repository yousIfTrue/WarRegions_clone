// Core/Controllers/TerrainManager.cs
// Dependencies:
// - Models/Terrain/TerrainType.cs
// - Models/Terrain/TerrainTile.cs
// - Models/Region.cs
// - Models/Units/MovementType.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegionsClone.Controllers
{
    public class TerrainManager
    {
        private List<TerrainTile> _terrainTiles;
        private int _mapWidth;
        private int _mapHeight;
        
        public TerrainManager()
        {
            _terrainTiles = new List<TerrainTile>();
            Console.WriteLine("[TERRAIN] TerrainManager initialized");
        }
        
        public void GenerateMap(int width, int height, int? seed = null)
        {
            _mapWidth = width;
            _mapHeight = height;
            _terrainTiles.Clear();
            
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TerrainType terrain = GenerateTerrainAt(x, y, random);
                    var tile = new TerrainTile(x, y, terrain);
                    _terrainTiles.Add(tile);
                }
            }
            
            Console.WriteLine($"[TERRAIN] Generated {width}x{height} map with {_terrainTiles.Count} tiles");
        }
        
        private TerrainType GenerateTerrainAt(int x, int y, Random random)
        {
            // Simple terrain generation algorithm
            double noise = random.NextDouble();
            
            // Create some interesting terrain patterns
            double distanceFromCenter = Math.Sqrt(Math.Pow(x - _mapWidth / 2, 2) + Math.Pow(y - _mapHeight / 2, 2));
            double maxDistance = Math.Sqrt(Math.Pow(_mapWidth / 2, 2) + Math.Pow(_mapHeight / 2, 2));
            double normalizedDistance = distanceFromCenter / maxDistance;
            
            // Edge regions are more likely to be mountains or water
            if (normalizedDistance > 0.8)
            {
                if (noise < 0.3) return TerrainType.Mountains;
                if (noise < 0.5) return TerrainType.River;
            }
            
            // Center regions are more likely to be plains
            if (normalizedDistance < 0.3)
            {
                if (noise < 0.6) return TerrainType.Plains;
                if (noise < 0.8) return TerrainType.Forest;
            }
            
            // Default distribution
            if (noise < 0.4) return TerrainType.Plains;
            if (noise < 0.6) return TerrainType.Forest;
            if (noise < 0.7) return TerrainType.River;
            if (noise < 0.8) return TerrainType.Desert;
            if (noise < 0.9) return TerrainType.Swamp;
            return TerrainType.Mountains;
        }
        
        public void LoadMap(List<Region> regions)
        {
            _terrainTiles.Clear();
            
            foreach (var region in regions)
            {
                var tile = new TerrainTile(region.X, region.Y, region.Terrain)
                {
                    LinkedRegion = region
                };
                _terrainTiles.Add(tile);
            }
            
            _mapWidth = regions.Max(r => r.X) + 1;
            _mapHeight = regions.Max(r => r.Y) + 1;
            
            Console.WriteLine($"[TERRAIN] Loaded map from {regions.Count} regions ({_mapWidth}x{_mapHeight})");
        }
        
        public TerrainTile GetTileAt(int x, int y)
        {
            return _terrainTiles.FirstOrDefault(t => t.X == x && t.Y == y);
        }
        
        public List<TerrainTile> GetTilesInRange(int centerX, int centerY, int range)
        {
            return _terrainTiles.Where(t => 
                Math.Abs(t.X - centerX) <= range && 
                Math.Abs(t.Y - centerY) <= range
            ).ToList();
        }
        
        public List<TerrainTile> GetPassableTiles(MovementType movementType, int centerX, int centerY, int range)
        {
            return GetTilesInRange(centerX, centerY, range)
                .Where(tile => tile.GetMovementCost(movementType) < 99) // Exclude impassable
                .ToList();
        }
        
        public int CalculateMovementCost(MovementType movementType, int fromX, int fromY, int toX, int toY)
        {
            var fromTile = GetTileAt(fromX, fromY);
            var toTile = GetTileAt(toX, toY);
            
            if (fromTile == null || toTile == null)
                return 99; // Impassable if tiles don't exist
            
            // Base cost is the cost of the destination tile
            int baseCost = toTile.GetMovementCost(movementType);
            
            // Additional cost for elevation changes
            double heightDifference = Math.Abs(toTile.Height - fromTile.Height);
            if (heightDifference > 0.3)
            {
                baseCost += (int)(heightDifference * 10);
            }
            
            return baseCost;
        }
        
        public bool CanTraverse(MovementType movementType, int fromX, int fromY, int toX, int toY)
        {
            var toTile = GetTileAt(toX, toY);
            if (toTile == null) return false;
            
            return toTile.GetMovementCost(movementType) < 99;
        }
        
        public List<TerrainTile> FindPath(MovementType movementType, int startX, int startY, int targetX, int targetY, int maxCost = 100)
        {
            var openSet = new List<TerrainTile>();
            var closedSet = new List<TerrainTile>();
            var cameFrom = new Dictionary<TerrainTile, TerrainTile>();
            var gScore = new Dictionary<TerrainTile, int>();
            var fScore = new Dictionary<TerrainTile, int>();
            
            var startTile = GetTileAt(startX, startY);
            var targetTile = GetTileAt(targetX, targetY);
            
            if (startTile == null || targetTile == null)
                return new List<TerrainTile>();
            
            openSet.Add(startTile);
            gScore[startTile] = 0;
            fScore[startTile] = HeuristicCost(startTile, targetTile);
            
            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(t => fScore.ContainsKey(t) ? fScore[t] : int.MaxValue).First();
                
                if (current == targetTile)
                {
                    return ReconstructPath(cameFrom, current);
                }
                
                openSet.Remove(current);
                closedSet.Add(current);
                
                foreach (var neighbor in GetNeighbors(current.X, current.Y))
                {
                    if (closedSet.Contains(neighbor))
                        continue;
                    
                    if (!CanTraverse(movementType, current.X, current.Y, neighbor.X, neighbor.Y))
                        continue;
                    
                    int tentativeGScore = gScore[current] + CalculateMovementCost(movementType, current.X, current.Y, neighbor.X, neighbor.Y);
                    
                    if (tentativeGScore > maxCost)
                        continue;
                    
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                    else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, int.MaxValue))
                        continue;
                    
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + HeuristicCost(neighbor, targetTile);
                }
            }
            
            return new List<TerrainTile>(); // No path found
        }
        
        private List<TerrainTile> GetNeighbors(int x, int y)
        {
            var neighbors = new List<TerrainTile>();
            
            // Check all 4 directions
            var directions = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            
            foreach (var (dx, dy) in directions)
            {
                var neighbor = GetTileAt(x + dx, y + dy);
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
            
            return neighbors;
        }
        
        private int HeuristicCost(TerrainTile from, TerrainTile to)
        {
            // Manhattan distance
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }
        
        private List<TerrainTile> ReconstructPath(Dictionary<TerrainTile, TerrainTile> cameFrom, TerrainTile current)
        {
            var path = new List<TerrainTile> { current };
            
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            
            return path;
        }
        
        public void UpdateTileVisibility(Player player, List<Army> armies)
        {
            // Reset visibility
            foreach (var tile in _terrainTiles)
            {
                tile.SetVisibility(false);
            }
            
            // Set tiles visible based on army positions and vision
            foreach (var army in armies.Where(a => a.Owner == player))
            {
                var visibleTiles = GetTilesInRange(army.CurrentRegion.X, army.CurrentRegion.Y, GetArmyVisionRange(army));
                foreach (var tile in visibleTiles)
                {
                    tile.Explore();
                    tile.SetVisibility(true);
                }
            }
        }
        
        private int GetArmyVisionRange(Army army)
        {
            if (army.Units.Count == 0) return 1;
            
            // Vision range is based on the unit with the highest vision
            return army.Units.Max(u => u.Stats.Vision);
        }
        
        public void BuildRoad(int x, int y)
        {
            var tile = GetTileAt(x, y);
            if (tile != null && tile.CanBuildRoad())
            {
                tile.BuildRoad();
                Console.WriteLine($"[TERRAIN] Road built at ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"[TERRAIN] Cannot build road at ({x}, {y})");
            }
        }
        
        public void BuildBridge(int x, int y)
        {
            var tile = GetTileAt(x, y);
            if (tile != null && tile.CanBuildBridge())
            {
                tile.BuildBridge();
                Console.WriteLine($"[TERRAIN] Bridge built at ({x}, {y})");
            }
            else
            {
                Console.WriteLine($"[TERRAIN] Cannot build bridge at ({x}, {y})");
            }
        }
        
        public string GetMapPreview()
        {
            if (_terrainTiles.Count == 0)
                return "No map generated";
            
            var preview = new System.Text.StringBuilder();
            preview.AppendLine("Map Preview:");
            
            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    var tile = GetTileAt(x, y);
                    if (tile != null)
                    {
                        preview.Append(tile.Terrain.GetMapSymbol());
                    }
                    else
                    {
                        preview.Append('?');
                    }
                    preview.Append(' ');
                }
                preview.AppendLine();
            }
            
            return preview.ToString();
        }
        
        public string GetTerrainDistribution()
        {
            var distribution = _terrainTiles
                .GroupBy(t => t.Terrain)
                .Select(g => new { Terrain = g.Key, Count = g.Count(), Percentage = (double)g.Count() / _terrainTiles.Count * 100 })
                .OrderByDescending(x => x.Count);
            
            var report = new System.Text.StringBuilder();
            report.AppendLine("Terrain Distribution:");
            
            foreach (var item in distribution)
            {
                report.AppendLine($"{item.Terrain}: {item.Count} tiles ({item.Percentage:F1}%)");
            }
            
            return report.ToString();
        }
        
        public void ClearMap()
        {
            _terrainTiles.Clear();
            _mapWidth = 0;
            _mapHeight = 0;
            Console.WriteLine("[TERRAIN] Map cleared");
        }
    }
}