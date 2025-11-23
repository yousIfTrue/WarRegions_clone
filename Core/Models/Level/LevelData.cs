using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Models.Level
{
    // Core/Models/Level/LevelData.cs
    // Dependencies:
    // - SpawnPoint.cs (for PlayerSpawnPoints and EnemySpawnPoints)
    // - Terrain/TerrainType.cs (for map generation)
    
    using System;
    using System.Collections.Generic;
    
    namespace WarRegionsClone.Models.Level
    {
        public class LevelData
        {
            public string LevelId { get; set; }
            public string LevelName { get; set; }
            public string Description { get; set; }
            
            // Map configuration
            public int MapWidth { get; set; } = 8;
            public int MapHeight { get; set; } = 6;
            public string MapLayout { get; set; } // Could be a string representation or file reference
            
            // AI configuration
            public string AIDifficulty { get; set; } = "medium";
            public string AIBehavior { get; set; } = "balanced";
            public int AIStartingSilver { get; set; } = 800;
            public int AIStartingGold { get; set; } = 80;
            
            // Spawn points
            public List<SpawnPoint> PlayerSpawnPoints { get; set; }
            public List<SpawnPoint> EnemySpawnPoints { get; set; }
            
            // Victory conditions
            public VictoryCondition VictoryCondition { get; set; }
            public int TurnsLimit { get; set; } = 30;
            
            // Rewards
            public int SilverReward { get; set; } = 200;
            public int GoldReward { get; set; } = 20;
            public List<string> UnitRewards { get; set; }
            
            // Level requirements
            public int RequiredLevel { get; set; } = 1;
            public List<string> RequiredUnits { get; set; }
            
            // Development flags
            public bool IsUnlocked { get; set; } = true;
            public bool IsCompleted { get; set; } = false;
            public int CompletionStars { get; set; } = 0;
            
            public LevelData()
            {
                LevelId = Guid.NewGuid().ToString();
                PlayerSpawnPoints = new List<SpawnPoint>();
                EnemySpawnPoints = new List<SpawnPoint>();
                UnitRewards = new List<string>();
                RequiredUnits = new List<string>();
                VictoryCondition = new VictoryCondition();
            }
            
            public LevelData(string name, string description, int width, int height) : this()
            {
                LevelName = name;
                Description = description;
                MapWidth = width;
                MapHeight = height;
                InitializeDefaultSpawnPoints();
            }
            
            private void InitializeDefaultSpawnPoints()
            {
                // Default spawn points - player bottom-left, enemy top-right
                PlayerSpawnPoints.Add(new SpawnPoint(0, 0, "infantry"));
                PlayerSpawnPoints.Add(new SpawnPoint(1, 0, "archer"));
                
                EnemySpawnPoints.Add(new SpawnPoint(MapWidth - 1, MapHeight - 1, "infantry"));
                EnemySpawnPoints.Add(new SpawnPoint(MapWidth - 2, MapHeight - 1, "cavalry"));
            }
            
            public void AddPlayerSpawnPoint(int x, int y, string unitType)
            {
                PlayerSpawnPoints.Add(new SpawnPoint(x, y, unitType));
            }
            
            public void AddEnemySpawnPoint(int x, int y, string unitType)
            {
                EnemySpawnPoints.Add(new SpawnPoint(x, y, unitType));
            }
            
            public bool MeetsRequirements(Player player)
            {
                // Check level requirement
                if (player.LevelProgress < RequiredLevel)
                    return false;
                
                // Check unit requirements
                foreach (var requiredUnit in RequiredUnits)
                {
                    bool hasUnit = player.AvailableUnits.Exists(u => u.UnitName.ToLower().Contains(requiredUnit.ToLower()));
                    if (!hasUnit)
                        return false;
                }
                
                return true;
            }
            
            public void CalculateRewards(Player player, int turnsUsed, bool perfectVictory = false)
            {
                // Base rewards
                int silver = SilverReward;
                int gold = GoldReward;
                
                // Turn bonus - faster completion = better reward
                double turnBonus = Math.Max(0.5, 1.0 - ((double)turnsUsed / TurnsLimit));
                silver = (int)(silver * turnBonus);
                gold = (int)(gold * turnBonus);
                
                // Perfect victory bonus
                if (perfectVictory)
                {
                    silver = (int)(silver * 1.5);
                    gold = (int)(gold * 1.5);
                    CompletionStars = 3;
                }
                else if (turnsUsed <= TurnsLimit * 0.7)
                {
                    CompletionStars = 2;
                }
                else
                {
                    CompletionStars = 1;
                }
                
                // Apply rewards
                player.SilverCoins += silver;
                player.GoldCoins += gold;
                
                Console.WriteLine($"Level completed! Rewards: {silver} silver, {gold} gold, {CompletionStars} stars");
                
                // Mark as completed
                IsCompleted = true;
                player.LevelProgress = Math.Max(player.LevelProgress, int.Parse(LevelId.Split('_').Last()) + 1);
            }
            
            public string GetAIDescription()
            {
                string difficultyText = AIDifficulty switch
                {
                    "easy" => "relaxed and predictable",
                    "medium" => "strategic and adaptive", 
                    "hard" => "aggressive and cunning",
                    "expert" => "ruthless and optimized",
                    _ => "balanced"
                };
                
                string behaviorText = AIBehavior switch
                {
                    "defensive" => "prefers defensive positions",
                    "aggressive" => "constantly seeks engagement",
                    "balanced" => "adapts to the situation",
                    "raider" => "targets weak points",
                    _ => "varies tactics"
                };
                
                return $"AI is {difficultyText} and {behaviorText}.";
            }
            
            public override string ToString()
            {
                return $"{LevelName} ({MapWidth}x{MapHeight}) - {AIDifficulty} AI - {VictoryCondition.Type}";
            }
            
            public string ToDetailedString()
            {
                return $"""
                Level: {LevelName}
                Size: {MapWidth}x{MapHeight}
                AI: {AIDifficulty} ({AIBehavior})
                Victory: {VictoryCondition.Type}
                Turns: {TurnsLimit}
                Reward: {SilverReward} silver, {GoldReward} gold
                {GetAIDescription()}
                """;
            }
        }
        
        public class VictoryCondition
        {
            public string Type { get; set; } = "eliminate_all";
            public string Target { get; set; } = "enemy_units";
            public int RequiredCount { get; set; } = 0;
            public int TimeLimit { get; set; } = 0;
            
            public VictoryCondition() { }
            
            public VictoryCondition(string type, string target = "enemy_units", int requiredCount = 0)
            {
                Type = type;
                Target = target;
                RequiredCount = requiredCount;
            }
            
            public bool IsConditionMet(GameState gameState)
            {
                switch (Type)
                {
                    case "eliminate_all":
                        return CheckEliminateAll(gameState);
                        
                    case "capture_regions":
                        return CheckCaptureRegions(gameState);
                        
                    case "survive_turns":
                        return CheckSurviveTurns(gameState);
                        
                    case "destroy_specific":
                        return CheckDestroySpecific(gameState);
                        
                    default:
                        return false;
                }
            }
            
            private bool CheckEliminateAll(GameState gameState)
            {
                var enemyArmies = gameState.Armies.Where(a => a.Owner != gameState.CurrentPlayer);
                return !enemyArmies.Any(a => a.GetAliveUnitCount() > 0);
            }
            
            private bool CheckCaptureRegions(GameState gameState)
            {
                var playerRegions = gameState.Regions.Count(r => r.Owner == gameState.CurrentPlayer);
                return playerRegions >= RequiredCount;
            }
            
            private bool CheckSurviveTurns(GameState gameState)
            {
                return gameState.TurnNumber >= TimeLimit;
            }
            
            private bool CheckDestroySpecific(GameState gameState)
            {
                // This would check for specific unit or structure destruction
                // For now, return false as implementation depends on specific targets
                return false;
            }
            
            public string GetDescription()
            {
                return Type switch
                {
                    "eliminate_all" => "Destroy all enemy units",
                    "capture_regions" => $"Capture at least {RequiredCount} regions",
                    "survive_turns" => $"Survive for {TimeLimit} turns",
                    "destroy_specific" => "Destroy the enemy commander",
                    _ => "Unknown victory condition"
                };
            }
        }
    }}
