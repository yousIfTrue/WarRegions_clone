// Core/Controllers/AIController.cs
// Dependencies:
// - Models/GameState.cs
// - Models/Player.cs
// - Models/Army.cs
// - Models/Region.cs
// - Controllers/BattleCalculator.cs
// - Controllers/TerrainManager.cs

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegionsClone.Controllers
{
    public class AIController
    {
        private GameState _gameState;
        private BattleCalculator _battleCalculator;
        private TerrainManager _terrainManager;
        private Random _random;
        
        // AI personality settings
        private string _difficulty;
        private string _behavior;
        private double _aggressionLevel;
        private double _cautionLevel;
        private double _expansionism;
        
        public AIController()
        {
            _battleCalculator = new BattleCalculator();
            _terrainManager = new TerrainManager();
            _random = new Random();
            Console.WriteLine("[AI] AIController initialized");
        }
        
        public void Initialize(GameState gameState)
        {
            _gameState = gameState;
            
            // Get AI player (first non-human player)
            var aiPlayer = gameState.Players.FirstOrDefault(p => p.PlayerId != gameState.CurrentPlayer.PlayerId);
            if (aiPlayer != null)
            {
                // Set AI personality based on level settings
                var level = gameState.CurrentLevel;
                _difficulty = level?.AIDifficulty ?? "medium";
                _behavior = level?.AIBehavior ?? "balanced";
                
                SetAIPersonality(_difficulty, _behavior);
                
                Console.WriteLine($"[AI] AI initialized - Difficulty: {_difficulty}, Behavior: {_behavior}");
            }
        }
        
        private void SetAIPersonality(string difficulty, string behavior)
        {
            // Set base values based on difficulty
            (_aggressionLevel, _cautionLevel, _expansionism) = difficulty.ToLower() switch
            {
                "easy" => (0.3, 0.7, 0.4),
                "medium" => (0.5, 0.5, 0.6),
                "hard" => (0.7, 0.3, 0.8),
                "expert" => (0.9, 0.2, 0.9),
                _ => (0.5, 0.5, 0.6)
            };
            
            // Adjust based on behavior
            switch (behavior.ToLower())
            {
                case "aggressive":
                    _aggressionLevel += 0.2;
                    _cautionLevel -= 0.2;
                    break;
                case "defensive":
                    _aggressionLevel -= 0.2;
                    _cautionLevel += 0.2;
                    break;
                case "raider":
                    _expansionism += 0.2;
                    break;
                case "balanced":
                    // Default values are fine
                    break;
            }
            
            // Clamp values between 0 and 1
            _aggressionLevel = Math.Max(0, Math.Min(1, _aggressionLevel));
            _cautionLevel = Math.Max(0, Math.Min(1, _cautionLevel));
            _expansionism = Math.Max(0, Math.Min(1, _expansionism));
        }
        
        public void ProcessTurn(GameState gameState)
        {
            if (gameState == null) return;
            
            _gameState = gameState;
            var aiPlayer = GetAIPlayer();
            
            if (aiPlayer == null)
            {
                Console.WriteLine("[AI] No AI player found");
                return;
            }
            
            Console.WriteLine($"[AI] Processing turn for {aiPlayer.PlayerName}");
            
            // AI decision making process
            var aiArmies = gameState.GetPlayerArmies(aiPlayer);
            
            foreach (var army in aiArmies.Where(a => a.GetAliveUnitCount() > 0))
            {
                ProcessArmyTurn(army);
            }
            
            // Strategic decisions
            MakeStrategicDecisions(aiPlayer);
            
            Console.WriteLine($"[AI] Turn processing completed for {aiPlayer.PlayerName}");
        }
        
        private void ProcessArmyTurn(Army army)
        {
            if (army.MovementPoints <= 0) return;
            
            Console.WriteLine($"[AI] Processing army: {army.ArmyName}");
            
            // Decision priorities
            var decisions = new List<AIDecision>();
            
            // 1. Check for attack opportunities
            decisions.AddRange(EvaluateAttackOpportunities(army));
            
            // 2. Check for region capture opportunities
            decisions.AddRange(EvaluateCaptureOpportunities(army));
            
            // 3. Consider reinforcement or regrouping
            decisions.AddRange(EvaluateMovementOptions(army));
            
            // Sort decisions by priority and execute the best one
            var bestDecision = decisions
                .OrderByDescending(d => d.Priority)
                .FirstOrDefault();
            
            if (bestDecision != null)
            {
                ExecuteDecision(army, bestDecision);
            }
            else
            {
                // Default behavior: move toward nearest enemy region
                ExecuteDefaultMovement(army);
            }
        }
        
        private List<AIDecision> EvaluateAttackOpportunities(Army army)
        {
            var decisions = new List<AIDecision>();
            var adjacentRegions = GetAdjacentRegions(army.CurrentRegion);
            
            foreach (var region in adjacentRegions)
            {
                if (region.OccupyingArmy != null && region.OccupyingArmy.Owner != army.Owner)
                {
                    // Enemy army detected - evaluate battle
                    var battleSimulation = _battleCalculator.SimulateBattle(army, region.OccupyingArmy, region, 100);
                    double winProbability = battleSimulation.AttackerWon ? 0.7 : 0.3; // Simplified for now
                    
                    if (winProbability > (0.5 - _cautionLevel + _aggressionLevel))
                    {
                        decisions.Add(new AIDecision
                        {
                            Type = AIDecisionType.Attack,
                            TargetRegion = region,
                            Priority = winProbability * 100 + _aggressionLevel * 50,
                            Description = $"Attack enemy at {region.RegionName} (Win chance: {winProbability:P0})"
                        });
                    }
                }
            }
            
            return decisions;
        }
        
        private List<AIDecision> EvaluateCaptureOpportunities(Army army)
        {
            var decisions = new List<AIDecision>();
            var adjacentRegions = GetAdjacentRegions(army.CurrentRegion);
            
            foreach (var region in adjacentRegions)
            {
                if (region.Owner != army.Owner && region.OccupyingArmy == null)
                {
                    // Undefended enemy region - consider capture
                    double priority = _expansionism * 80;
                    
                    // Increase priority for regions with high resource value
                    priority += (region.SilverProduction + region.GoldProduction * 10) * 2;
                    
                    decisions.Add(new AIDecision
                    {
                        Type = AIDecisionType.Capture,
                        TargetRegion = region,
                        Priority = priority,
                        Description = $"Capture undefended region {region.RegionName}"
                    });
                }
            }
            
            return decisions;
        }
        
        private List<AIDecision> EvaluateMovementOptions(Army army)
        {
            var decisions = new List<AIDecision>();
            
            // Consider regrouping with other armies
            var friendlyArmies = _gameState.Armies
                .Where(a => a.Owner == army.Owner && a != army && a.GetAliveUnitCount() > 0)
                .ToList();
            
            foreach (var friendlyArmy in friendlyArmies)
            {
                var path = FindPathToRegion(army, friendlyArmy.CurrentRegion);
                if (path.Count > 0 && path.Count <= 3) // Only consider if reasonably close
                {
                    decisions.Add(new AIDecision
                    {
                        Type = AIDecisionType.Regroup,
                        TargetRegion = friendlyArmy.CurrentRegion,
                        Priority = 40 * _cautionLevel,
                        Description = $"Regroup with {friendlyArmy.ArmyName}"
                    });
                }
            }
            
            // Consider strategic positioning
            var enemyRegions = _gameState.Regions
                .Where(r => r.Owner != army.Owner && r.Owner != null)
                .OrderBy(r => CalculateDistance(army.CurrentRegion, r))
                .Take(3);
            
            foreach (var enemyRegion in enemyRegions)
            {
                var path = FindPathToRegion(army, enemyRegion);
                if (path.Count > 1) // Don't consider adjacent regions (those are for attack)
                {
                    var moveRegion = path[1]; // First step in path
                    
                    decisions.Add(new AIDecision
                    {
                        Type = AIDecisionType.Position,
                        TargetRegion = moveRegion,
                        Priority = 30 * _aggressionLevel,
                        Description = $"Move toward enemy region {enemyRegion.RegionName}"
                    });
                }
            }
            
            return decisions;
        }
        
        private void ExecuteDecision(Army army, AIDecision decision)
        {
            Console.WriteLine($"[AI] Executing: {decision.Description}");
            
            switch (decision.Type)
            {
                case AIDecisionType.Attack:
                    if (army.CanMoveTo(decision.TargetRegion))
                    {
                        army.MoveTo(decision.TargetRegion);
                    }
                    break;
                    
                case AIDecisionType.Capture:
                    if (army.CanMoveTo(decision.TargetRegion))
                    {
                        army.MoveTo(decision.TargetRegion);
                    }
                    break;
                    
                case AIDecisionType.Regroup:
                case AIDecisionType.Position:
                    if (army.CanMoveTo(decision.TargetRegion))
                    {
                        army.MoveTo(decision.TargetRegion);
                    }
                    break;
            }
        }
        
        private void ExecuteDefaultMovement(Army army)
        {
            // Find nearest enemy region and move toward it
            var enemyRegions = _gameState.Regions
                .Where(r => r.Owner != army.Owner && r.Owner != null)
                .ToList();
            
            if (!enemyRegions.Any()) return;
            
            var nearestEnemyRegion = enemyRegions
                .OrderBy(r => CalculateDistance(army.CurrentRegion, r))
                .First();
            
            var path = FindPathToRegion(army, nearestEnemyRegion);
            if (path.Count > 1 && army.CanMoveTo(path[1]))
            {
                army.MoveTo(path[1]);
                Console.WriteLine($"[AI] Default movement: Moving toward {nearestEnemyRegion.RegionName}");
            }
        }
        
        private void MakeStrategicDecisions(Player aiPlayer)
        {
            // Make high-level strategic decisions
            // This could include building new units, upgrading, etc.
            
            // For now, implement simple unit purchasing if resources are available
            if (aiPlayer.SilverCoins > 200)
            {
                // Purchase a random unit
                var availableUnitTypes = new[] { "infantry", "archer", "cavalry" };
                var randomType = availableUnitTypes[_random.Next(availableUnitTypes.Length)];
                
                var newUnit = DefaultUnits.CreateUnitByType(randomType, UnitRarity.Common);
                
                // Find an army to add the unit to
                var aiArmy = _gameState.GetPlayerArmies(aiPlayer).FirstOrDefault();
                if (aiArmy != null)
                {
                    // Simulate purchase (in real implementation, this would use the shop system)
                    aiPlayer.AvailableUnits.Add(newUnit);
                    aiArmy.AddUnit(newUnit);
                    aiPlayer.SilverCoins -= newUnit.Stats.SilverCost;
                    
                    Console.WriteLine($"[AI] Purchased {newUnit.UnitName} for {aiPlayer.PlayerName}");
                }
            }
        }
        
        private List<Region> GetAdjacentRegions(Region region)
        {
            return region.ConnectedRegions
                .Where(r => r != null)
                .ToList();
        }
        
        private List<Region> FindPathToRegion(Army army, Region targetRegion)
        {
            // Simple pathfinding - in real implementation, use proper pathfinding algorithm
            var path = new List<Region> { army.CurrentRegion };
            
            var current = army.CurrentRegion;
            int maxSteps = 10;
            
            while (current != targetRegion && maxSteps > 0)
            {
                var nextRegion = current.ConnectedRegions
                    .OrderBy(r => CalculateDistance(r, targetRegion))
                    .FirstOrDefault();
                
                if (nextRegion == null || path.Contains(nextRegion)) break;
                
                path.Add(nextRegion);
                current = nextRegion;
                maxSteps--;
            }
            
            return path;
        }
        
        private int CalculateDistance(Region from, Region to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }
        
        private Player GetAIPlayer()
        {
            return _gameState?.Players?.FirstOrDefault(p => p.PlayerId != _gameState.CurrentPlayer?.PlayerId);
        }
        
        public string GetAIDebugInfo()
        {
            var aiPlayer = GetAIPlayer();
            if (aiPlayer == null) return "No AI player";
            
            return $"""
            AI Player: {aiPlayer.PlayerName}
            Difficulty: {_difficulty}
            Behavior: {_behavior}
            Aggression: {_aggressionLevel:P0}
            Caution: {_cautionLevel:P0}
            Expansionism: {_expansionism:P0}
            Armies: {_gameState?.GetPlayerArmies(aiPlayer)?.Count ?? 0}
            Resources: {aiPlayer.SilverCoins} silver, {aiPlayer.GoldCoins} gold
            """;
        }
    }
    
    public class AIDecision
    {
        public AIDecisionType Type { get; set; }
        public Region TargetRegion { get; set; }
        public double Priority { get; set; }
        public string Description { get; set; }
    }
    
    public enum AIDecisionType
    {
        Attack,     // Attack enemy army
        Capture,    // Capture undefended region
        Regroup,    // Join with friendly army
        Position,   // Move to strategic position
        Retreat,    // Move away from danger
        Hold        // Stay in current position
    }
}