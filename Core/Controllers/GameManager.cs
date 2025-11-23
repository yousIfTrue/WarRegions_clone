using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Controllers
{
    // Core/Controllers/GameManager.cs
    // Dependencies:
    // - Models/GameState.cs
    // - Models/Player.cs
    // - Models/Level/LevelData.cs
    // - Development/DevConfig.cs
    // - Development/DefaultUnits.cs
    // - Interfaces/IViewManager.cs
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace WarRegionsClone.Controllers
    {
        public class GameManager
        {
            public GameState CurrentGame { get; private set; }
            public IViewManager CurrentViewManager { get; private set; }
            
            // Game flow control
            public bool IsGameRunning { get; private set; }
            public bool IsPaused { get; private set; }
            
            // Managers
            private LevelManager _levelManager;
            private BattleCalculator _battleCalculator;
            private AIController _aiController;
            
            // Event delegates
            public delegate void GameEvent(string message);
            public event GameEvent OnGameStart;
            public event GameEvent OnGameEnd;
            public event GameEvent OnTurnStart;
            public event GameEvent OnTurnEnd;
            
            public GameManager()
            {
                CurrentGame = new GameState();
                _levelManager = new LevelManager();
                _battleCalculator = new BattleCalculator();
                _aiController = new AIController();
                
                Console.WriteLine("[GAME] GameManager initialized");
            }
            
            public void Initialize(ViewMode startMode = ViewMode.View2D)
            {
                // Set up view manager based on mode
                SetViewMode(startMode);
                
                // Apply development configuration
                DevConfig.ApplyDevelopmentCheats(CurrentGame.Players.FirstOrDefault());
                
                Console.WriteLine($"[GAME] GameManager initialized with {startMode} view");
                
                OnGameStart?.Invoke("Game initialized successfully");
            }
            
            public void SetViewMode(ViewMode newMode)
            {
                // Clean up current view manager if exists
                CurrentViewManager?.Cleanup();
                
                CurrentViewManager = newMode switch
                {
                    ViewMode.View2D => new ViewManager2D(),
                    ViewMode.View3D => new ViewManager3D(),
                    _ => new ViewManager2D()
                };
                
                CurrentViewManager.Initialize();
                
                Console.WriteLine($"[GAME] View mode set to: {newMode}");
            }
            
            public void StartNewGame(Player humanPlayer, string levelId = "level_01")
            {
                try
                {
                    // Load level data
                    var level = _levelManager.LoadLevel(levelId);
                    if (level == null)
                    {
                        Console.WriteLine($"[GAME] Failed to load level: {levelId}");
                        return;
                    }
                    
                    // Add default units to player for development
                    if (DevConfig.AllUnitsUnlocked)
                    {
                        DefaultUnits.AddDefaultUnitsToPlayer(humanPlayer);
                    }
                    
                    // Initialize game state
                    CurrentGame.InitializeNewGame(humanPlayer, level);
                    
                    IsGameRunning = true;
                    IsPaused = false;
                    
                    Console.WriteLine($"[GAME] New game started - Level: {level.LevelName}");
                    
                    // Initialize AI
                    _aiController.Initialize(CurrentGame);
                    
                    OnGameStart?.Invoke($"New game started: {level.LevelName}");
                    
                    // Start first turn
                    StartPlayerTurn();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GAME ERROR] Failed to start new game: {ex.Message}");
                }
            }
            
            public void StartPlayerTurn()
            {
                if (!IsGameRunning || CurrentGame.IsGameOver) return;
                
                CurrentGame.CurrentPhase = GamePhase.PlayerTurn;
                
                // Reset movement points for player's armies
                var playerArmies = CurrentGame.GetPlayerArmies(CurrentGame.CurrentPlayer);
                foreach (var army in playerArmies)
                {
                    army.ResetMovementPoints();
                }
                
                Console.WriteLine($"[GAME] Player turn started - Turn {CurrentGame.TurnNumber}");
                OnTurnStart?.Invoke($"Player turn {CurrentGame.TurnNumber} started");
                
                // Update view
                CurrentViewManager?.UpdateView();
            }
            
            public void EndPlayerTurn()
            {
                if (!IsGameRunning) return;
                
                Console.WriteLine($"[GAME] Player turn ended");
                
                // Process AI turn
                ProcessAITurn();
                
                // Check win conditions
                CurrentGame.CheckWinConditions();
                
                if (CurrentGame.IsGameOver)
                {
                    EndGame();
                    return;
                }
                
                // Start next turn
                CurrentGame.EndTurn();
                StartPlayerTurn();
                
                OnTurnEnd?.Invoke($"Player turn {CurrentGame.TurnNumber - 1} ended");
            }
            
            private void ProcessAITurn()
            {
                CurrentGame.CurrentPhase = GamePhase.AITurn;
                
                Console.WriteLine($"[GAME] Processing AI turn...");
                
                // Let AI make decisions
                _aiController.ProcessTurn(CurrentGame);
                
                Console.WriteLine($"[GAME] AI turn completed");
            }
            
            public bool MoveArmy(Army army, Region targetRegion)
            {
                if (!IsGameRunning || CurrentGame.CurrentPhase != GamePhase.PlayerTurn)
                {
                    Console.WriteLine("[GAME] Cannot move army - not player turn or game not running");
                    return false;
                }
                
                if (army.Owner != CurrentGame.CurrentPlayer)
                {
                    Console.WriteLine("[GAME] Cannot move enemy army");
                    return false;
                }
                
                if (!army.CanMoveTo(targetRegion))
                {
                    Console.WriteLine($"[GAME] Army cannot move to {targetRegion.RegionName}");
                    return false;
                }
                
                // Check if target region is occupied by enemy
                if (targetRegion.OccupyingArmy != null && targetRegion.OccupyingArmy.Owner != army.Owner)
                {
                    // Start battle
                    return StartBattle(army, targetRegion.OccupyingArmy, targetRegion);
                }
                else
                {
                    // Normal movement
                    army.MoveTo(targetRegion);
                    CurrentViewManager?.UpdateView();
                    return true;
                }
            }
            
            private bool StartBattle(Army attacker, Army defender, Region battleRegion)
            {
                Console.WriteLine($"[BATTLE] Battle started at {battleRegion.RegionName}!");
                
                CurrentGame.CurrentPhase = GamePhase.Battle;
                
                // Calculate battle result
                var result = _battleCalculator.CalculateBattle(attacker, defender, battleRegion);
                
                // Apply battle results
                ApplyBattleResult(result);
                
                // If attacker wins and defender is destroyed, move into region
                if (result.AttackerWon && defender.GetAliveUnitCount() == 0)
                {
                    attacker.MoveTo(battleRegion);
                }
                
                CurrentGame.CurrentPhase = GamePhase.PlayerTurn;
                CurrentViewManager?.UpdateView();
                
                return result.AttackerWon;
            }
            
            private void ApplyBattleResult(BattleResult result)
            {
                // Apply casualties
                result.Attacker.TakeCasualties(result.AttackerCasualties);
                result.Defender.TakeCasualties(result.DefenderCasualties);
                
                // Apply experience to surviving units
                foreach (var unit in result.Attacker.Units.Where(u => u.IsAlive))
                {
                    unit.Experience += result.ExperienceGained;
                }
                
                // Award resources to winner
                if (result.AttackerWon)
                {
                    Currency.AddCurrency(
                        result.Attacker.Owner,
                        result.SilverReward,
                        result.GoldReward,
                        "battle victory"
                    );
                }
                else
                {
                    Currency.AddCurrency(
                        result.Defender.Owner, 
                        result.SilverReward,
                        result.GoldReward,
                        "battle defense"
                    );
                }
                
                Console.WriteLine($"[BATTLE] Battle finished - {(result.AttackerWon ? "Attacker" : "Defender")} wins!");
            }
            
            public void PurchaseUnit(UnitCard unit, Army targetArmy = null)
            {
                if (!IsGameRunning) return;
                
                var player = CurrentGame.CurrentPlayer;
                
                // Check if player can afford the unit
                if (!Currency.CanAfford(player, unit.Stats.SilverCost, unit.Stats.GoldCost))
                {
                    Console.WriteLine($"[GAME] Cannot afford {unit.UnitName}");
                    return;
                }
                
                // Spend currency
                var transaction = Currency.SpendCurrency(
                    player, 
                    unit.Stats.SilverCost, 
                    unit.Stats.GoldCost, 
                    $"Purchase {unit.UnitName}"
                );
                
                if (!transaction.Success) return;
                
                // Add unit to player's available units
                player.AvailableUnits.Add(unit);
                
                // If target army specified, add to that army
                if (targetArmy != null && targetArmy.Owner == player)
                {
                    targetArmy.AddUnit(unit);
                }
                
                Console.WriteLine($"[GAME] Purchased {unit.UnitName} for army");
            }
            
            public void UpgradeUnit(UnitCard unit)
            {
                if (!IsGameRunning) return;
                
                // Calculate upgrade cost
                var upgradeCost = UpgradeCost.CreateUnitUpgradeCost(unit.Rarity, unit.Level);
                var (silverCost, goldCost) = upgradeCost.CalculateCurrencyCostForLevel(unit.Level, unit.Level + 1);
                
                if (!Currency.CanAfford(unit.Owner, silverCost, goldCost))
                {
                    Console.WriteLine($"[GAME] Cannot afford unit upgrade");
                    return;
                }
                
                // Spend currency
                var transaction = Currency.SpendCurrency(
                    unit.Owner,
                    silverCost,
                    goldCost,
                    $"Upgrade {unit.UnitName} to level {unit.Level + 1}"
                );
                
                if (!transaction.Success) return;
                
                // Apply upgrade
                unit.Stats.Upgrade(unit.Level);
                unit.Level++;
                
                Console.WriteLine($"[GAME] Upgraded {unit.UnitName} to level {unit.Level}");
            }
            
            public void PauseGame()
            {
                IsPaused = true;
                Console.WriteLine("[GAME] Game paused");
            }
            
            public void ResumeGame()
            {
                IsPaused = false;
                Console.WriteLine("[GAME] Game resumed");
            }
            
            public void SaveGame(string saveName)
            {
                // This would implement actual save functionality
                Console.WriteLine($"[GAME] Game saved as: {saveName}");
            }
            
            public void LoadGame(string saveName)
            {
                // This would implement actual load functionality
                Console.WriteLine($"[GAME] Game loaded: {saveName}");
            }
            
            private void EndGame()
            {
                IsGameRunning = false;
                
                Console.WriteLine($"[GAME] Game over! Winner: {CurrentGame.Winner?.PlayerName ?? "None"}");
                
                // Calculate and award rewards
                if (CurrentGame.Winner == CurrentGame.Players[0]) // Human player
                {
                    var level = CurrentGame.CurrentLevel;
                    level.CalculateRewards(CurrentGame.Players[0], CurrentGame.TurnNumber);
                }
                
                OnGameEnd?.Invoke($"Game over! Winner: {CurrentGame.Winner?.PlayerName}");
                
                CurrentViewManager?.UpdateView();
            }
            
            public string GetGameStatus()
            {
                if (!IsGameRunning)
                    return "Game not running";
                    
                if (CurrentGame.IsGameOver)
                    return $"Game Over - Winner: {CurrentGame.Winner?.PlayerName}";
                    
                return $"""
                Game Status:
                Turn: {CurrentGame.TurnNumber}
                Phase: {CurrentGame.CurrentPhase}
                Current Player: {CurrentGame.CurrentPlayer?.PlayerName}
                Regions: {CurrentGame.Regions.Count}
                Armies: {CurrentGame.Armies.Count}
                Game Over: {CurrentGame.IsGameOver}
                """;
            }
            
            public void Dispose()
            {
                CurrentViewManager?.Cleanup();
                IsGameRunning = false;
                Console.WriteLine("[GAME] GameManager disposed");
            }
        }
        
        public enum ViewMode
        {
            View2D,
            View3D
        }
    }}
