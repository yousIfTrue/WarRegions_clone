// Core/Controllers/Pathfinding/HybridPathfinder.cs
// Dependencies:
// - IPathfinder.cs
// - Models/Terrain/TerrainTile.cs
// - Models/Units/MovementType.cs
// - CentralUnitPathfinder.cs
// - BlockBasedPathfinder.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WarRegionsClone.Controllers.Pathfinding
{
    public class HybridPathfinder : IPathfinder
    {
        private CentralUnitPathfinder _centralPathfinder;
        private BlockBasedPathfinder _blockPathfinder;
        private TerrainManager _terrainManager;
        
        // Configuration
        private double _blockPathThreshold = 0.7; // Use block-based if confidence > 70%
        private int _maxBlockSearchDistance = 5;
        
        // Performance tracking
        private int _centralPathfinderUses = 0;
        private int _blockPathfinderUses = 0;
        private long _totalComputationTime = 0;
        
        public HybridPathfinder(TerrainManager terrainManager)
        {
            _terrainManager = terrainManager;
            _centralPathfinder = new CentralUnitPathfinder(terrainManager);
            _blockPathfinder = new BlockBasedPathfinder(terrainManager);
            
            Console.WriteLine("[PATHFINDING] HybridPathfinder initialized");
        }
        
        public List<TerrainTile> FindPath(int startX, int startY, int targetX, int targetY, 
                                        MovementType movementType, int maxCost = 100)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // First, check if block-based pathfinding is appropriate
                if (ShouldUseBlockBasedPathfinding(startX, startY, targetX, targetY, movementType))
                {
                    _blockPathfinderUses++;
                    var blockPath = _blockPathfinder.FindPath(startX, startY, targetX, targetY, movementType, maxCost);
                    
                    if (blockPath.Count > 0 && CalculatePathConfidence(blockPath, movementType) > _blockPathThreshold)
                    {
                        stopwatch.Stop();
                        _totalComputationTime += stopwatch.ElapsedMilliseconds;
                        
                        Console.WriteLine($"[PATHFINDING] Used Block-Based pathfinding (Confidence: High)");
                        return blockPath;
                    }
                }
                
                // Fall back to central unit pathfinding
                _centralPathfinderUses++;
                var centralPath = _centralPathfinder.FindPath(startX, startY, targetX, targetY, movementType, maxCost);
                
                stopwatch.Stop();
                _totalComputationTime += stopwatch.ElapsedMilliseconds;
                
                Console.WriteLine($"[PATHFINDING] Used Central Unit pathfinding");
                return centralPath;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"[PATHFINDING ERROR] Hybrid pathfinding failed: {ex.Message}");
                return new List<TerrainTile>();
            }
        }
        
        private bool ShouldUseBlockBasedPathfinding(int startX, int startY, int targetX, int targetY, MovementType movementType)
        {
            // Check distance - block-based works better for shorter distances
            int distance = Math.Abs(targetX - startX) + Math.Abs(targetY - startY);
            if (distance > _maxBlockSearchDistance)
                return false;
            
            // Check if there are block markers along the potential path
            int blockMarkersCount = CountBlockMarkersAlongPath(startX, startY, targetX, targetY);
            if (blockMarkersCount == 0)
                return false;
            
            // Check terrain complexity - block-based works better in simple terrain
            double terrainComplexity = CalculateTerrainComplexity(startX, startY, targetX, targetY, movementType);
            if (terrainComplexity > 0.5) // More than 50% complex terrain
                return false;
            
            return true;
        }
        
        private int CountBlockMarkersAlongPath(int startX, int startY, int targetX, int targetY)
        {
            // This would check for actual block markers in the game world
            // For now, simulate based on distance and position
            
            int markers = 0;
            int steps = Math.Max(Math.Abs(targetX - startX), Math.Abs(targetY - startY));
            
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                int currentX = (int)(startX + (targetX - startX) * t);
                int currentY = (int)(startY + (targetY - startY) * t);
                
                var tile = _terrainManager.GetTileAt(currentX, currentY);
                if (tile != null && tile.HasRoad)
                {
                    markers += 2; // Roads count as strong markers
                }
                else if (tile != null && IsNaturalPath(tile.Terrain))
                {
                    markers += 1; // Natural paths count as weak markers
                }
            }
            
            return markers;
        }
        
        private bool IsNaturalPath(TerrainType terrain)
        {
            return terrain == TerrainType.Plains || 
                   terrain == TerrainType.Forest ||
                   (terrain == TerrainType.River);
        }
        
        private double CalculateTerrainComplexity(int startX, int startY, int targetX, int targetY, MovementType movementType)
        {
            int complexTiles = 0;
            int totalTiles = 0;
            
            int steps = Math.Max(Math.Abs(targetX - startX), Math.Abs(targetY - startY));
            
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                int currentX = (int)(startX + (targetX - startX) * t);
                int currentY = (int)(startY + (targetY - startY) * t);
                
                var tile = _terrainManager.GetTileAt(currentX, currentY);
                if (tile != null)
                {
                    totalTiles++;
                    if (IsComplexTerrain(tile.Terrain, movementType))
                    {
                        complexTiles++;
                    }
                }
            }
            
            return totalTiles > 0 ? (double)complexTiles / totalTiles : 0;
        }
        
        private bool IsComplexTerrain(TerrainType terrain, MovementType movementType)
        {
            // Terrain is complex if it significantly affects movement for this unit type
            switch (movementType)
            {
                case MovementType.Infantry:
                    return terrain == TerrainType.Mountains || terrain == TerrainType.Swamp;
                case MovementType.Cavalry:
                    return terrain == TerrainType.Forest || terrain == TerrainType.Swamp || terrain == TerrainType.Mountains;
                case MovementType.Siege:
                    return terrain != TerrainType.Plains;
                case MovementType.Naval:
                    return terrain != TerrainType.River;
                case MovementType.Flying:
                    return false; // Flying units ignore terrain complexity
                default:
                    return terrain == TerrainType.Mountains || terrain == TerrainType.Swamp;
            }
        }
        
        private double CalculatePathConfidence(List<TerrainTile> path, MovementType movementType)
        {
            if (path.Count == 0) return 0;
            
            double confidence = 1.0;
            
            foreach (var tile in path)
            {
                // Reduce confidence for complex terrain
                if (IsComplexTerrain(tile.Terrain, movementType))
                {
                    confidence *= 0.8;
                }
                
                // Increase confidence for marked paths (roads, etc.)
                if (tile.HasRoad)
                {
                    confidence *= 1.2;
                }
                
                // Cap confidence between 0 and 1
                confidence = Math.Max(0, Math.Min(1, confidence));
            }
            
            return confidence;
        }
        
        public Dictionary<TerrainTile, int> FindReachablePositions(int startX, int startY, 
                                                                 MovementType movementType, int maxCost)
        {
            // Use central pathfinder for reachability analysis (more accurate)
            return _centralPathfinder.FindReachablePositions(startX, startY, movementType, maxCost);
        }
        
        public bool HasPath(int startX, int startY, int targetX, int targetY, 
                          MovementType movementType, int maxCost)
        {
            var path = FindPath(startX, startY, targetX, targetY, movementType, maxCost);
            return path.Count > 0 && CalculatePathCost(path, movementType) <= maxCost;
        }
        
        public int GetMovementCost(int fromX, int fromY, int toX, int toY, MovementType movementType)
        {
            // Delegate to terrain manager for individual tile costs
            return _terrainManager.CalculateMovementCost(movementType, fromX, fromY, toX, toY);
        }
        
        private int CalculatePathCost(List<TerrainTile> path, MovementType movementType)
        {
            if (path.Count < 2) return 0;
            
            int totalCost = 0;
            for (int i = 1; i < path.Count; i++)
            {
                totalCost += GetMovementCost(
                    path[i-1].X, path[i-1].Y,
                    path[i].X, path[i].Y,
                    movementType
                );
            }
            
            return totalCost;
        }
        
        public string GetName()
        {
            return "Hybrid Pathfinder";
        }
        
        public string GetDescription()
        {
            return "Combines block-based pathfinding for marked routes with central unit pathfinding for complex navigation. Optimized for performance and accuracy.";
        }
        
        public PathfindingResult FindPathWithDetails(int startX, int startY, int targetX, int targetY, 
                                                   MovementType movementType, int maxCost = 100)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new PathfindingResult();
            
            var path = FindPath(startX, startY, targetX, targetY, movementType, maxCost);
            
            stopwatch.Stop();
            
            result.Path = path;
            result.PathFound = path.Count > 0;
            result.TotalCost = CalculatePathCost(path, movementType);
            result.AlgorithmUsed = _blockPathfinderUses > _centralPathfinderUses ? "Block-Based" : "Central Unit";
            result.ComputationTimeMs = stopwatch.ElapsedMilliseconds;
            
            return result;
        }
        
        public string GetPerformanceStats()
        {
            int totalUses = _centralPathfinderUses + _blockPathfinderUses;
            double centralPercentage = totalUses > 0 ? (double)_centralPathfinderUses / totalUses * 100 : 0;
            double blockPercentage = totalUses > 0 ? (double)_blockPathfinderUses / totalUses * 100 : 0;
            double averageTime = totalUses > 0 ? (double)_totalComputationTime / totalUses : 0;
            
            return $"""
            Hybrid Pathfinder Performance:
            Total Pathfinding Operations: {totalUses}
            Central Unit Pathfinder: {_centralPathfinderUses} ({centralPercentage:F1}%)
            Block-Based Pathfinder: {_blockPathfinderUses} ({blockPercentage:F1}%)
            Average Computation Time: {averageTime:F2}ms
            Total Computation Time: {_totalComputationTime}ms
            """;
        }
        
        public void Configure(double blockPathThreshold = 0.7, int maxBlockSearchDistance = 5)
        {
            _blockPathThreshold = Math.Max(0.1, Math.Min(1.0, blockPathThreshold));
            _maxBlockSearchDistance = Math.Max(1, maxBlockSearchDistance);
            
            Console.WriteLine($"[PATHFINDING] HybridPathfinder configured: BlockThreshold={_blockPathThreshold}, MaxBlockDistance={_maxBlockSearchDistance}");
        }
        
        public void ResetStats()
        {
            _centralPathfinderUses = 0;
            _blockPathfinderUses = 0;
            _totalComputationTime = 0;
            Console.WriteLine("[PATHFINDING] Performance statistics reset");
        }
    }
}