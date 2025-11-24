using System;
using System.Collections.Generic;
using System.Linq;
    
    namespace WarRegions.Controllers.Pathfinding
    {
            // Core/Controllers/Pathfinding/CentralUnitPathfinder.cs
    // Dependencies:
    // - IPathfinder.cs
    // - Models/Terrain/TerrainTile.cs
    // - Models/Units/MovementType.cs
        public class CentralUnitPathfinder : IPathfinder
        {
            private TerrainManager _terrainManager;
            
            public CentralUnitPathfinder(TerrainManager terrainManager)
            {
                _terrainManager = terrainManager;
                Console.WriteLine("[PATHFINDING] CentralUnitPathfinder initialized");
            }
            
            public List<TerrainTile> FindPath(int startX, int startY, int targetX, int targetY, 
                                            MovementType movementType, int maxCost = 100)
            {
                // Implementation of A* pathfinding algorithm
                var openSet = new List<TerrainTile>();
                var closedSet = new HashSet<TerrainTile>();
                var cameFrom = new Dictionary<TerrainTile, TerrainTile>();
                var gScore = new Dictionary<TerrainTile, int>();
                var fScore = new Dictionary<TerrainTile, int>();
                
                var startTile = _terrainManager.GetTileAt(startX, startY);
                var targetTile = _terrainManager.GetTileAt(targetX, targetY);
                
                if (startTile == null || targetTile == null)
                {
                    Console.WriteLine($"[PATHFINDING] Invalid start or target position");
                    return new List<TerrainTile>();
                }
                
                // Initialize
                openSet.Add(startTile);
                gScore[startTile] = 0;
                fScore[startTile] = CalculateHeuristic(startTile, targetTile);
                
                while (openSet.Count > 0)
                {
                    // Get node with lowest fScore
                    var current = openSet.OrderBy(tile => fScore.ContainsKey(tile) ? fScore[tile] : int.MaxValue).First();
                    
                    // Check if we reached the target
                    if (current == targetTile)
                    {
                        var path = ReconstructPath(cameFrom, current);
                        Console.WriteLine($"[PATHFINDING] Path found: {path.Count} steps, cost: {CalculatePathCost(path, movementType)}");
                        return path;
                    }
                    
                    openSet.Remove(current);
                    closedSet.Add(current);
                    
                    // Check all neighbors
                    foreach (var neighbor in GetNeighbors(current))
                    {
                        if (closedSet.Contains(neighbor))
                            continue;
                        
                        // Check if neighbor is passable
                        int moveCost = _terrainManager.CalculateMovementCost(movementType, current.X, current.Y, neighbor.X, neighbor.Y);
                        if (moveCost >= 99) // Impassable
                            continue;
                        
                        int tentativeGScore = gScore[current] + moveCost;
                        
                        // Check if path exceeds max cost
                        if (tentativeGScore > maxCost)
                            continue;
                        
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                        else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, int.MaxValue))
                            continue;
                        
                        // This path is better than any previous one
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + CalculateHeuristic(neighbor, targetTile);
                    }
                }
                
                // No path found
                Console.WriteLine($"[PATHFINDING] No path found from ({startX},{startY}) to ({targetX},{targetY})");
                return new List<TerrainTile>();
            }
            
            public Dictionary<TerrainTile, int> FindReachablePositions(int startX, int startY, 
                                                                     MovementType movementType, int maxCost)
            {
                var reachable = new Dictionary<TerrainTile, int>();
                var openSet = new Queue<TerrainTile>();
                var visited = new HashSet<TerrainTile>();
                
                var startTile = _terrainManager.GetTileAt(startX, startY);
                if (startTile == null) return reachable;
                
                openSet.Enqueue(startTile);
                visited.Add(startTile);
                reachable[startTile] = 0;
                
                while (openSet.Count > 0)
                {
                    var current = openSet.Dequeue();
                    int currentCost = reachable[current];
                    
                    foreach (var neighbor in GetNeighbors(current))
                    {
                        if (visited.Contains(neighbor))
                            continue;
                        
                        int moveCost = _terrainManager.CalculateMovementCost(movementType, current.X, current.Y, neighbor.X, neighbor.Y);
                        if (moveCost >= 99) // Impassable
                            continue;
                        
                        int totalCost = currentCost + moveCost;
                        if (totalCost <= maxCost)
                        {
                            visited.Add(neighbor);
                            reachable[neighbor] = totalCost;
                            openSet.Enqueue(neighbor);
                        }
                    }
                }
                
                Console.WriteLine($"[PATHFINDING] Found {reachable.Count} reachable positions from ({startX},{startY})");
                return reachable;
            }
            
            public bool HasPath(int startX, int startY, int targetX, int targetY, 
                              MovementType movementType, int maxCost)
            {
                var path = FindPath(startX, startY, targetX, targetY, movementType, maxCost);
                return path.Count > 0 && CalculatePathCost(path, movementType) <= maxCost;
            }
            
            public int GetMovementCost(int fromX, int fromY, int toX, int toY, MovementType movementType)
            {
                return _terrainManager.CalculateMovementCost(movementType, fromX, fromY, toX, toY);
            }
            
            private List<TerrainTile> GetNeighbors(TerrainTile tile)
            {
                var neighbors = new List<TerrainTile>();
                var directions = new (int, int)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
                
                foreach (var (dx, dy) in directions)
                {
                    var neighbor = _terrainManager.GetTileAt(tile.X + dx, tile.Y + dy);
                    if (neighbor != null)
                    {
                        neighbors.Add(neighbor);
                    }
                }
                
                return neighbors;
            }
            
            private int CalculateHeuristic(TerrainTile from, TerrainTile to)
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
                return "Central Unit Pathfinder";
            }
            
            public string GetDescription()
            {
                return "Uses A* algorithm to find optimal paths by examining all possible directions from a central unit. Highly accurate but computationally intensive.";
            }
            
            public List<TerrainTile> FindMultiplePaths(int startX, int startY, List<(int, int)> targets, 
                                                     MovementType movementType, int maxCost = 100)
            {
                var results = new List<TerrainTile>();
                var reachable = FindReachablePositions(startX, startY, movementType, maxCost);
                
                foreach (var (targetX, targetY) in targets)
                {
                    var targetTile = _terrainManager.GetTileAt(targetX, targetY);
                    if (targetTile != null && reachable.ContainsKey(targetTile))
                    {
                        var path = FindPath(startX, startY, targetX, targetY, movementType, maxCost);
                        if (path.Count > 0)
                        {
                            results.AddRange(path);
                        }
                    }
                }
                
                return results.Distinct().ToList();
            }
            
            public TerrainTile FindNearestReachable(int startX, int startY, List<(int, int)> targets, 
                                                  MovementType movementType, int maxCost = 100)
            {
                var reachable = FindReachablePositions(startX, startY, movementType, maxCost);
                TerrainTile nearest = null!;
                int minDistance = int.MaxValue;
                
                foreach (var (targetX, targetY) in targets)
                {
                    var targetTile = _terrainManager.GetTileAt(targetX, targetY);
                    if (targetTile != null && reachable.ContainsKey(targetTile))
                    {
                        int distance = CalculateHeuristic(_terrainManager.GetTileAt(startX, startY), targetTile);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearest = targetTile;
                        }
                    }
                }
                
                return nearest;
            }
        }
    }
