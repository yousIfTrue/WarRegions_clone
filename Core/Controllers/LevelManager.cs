using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Controllers
{
    // Core/Controllers/LevelManager.cs
    // Dependencies:
    // - Models/Level/LevelData.cs
    // - Models/Level/SpawnPoint.cs
    // - Models/GameState.cs
    // - Models/Player.cs
    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    
    namespace WarRegionsClone.Controllers
    {
        public class LevelManager
        {
            private List<LevelData> _loadedLevels;
            private string _levelsDirectory;
            
            public LevelManager()
            {
                _loadedLevels = new List<LevelData>();
                _levelsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Levels");
                Console.WriteLine("[LEVEL] LevelManager initialized");
            }
            
            public LevelManager(string customLevelsDirectory) : this()
            {
                _levelsDirectory = customLevelsDirectory;
            }
            
            public LevelData LoadLevel(string levelId)
            {
                // Check if level is already loaded
                var existingLevel = _loadedLevels.FirstOrDefault(l => l.LevelId == levelId);
                if (existingLevel != null)
                {
                    Console.WriteLine($"[LEVEL] Level {levelId} already loaded from cache");
                    return existingLevel;
                }
                
                try
                {
                    string filePath = Path.Combine(_levelsDirectory, $"{levelId}.json");
                    
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"[LEVEL] Level file not found: {filePath}");
                        return CreateDefaultLevel(levelId);
                    }
                    
                    string jsonContent = File.ReadAllText(filePath);
                    var levelData = JsonSerializer.Deserialize<LevelData>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (levelData != null)
                    {
                        _loadedLevels.Add(levelData);
                        Console.WriteLine($"[LEVEL] Successfully loaded level: {levelData.LevelName}");
                        return levelData;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LEVEL ERROR] Failed to load level {levelId}: {ex.Message}");
                }
                
                // Fallback to default level
                return CreateDefaultLevel(levelId);
            }
            
            private LevelData CreateDefaultLevel(string levelId)
            {
                Console.WriteLine($"[LEVEL] Creating default level for: {levelId}");
                
                var defaultLevel = new LevelData(
                    $"Default {levelId}",
                    "A automatically generated level for testing",
                    8, 6
                );
                
                defaultLevel.LevelId = levelId;
                defaultLevel.AIDifficulty = "medium";
                defaultLevel.AIBehavior = "balanced";
                defaultLevel.SilverReward = 200;
                defaultLevel.GoldReward = 20;
                defaultLevel.TurnsLimit = 30;
                
                // Add some additional spawn points
                defaultLevel.AddPlayerSpawnPoint(1, 1, "archer");
                defaultLevel.AddEnemySpawnPoint(6, 4, "cavalry");
                
                _loadedLevels.Add(defaultLevel);
                
                return defaultLevel;
            }
            
            public List<LevelData> LoadAllLevels()
            {
                if (!Directory.Exists(_levelsDirectory))
                {
                    Console.WriteLine($"[LEVEL] Levels directory not found: {_levelsDirectory}");
                    return new List<LevelData>();
                }
                
                var levelFiles = Directory.GetFiles(_levelsDirectory, "*.json");
                var levels = new List<LevelData>();
                
                foreach (var file in levelFiles)
                {
                    try
                    {
                        string levelId = Path.GetFileNameWithoutExtension(file);
                        var level = LoadLevel(levelId);
                        if (level != null)
                        {
                            levels.Add(level);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LEVEL ERROR] Failed to load level from {file}: {ex.Message}");
                    }
                }
                
                Console.WriteLine($"[LEVEL] Loaded {levels.Count} levels from {_levelsDirectory}");
                return levels;
            }
            
            public void SaveLevel(LevelData levelData)
            {
                try
                {
                    if (!Directory.Exists(_levelsDirectory))
                    {
                        Directory.CreateDirectory(_levelsDirectory);
                    }
                    
                    string filePath = Path.Combine(_levelsDirectory, $"{levelData.LevelId}.json");
                    
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    
                    string jsonContent = JsonSerializer.Serialize(levelData, options);
                    File.WriteAllText(filePath, jsonContent);
                    
                    Console.WriteLine($"[LEVEL] Successfully saved level: {levelData.LevelName} to {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LEVEL ERROR] Failed to save level {levelData.LevelName}: {ex.Message}");
                }
            }
            
            public LevelData CreateNewLevel(string levelName, string description, int width, int height)
            {
                var newLevel = new LevelData(levelName, description, width, height)
                {
                    LevelId = $"level_{DateTime.Now:yyyyMMddHHmmss}",
                    AIDifficulty = "medium",
                    AIBehavior = "balanced",
                    SilverReward = 150,
                    GoldReward = 15,
                    TurnsLimit = 25
                };
                
                _loadedLevels.Add(newLevel);
                Console.WriteLine($"[LEVEL] Created new level: {levelName} ({width}x{height})");
                
                return newLevel;
            }
            
            public void UnlockLevel(string levelId, Player player)
            {
                var level = _loadedLevels.FirstOrDefault(l => l.LevelId == levelId);
                if (level != null)
                {
                    level.IsUnlocked = true;
                    Console.WriteLine($"[LEVEL] Level {level.LevelName} unlocked for {player.PlayerName}");
                }
            }
            
            public void CompleteLevel(string levelId, Player player, int turnsUsed, bool perfectVictory = false)
            {
                var level = _loadedLevels.FirstOrDefault(l => l.LevelId == levelId);
                if (level != null)
                {
                    level.CalculateRewards(player, turnsUsed, perfectVictory);
                    level.IsCompleted = true;
                    
                    // Unlock next level if exists
                    UnlockNextLevel(levelId, player);
                    
                    Console.WriteLine($"[LEVEL] Level {level.LevelName} completed by {player.PlayerName}");
                }
            }
            
            private void UnlockNextLevel(string currentLevelId, Player player)
            {
                // Simple progression: level_01 -> level_02 -> level_03, etc.
                if (currentLevelId.StartsWith("level_"))
                {
                    try
                    {
                        string levelNumberStr = currentLevelId.Substring(6);
                        if (int.TryParse(levelNumberStr, out int levelNumber))
                        {
                            string nextLevelId = $"level_{levelNumber + 1:00}";
                            var nextLevel = _loadedLevels.FirstOrDefault(l => l.LevelId == nextLevelId);
                            if (nextLevel != null)
                            {
                                nextLevel.IsUnlocked = true;
                                Console.WriteLine($"[LEVEL] Next level {nextLevel.LevelName} unlocked!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[LEVEL] Error unlocking next level: {ex.Message}");
                    }
                }
            }
            
            public List<LevelData> GetAvailableLevels(Player player)
            {
                return _loadedLevels
                    .Where(level => level.IsUnlocked || level.MeetsRequirements(player))
                    .OrderBy(level => level.LevelId)
                    .ToList();
            }
            
            public List<LevelData> GetCompletedLevels(Player player)
            {
                return _loadedLevels
                    .Where(level => level.IsCompleted)
                    .OrderBy(level => level.LevelId)
                    .ToList();
            }
            
            public LevelData GetLevelProgression(Player player)
            {
                // Get the highest level the player should attempt next
                var availableLevels = GetAvailableLevels(player);
                var incompleteLevels = availableLevels.Where(level => !level.IsCompleted).ToList();
                
                if (incompleteLevels.Any())
                {
                    return incompleteLevels.First();
                }
                
                // If all available levels are completed, return the highest level
                return availableLevels.LastOrDefault();
            }
            
            public void ProcessReinforcements(GameState gameState, int currentTurn)
            {
                var level = gameState.CurrentLevel;
                if (level == null) return;
                
                // Process player reinforcements
                foreach (var spawnPoint in level.PlayerSpawnPoints.Where(sp => sp.IsReinforcement))
                {
                    if (spawnPoint.ShouldSpawn(currentTurn, gameState))
                    {
                        SpawnReinforcement(spawnPoint, gameState.Players[0], gameState);
                    }
                }
                
                // Process AI reinforcements
                foreach (var spawnPoint in level.EnemySpawnPoints.Where(sp => sp.IsReinforcement))
                {
                    if (spawnPoint.ShouldSpawn(currentTurn, gameState))
                    {
                        var aiPlayer = gameState.Players.FirstOrDefault(p => p.PlayerId != gameState.CurrentPlayer.PlayerId);
                        if (aiPlayer != null)
                        {
                            SpawnReinforcement(spawnPoint, aiPlayer, gameState);
                        }
                    }
                }
            }
            
            private void SpawnReinforcement(SpawnPoint spawnPoint, Player owner, GameState gameState)
            {
                var region = gameState.GetRegionAt(spawnPoint.X, spawnPoint.Y);
                if (region == null)
                {
                    Console.WriteLine($"[LEVEL] Cannot spawn reinforcement at invalid region ({spawnPoint.X}, {spawnPoint.Y})");
                    return;
                }
                
                // Find or create army for this player in the region
                var army = gameState.Armies.FirstOrDefault(a => a.Owner == owner && a.CurrentRegion == region);
                if (army == null)
                {
                    army = new Army(owner, $"{owner.PlayerName} Reinforcements");
                    army.CurrentRegion = region;
                    region.OccupyingArmy = army;
                    gameState.Armies.Add(army);
                }
                
                // Create and add the unit
                var unit = spawnPoint.CreateUnit(owner);
                army.AddUnit(unit);
                
                Console.WriteLine($"[LEVEL] Reinforcement spawned: {unit.UnitName} for {owner.PlayerName} at {region.RegionName}");
            }
            
            public bool CheckLevelVictory(GameState gameState)
            {
                var level = gameState.CurrentLevel;
                if (level == null) return false;
                
                return level.VictoryCondition.IsConditionMet(gameState);
            }
            
            public bool CheckLevelDefeat(GameState gameState)
            {
                var humanPlayer = gameState.Players[0];
                var humanArmies = gameState.Armies.Where(a => a.Owner == humanPlayer);
                
                // Defeat if no alive human units
                if (!humanArmies.Any(a => a.GetAliveUnitCount() > 0))
                {
                    return true;
                }
                
                // Defeat if turn limit exceeded
                if (gameState.TurnNumber > gameState.CurrentLevel?.TurnsLimit)
                {
                    return true;
                }
                
                return false;
            }
            
            public string GetLevelSummary(LevelData level)
            {
                return $"""
                {level.LevelName}
                {level.Description}
                
                Map Size: {level.MapWidth}x{level.MapHeight}
                AI: {level.AIDifficulty} ({level.AIBehavior})
                Victory: {level.VictoryCondition.GetDescription()}
                Turns: {level.TurnsLimit}
                Reward: {level.SilverReward} silver, {level.GoldReward} gold
                
                Player Start: {level.PlayerSpawnPoints.Count} units
                Enemy Start: {level.EnemySpawnPoints.Count} units
                Reinforcements: {level.PlayerSpawnPoints.Count(sp => sp.IsReinforcement) + level.EnemySpawnPoints.Count(sp => sp.IsReinforcement)}
                """;
            }
            
            public void ClearCache()
            {
                _loadedLevels.Clear();
                Console.WriteLine("[LEVEL] Level cache cleared");
            }
        }
    }}
