using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Models
{
    // GameState.cs
    // Dependencies:
    // - Player.cs (for Players list)
    // - Region.cs (for Regions list)
    // - Army.cs (for Armies list)
    // - Level/LevelData.cs (for CurrentLevel)
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace WarRegions.Models
    {
        public class GameState
        {
            public string GameId { get; set; }
            public List<Player> Players { get; set; }
            public List<Region> Regions { get; set; }
            public List<Army> Armies { get; set; }
            public Player CurrentPlayer { get; set; }
            public LevelData CurrentLevel { get; set; }
            
            // Game progress
            public int TurnNumber { get; set; } = 1;
            public GamePhase CurrentPhase { get; set; } = GamePhase.PlayerTurn;
            public bool IsGameOver { get; set; }
            public Player Winner { get; set; }
            
            // Game settings
            public int MaxTurns { get; set; } = 50;
            public bool DebugMode { get; set; } = true;
            
            public GameState()
            {
                GameId = Guid.NewGuid().ToString();
                Players = new List<Player>();
                Regions = new List<Region>();
                Armies = new List<Army>();
            }
            
            public void InitializeNewGame(Player humanPlayer, LevelData level)
            {
                Players.Clear();
                Regions.Clear();
                Armies.Clear();
                
                // Add human player
                Players.Add(humanPlayer);
                CurrentPlayer = humanPlayer;
                
                // Add AI players based on level
                InitializeAIPlayers(level.AIDifficulty, level.AIBehavior);
                
                // Set current level
                CurrentLevel = level;
                
                // Generate map regions
                GenerateMapRegions(level.MapWidth, level.MapHeight);
                
                // Place starting armies
                PlaceStartingArmies(level);
                
                Console.WriteLine($"New game started - Level: {level.LevelName}, Turn: {TurnNumber}");
            }
            
            private void InitializeAIPlayers(string difficulty, string behavior)
            {
                // Create AI players based on difficulty
                var aiPlayer = new Player
                {
                    PlayerId = "AI_1",
                    PlayerName = $"AI_{difficulty}",
                    SilverCoins = 800, // AI gets slightly less resources
                    GoldCoins = 80
                };
                
                Players.Add(aiPlayer);
                Console.WriteLine($"AI Player created: {aiPlayer.PlayerName}");
            }
            
            private void GenerateMapRegions(int width, int height)
            {
                // Simple grid-based map generation
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var terrain = GetRandomTerrainType(x, y);
                        var region = new Region($"Region_{x}_{y}", x, y, terrain);
                        
                        // Set ownership - center regions to player, edges to AI
                        if (x == 0 && y == 0)
                            region.Owner = Players[0]; // Human player
                        else if (x == width - 1 && y == height - 1)
                            region.Owner = Players[1]; // AI player
                        
                        Regions.Add(region);
                    }
                }
                
                // Connect adjacent regions
                ConnectAdjacentRegions();
                
                Console.WriteLine($"Generated map with {Regions.Count} regions ({width}x{height})");
            }
            
            private TerrainType GetRandomTerrainType(int x, int y)
            {
                // Simple terrain distribution for testing
                var random = new Random(x * 1000 + y);
                var terrains = new[] {
                    TerrainType.Plains, TerrainType.Plains, TerrainType.Plains,
                    TerrainType.Forest, TerrainType.Forest,
                    TerrainType.Mountains, TerrainType.River
                };
                
                return terrains[random.Next(terrains.Length)];
            }
            
            private void ConnectAdjacentRegions()
            {
                foreach (var region in Regions)
                {
                    var adjacent = Regions.Where(r => 
                        (Math.Abs(r.X - region.X) == 1 && r.Y == region.Y) ||
                        (Math.Abs(r.Y - region.Y) == 1 && r.X == region.X));
                    
                    foreach (var neighbor in adjacent)
                    {
                        region.ConnectTo(neighbor);
                    }
                }
            }
            
            private void PlaceStartingArmies(LevelData level)
            {
                // Place human player army
                var humanStartRegion = Regions.First(r => r.X == 0 && r.Y == 0);
                var humanArmy = new Army(Players[0], "Human Army");
                humanArmy.CurrentRegion = humanStartRegion;
                humanStartRegion.OccupyingArmy = humanArmy;
                Armies.Add(humanArmy);
                
                // Place AI army
                var aiStartRegion = Regions.First(r => r.X == level.MapWidth - 1 && r.Y == level.MapHeight - 1);
                var aiArmy = new Army(Players[1], "AI Army");
                aiArmy.CurrentRegion = aiStartRegion;
                aiStartRegion.OccupyingArmy = aiArmy;
                Armies.Add(aiArmy);
                
                Console.WriteLine("Starting armies placed on map");
            }
            
            public void EndTurn()
            {
                // Reset movement points for all armies of current player
                var playerArmies = Armies.Where(a => a.Owner == CurrentPlayer);
                foreach (var army in playerArmies)
                {
                    army.ResetMovementPoints();
                }
                
                // Switch to next player
                var currentIndex = Players.IndexOf(CurrentPlayer);
                var nextIndex = (currentIndex + 1) % Players.Count;
                CurrentPlayer = Players[nextIndex];
                
                TurnNumber++;
                
                Console.WriteLine($"Turn {TurnNumber} - Current Player: {CurrentPlayer.PlayerName}");
                
                // Check win conditions
                CheckWinConditions();
            }
            
            private void CheckWinConditions()
            {
                // Simple win condition: eliminate all enemy units
                var humanArmies = Armies.Where(a => a.Owner == Players[0]);
                var aiArmies = Armies.Where(a => a.Owner == Players[1]);
                
                if (!humanArmies.Any(a => a.GetAliveUnitCount() > 0))
                {
                    IsGameOver = true;
                    Winner = Players[1]; // AI wins
                    Console.WriteLine("Game Over - AI Wins!");
                }
                else if (!aiArmies.Any(a => a.GetAliveUnitCount() > 0))
                {
                    IsGameOver = true;
                    Winner = Players[0]; // Human wins
                    Console.WriteLine("Game Over - Human Wins!");
                }
                else if (TurnNumber >= MaxTurns)
                {
                    IsGameOver = true;
                    // Determine winner by region control
                    var humanRegions = Regions.Count(r => r.Owner == Players[0]);
                    var aiRegions = Regions.Count(r => r.Owner == Players[1]);
                    
                    Winner = humanRegions > aiRegions ? Players[0] : Players[1];
                    Console.WriteLine($"Game Over - Time's up! Winner: {Winner.PlayerName}");
                }
            }
            
            public Region GetRegionAt(int x, int y)
            {
                return Regions.FirstOrDefault(r => r.X == x && r.Y == y);
            }
            
            public List<Army> GetPlayerArmies(Player player)
            {
                return Armies.Where(a => a.Owner == player).ToList();
            }
            
            public override string ToString()
            {
                return $"Game State - Turn: {TurnNumber}, Phase: {CurrentPhase}, Players: {Players.Count}, Regions: {Regions.Count}";
            }
        }
        
        public enum GamePhase
        {
            PlayerTurn,
            AITurn,
            Battle,
            GameOver
        }
    }}
