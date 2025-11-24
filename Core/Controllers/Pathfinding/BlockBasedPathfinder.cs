using System;
using System.Collections.Generic;
using System.Linq;
    
    namespace WarRegions.Controllers.Pathfinding
    {
            // Core/Controllers/Pathfinding/BlockBasedPathfinder.cs
    // Dependencies:
    // - IPathfinder.cs
    // - Models/Terrain/TerrainTile.cs
    // - Models/Units/MovementType.cs
        public class BlockBasedPathfinder : IPathfinder
        {
            private TerrainManager _terrainManager;
            
            // Block markers represent pre-defined paths or routes
            private Dictionary<(int, int), BlockMarker> _blockMarkers;
            
            public BlockBasedPathfinder(TerrainManager terrainManager)
            {
                _terrainManager = terrainManager;
                _blockMarkers = new Dictionary<(int, int), BlockMarker>();
                InitializeBlockMarkers();
                Console.WriteLine("[PATHFINDING] BlockBasedPathfinder initialized");
            }
            
            private void InitializeBlockMarkers()
            {
                // In a real game, these would be loaded from level data or generated dynamically
                // For now, create some sample block markers
                
                // Create road-like markers along common paths
                for (int x = 0; x < 10; x++)
                {
                    AddBlockMarker(x, 2, BlockType.Road, 1.0);
                    AddBlockMarker(x, 5, BlockType.Road, 1.0);
                }
                
                for (int y = 0; y < 8; y++)
                {
                    AddBlockMarker(3, y, BlockType.Road, 1.0);
                    AddBlockMarker(7, y, BlockType.Road, 1.0);
                }
                
                // Add some natural path markers
                AddBlockMarker(1, 1, BlockType.NaturalPath, 0.7);
                AddBlockMarker(2, 2, BlockType.NaturalPath, 0.7);
                AddBlockMarker(8, 6, BlockType.NaturalPath, 0.7);
                
                Console.WriteLine($"[PATHFINDING] Initialized {_blockMarkers.Count} block markers");
            }
            
            public void AddBlockMarker(int x, int y, BlockType type, double strength)
            {
                var key = (x, y);
                _blockMarkers[key] = new BlockMarker
                {
                    X = x,
                    Y = y,
                    Type = type,
                    Strength = Math.Max(0, Math.Min(1, strength))
                };
            }
            
            public void RemoveBlockMarker(int x, int y)
            {
                var key = (x, y);
                if (_blockMarkers.ContainsKey(key))
                {
                    _blockMarkers.Remove(key);
                }
            }
            
            public List<TerrainTile> FindPath(int startX, int startY, int targetX, int targetY, 
                                            MovementType movementType, int maxCost = 100)
            {
                // Block-based pathfinding follows marked routes
                var path = new List<TerrainTile>();
                
                var startTile = _terrainManager.GetTileAt(startX, startY);
                var targetTile = _terrainManager.GetTileAt(targetX, targetY);
                
                if (startTile == null || targetTile == null)
                {
                    Console.WriteLine($"[PATHFINDING] Invalid start or target position");
                    return path;
                }
                
                // Find nearest block marker to start
                var startMarker = FindNearestBlockMarker(startX, startY, 3); // Search within 3 tiles
                if (startMarker == null)
                {
                    Console.WriteLine($"[PATHFINDING] No block marker near start position");
                    return path;
                }
                
                // Find nearest block marker to target
                var targetMarker = FindNearestBlockMarker(targetX, targetY, 3);
                if (targetMarker == null)
                {
                    Console.WriteLine($"[PATHFINDING] No block marker near target position");
                    return path;
                }
                
                // Find path along block markers from start to target
                var markerPath = FindMarkerPath(startMarker, targetMarker);
                if (markerPath.Count == 0)
                {
                    Console.WriteLine($"[PATHFINDING] No block marker path found");
                    return path;
                }
                
                // Convert marker path to tile path
                path = ConvertMarkerPathToTilePath(markerPath, startTile, targetTile, movementType, maxCost);
                
                if (path.Count > 0)
                {
                    Console.WriteLine($"[PATHFINDING] Block-based path found: {path.Count} steps");
                }
                
                return path;
            }
            
            private BlockMarker FindNearestBlockMarker(int x, int y, int maxDistance)
            {
                BlockMarker nearest = null!;
                double minDistance = double.MaxValue;
                
                foreach (var marker in _blockMarkers.Values)
                {
                    double distance = Math.Abs(marker.X - x) + Math.Abs(marker.Y - y);
                    if (distance <= maxDistance && distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = marker;
                    }
                }
                
                return nearest;
            }
            
            private List<BlockMarker> FindMarkerPath(BlockMarker start, BlockMarker target)
            {
                // Simple marker pathfinding - connect markers in straight lines
                var path = new List<BlockMarker> { start };
                
                int currentX = start.X;
                int currentY = start.Y;
                
                while (currentX != target.X || currentY != target.Y)
                {
                    // Move toward target
                    if (currentX < target.X) currentX++;
                    else if (currentX > target.X) currentX--;
                    
                    if (currentY < target.Y) currentY++;
                    else if (currentY > target.Y) currentY--;
                    
                    // Check if there's a block marker at this position
                    var key = (currentX, currentY);
                    if (_blockMarkers.ContainsKey(key))
                    {
                        path.Add(_blockMarkers[key]);
                    }
                    
                    // Prevent infinite loops
                    if (path.Count > 100) break;
                }
                
                path.Add(target);
                return path;
            }
            
            private List<TerrainTile> ConvertMarkerPathToTilePath(List<BlockMarker> markerPath, 
                                                                TerrainTile startTile, TerrainTile targetTile,
                                                                MovementType movementType, int maxCost)
            {
                var tilePath = new List<TerrainTile> { startTile };
                int totalCost = 0;
                
                for (int i = 0; i < markerPath.Count - 1; i++)
                {
                    var currentMarker = markerPath[i];
                    var nextMarker = markerPath[i + 1];
                    
                    // Get direct path between markers
                    var segmentPath = GetDirectPath(currentMarker.X, currentMarker.Y, 
                                                  nextMarker.X, nextMarker.Y, 
                                                  movementType);
                    
                    foreach (var tile in segmentPath)
                    {
                        if (tilePath.Contains(tile)) continue;
                        
                        // Check movement cost
                        int moveCost = _terrainManager.CalculateMovementCost(
                            movementType, 
                            tilePath.Last().X, tilePath.Last().Y,
                            tile.X, tile.Y
                        );
                        
                        if (moveCost >= 99) // Impassable
                        {
                            Console.WriteLine($"[PATHFINDING] Block path interrupted by impassable terrain");
                            return new List<TerrainTile>();
                        }
                        
                        totalCost += moveCost;
                        if (totalCost > maxCost)
                        {
                            Console.WriteLine($"[PATHFINDING] Block path exceeds maximum cost");
                            return new List<TerrainTile>();
                        }
                        
                        tilePath.Add(tile);
                    }
                }
                
                // Add final path to target
                var finalSegment = GetDirectPath(
                    markerPath.Last().X, markerPath.Last().Y,
                    targetTile.X, targetTile.Y,
                    movementType
                );
                
                tilePath.AddRange(finalSegment);
                
                return tilePath;
            }
            
            private List<TerrainTile> GetDirectPath(int fromX, int fromY, int toX, int toY, MovementType movementType)
            {
                var path = new List<TerrainTile>();
                
                int currentX = fromX;
                int currentY = fromY;
                
                while (currentX != toX || currentY != toY)
                {
                    // Move toward target in straight line
                    if (currentX < toX) currentX++;
                    else if (currentX > toX) currentX--;
                    
                    if (currentY < toY) currentY++;
                    else if (currentY > toY) currentY--;
                    
                    var tile = _terrainManager.GetTileAt(currentX, currentY);
                    if (tile != null)
                    {
                        // Check if tile is passable
                        int moveCost = _terrainManager.CalculateMovementCost(movementType, 
                            path.Count > 0 ? path.Last().X : fromX,
                            path.Count > 0 ? path.Last().Y : fromY,
                            tile.X, tile.Y);
                        
                        if (moveCost < 99)
                        {
                            path.Add(tile);
                        }
                        else
                        {
                            // Impassable tile encountered
                            break;
                        }
                    }
                    else
                    {
                        // Invalid tile encountered
                        break;
                    }
                    
                    // Prevent infinite loops
                    if (path.Count > 50) break;
                }
                
                return path;
            }
            
            public Dictionary<TerrainTile, int> FindReachablePositions(int startX, int startY, 
                                                                     MovementType movementType, int maxCost)
            {
                var reachable = new Dictionary<TerrainTile, int>();
                var startTile = _terrainManager.GetTileAt(startX, startY);
                if (startTile == null) return reachable;
                
                // Find nearby block markers
                var nearbyMarkers = _blockMarkers.Values
                    .Where(m => Math.Abs(m.X - startX) + Math.Abs(m.Y - startY) <= 3)
                    .ToList();
                
                foreach (var marker in nearbyMarkers)
                {
                    var markerTile = _terrainManager.GetTileAt(marker.X, marker.Y);
                    if (markerTile != null)
                    {
                        // Calculate path cost to marker
                        var path = FindPath(startX, startY, marker.X, marker.Y, movementType, maxCost);
                        if (path.Count > 0)
                        {
                            int cost = CalculatePathCost(path, movementType);
                            if (cost <= maxCost)
                            {
                                reachable[markerTile] = cost;
                                
                                // Also add tiles along connected block markers
                                AddConnectedMarkers(marker, reachable, movementType, maxCost, cost);
                            }
                        }
                    }
                }
                
                Console.WriteLine($"[PATHFINDING] Block-based reachability: {reachable.Count} positions");
                return reachable;
            }
            
            private void AddConnectedMarkers(BlockMarker startMarker, Dictionary<TerrainTile, int> reachable,
                                           MovementType movementType, int maxCost, int currentCost)
            {
                // Find markers connected to this one (within 2 tiles)
                var connectedMarkers = _blockMarkers.Values
                    .Where(m => m != startMarker && 
                               Math.Abs(m.X - startMarker.X) + Math.Abs(m.Y - startMarker.Y) <= 2)
                    .ToList();
                
                foreach (var connectedMarker in connectedMarkers)
                {
                    var markerTile = _terrainManager.GetTileAt(connectedMarker.X, connectedMarker.Y);
                    if (markerTile != null && !reachable.ContainsKey(markerTile))
                    {
                        int additionalCost = Math.Abs(connectedMarker.X - startMarker.X) + 
                                           Math.Abs(connectedMarker.Y - startMarker.Y);
                        
                        int totalCost = currentCost + additionalCost;
                        if (totalCost <= maxCost)
                        {
                            reachable[markerTile] = totalCost;
                            AddConnectedMarkers(connectedMarker, reachable, movementType, maxCost, totalCost);
                        }
                    }
                }
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
                return "Block-Based Pathfinder";
            }
            
            public string GetDescription()
            {
                return "Uses pre-defined block markers and routes for efficient pathfinding. Excellent for marked paths but limited in unexplored areas.";
            }
            
            public int GetBlockMarkerCount()
            {
                return _blockMarkers.Count;
            }
            
            public void ClearBlockMarkers()
            {
                _blockMarkers.Clear();
                Console.WriteLine("[PATHFINDING] All block markers cleared");
            }
        }
        
        public class BlockMarker
        {
            public int X { get; set; }
            public int Y { get; set; }
            public BlockType Type { get; set; }
            public double Strength { get; set; } // 0.0 to 1.0
            
            public override string ToString()
            {
                return $"{Type} at ({X},{Y}) - Strength: {Strength:P0}";
            }
        }
        
        public enum BlockType
        {
            Road,
            NaturalPath,
            Trail,
            RiverPath,
            MountainPass
        }
    }
