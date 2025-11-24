using System;
using System.Collections.Generic;
using System.Linq;

    namespace WarRegions.Controllers.Pathfinding
    {
            // Core/Controllers/Pathfinding/IPathfinder.cs
    // Dependencies:
    // - Models/Terrain/TerrainTile.cs
    // - Models/Units/MovementType.cs
        public interface IPathfinder
        {
            /// <summary>
            /// Finds a path from start to target position
            /// </summary>
            /// <param name="startX">Starting X coordinate</param>
            /// <param name="startY">Starting Y coordinate</param>
            /// <param name="targetX">Target X coordinate</param>
            /// <param name="targetY">Target Y coordinate</param>
            /// <param name="movementType">Type of movement for terrain costs</param>
            /// <param name="maxCost">Maximum movement cost allowed</param>
            /// <returns>List of tiles representing the path, empty if no path found</returns>
            List<TerrainTile> FindPath(int startX, int startY, int targetX, int targetY, 
                                     MovementType movementType, int maxCost = 100);
            
            /// <summary>
            /// Finds all reachable positions from start within movement cost
            /// </summary>
            /// <param name="startX">Starting X coordinate</param>
            /// <param name="startY">Starting Y coordinate</param>
            /// <param name="movementType">Type of movement for terrain costs</param>
            /// <param name="maxCost">Maximum movement cost allowed</param>
            /// <returns>Dictionary of reachable tiles and their movement costs</returns>
            Dictionary<TerrainTile, int> FindReachablePositions(int startX, int startY, 
                                                              MovementType movementType, int maxCost);
            
            /// <summary>
            /// Checks if a path exists between two points within cost limit
            /// </summary>
            /// <param name="startX">Starting X coordinate</param>
            /// <param name="startY">Starting Y coordinate</param>
            /// <param name="targetX">Target X coordinate</param>
            /// <param name="targetY">Target Y coordinate</param>
            /// <param name="movementType">Type of movement for terrain costs</param>
            /// <param name="maxCost">Maximum movement cost allowed</param>
            /// <returns>True if path exists and is within cost limit</returns>
            bool HasPath(int startX, int startY, int targetX, int targetY, 
                       MovementType movementType, int maxCost);
            
            /// <summary>
            /// Calculates the movement cost between two adjacent tiles
            /// </summary>
            /// <param name="fromX">From X coordinate</param>
            /// <param name="fromY">From Y coordinate</param>
            /// <param name="toX">To X coordinate</param>
            /// <param name="toY">To Y coordinate</param>
            /// <param name="movementType">Type of movement for terrain costs</param>
            /// <returns>Movement cost, 99 if impassable</returns>
            int GetMovementCost(int fromX, int fromY, int toX, int toY, MovementType movementType);
            
            /// <summary>
            /// Gets the name of the pathfinding algorithm
            /// </summary>
            string GetName();
            
            /// <summary>
            /// Gets a description of the pathfinding algorithm
            /// </summary>
            string GetDescription();
        }
        
        public class PathfindingResult
        {
            public List<TerrainTile> Path { get; set; }
            public int TotalCost { get; set; }
            public bool PathFound { get; set; }
            public string AlgorithmUsed { get; set; }
            public long ComputationTimeMs { get; set; }
            
            public PathfindingResult()
            {
                Path = new List<TerrainTile>();
                PathFound = false;
            }
            
            public override string ToString()
            {
                if (!PathFound)
                    return $"No path found (Algorithm: {AlgorithmUsed})";
                    
                return $"Path found: {Path.Count} steps, {TotalCost} cost (Algorithm: {AlgorithmUsed}, Time: {ComputationTimeMs}ms)";
            }
        }
    }
