
// أضف في بداية GameManager.cs (قبل namespace)
using System.Reflection;
using WarRegions.Presentation.Interface2D.Scripts;
using WarRegions.Presentation.Interface3D.Scripts;
namespace WarRegions.Core.Controllers
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
        private GameState _gameState;
        private TerrainManager _terrainManager;
        // Event delegates
        public delegate void GameEvent(string message);
        public event GameEvent OnGameStart;
        public event GameEvent OnGameEnd;
        public event GameEvent OnTurnStart;
        public event GameEvent OnTurnEnd;
        public GameManager()
        {
            _terrainManager = new TerrainManager();
            CurrentGame = new GameState();
            _levelManager = new LevelManager();
            _battleCalculator = new BattleCalculator();
            //_aiController = new AIController(_gameState, _battleCalculator, _terrainManager);
            _aiController = new AIController(_gameState, _battleCalculator, _terrainManager);
            
            Console.WriteLine("[GAME] GameManager initialized");
        }
        
        // ✅ أضف هذه الدوال هنا - داخل كلاس GameManager:
        public void SetViewMode(ViewMode newMode)
        {
            CurrentViewManager?.CleanScreen();
            
            // استخدم Factory بدلاً من الـ Instance مباشرة
            CurrentViewManager = newMode switch
            {
                ViewMode.View2D => CreateViewManager2D(),
                ViewMode.View3D => CreateViewManager3D(),
                _ => CreateViewManager2D()
            };
            
            if (!CurrentViewManager.IsInitialized)
            {
                CurrentViewManager.Initialize();
            }
            
            Console.WriteLine($"[GAME] View mode set to: {newMode}");
        }

        private IViewManager CreateViewManager2D()
        {
            try
            {
                return ViewManager2D.Instance;
            }
            catch
            {
                // Fallback implementation
                return new SimpleViewManager2D();
            }
        }

        private IViewManager CreateViewManager3D()
        {
            try
            {
                return ViewManager3D.Instance;
            }
            catch
            {
                // Fallback implementation
                return new SimpleViewManager3D();
            }
        }
        public void SetViewMode(ViewMode newMode)
{
    // Clean up current view manager if exists
    CurrentViewManager?.CleanScreen();
    
    // Initialize the appropriate view manager
    CurrentViewManager = newMode switch
    {
        ViewMode.View2D => ViewManager2D.Instance,
        ViewMode.View3D => ViewManager3D.Instance,
        _ => ViewManager2D.Instance
    };
    
    // Initialize only if not already initialized
    if (!CurrentViewManager.IsInitialized)
    {
        CurrentViewManager.Initialize();
    }
    
    // Reinitialize terrain for the new view mode
    // ✅ صحيح - إنشاء instance جديد
    var terrainManager = new TerrainManager();
    terrainManager.SwitchViewMode(newMode);

    Console.WriteLine($"[GAME] View mode set to: {newMode}");
}
        public void Initialize(ViewMode startMode = ViewMode.View2D)
        {
            // Set up view manager based on mode
            SetViewMode(startMode);
            
            // Apply development configuration
            DevConfig.ApplyDevelopmentCheats(CurrentGame);  // ✅ صحيح
            
            Console.WriteLine($"[GAME] GameManager initialized with {startMode} view");
            OnGameStart?.Invoke("Game initialized successfully");
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
                if (DevConfig.UnlockAllUnits)  // ✅ صحيح - الخاصية الموجودة
                    DefaultUnits.AddDefaultUnitsToPlayer(humanPlayer);
                
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
            // Calculate upgrade cost
            var upgradeCost = UpgradeCost.CreateUnitUpgradeCost(unit.Rarity, unit.Level);
            var (silverCost, goldCost) = upgradeCost.CalculateCurrencyCostForLevel(unit.Level, unit.Level + 1);
            
            if (!Currency.CanAfford(unit.Owner, silverCost, goldCost))
            {
                Console.WriteLine($"[GAME] Cannot afford unit upgrade");
                return;
            }
            
            Currency.SpendCurrency(
                unit.Owner,
                silverCost,
                goldCost,
                $"Upgrade {unit.UnitName} to level {unit.Level + 1}"
            );
            
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
        }
        
        public string GetGameStatus()
        {
            if (!IsGameRunning)
            {
                return "Game not running";
            }
            
            if (CurrentGame.IsGameOver)
            {
                return $"Game Over - Winner: {CurrentGame.Winner?.PlayerName}";
            }
            
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
            Console.WriteLine("[GAME] GameManager disposed");
        }
    }
        
    public class SimpleViewManager2D : IViewManager
    {
        public bool IsInitialized { get; private set; }
        public void Initialize() { IsInitialized = true; Console.WriteLine("[Simple2D] Initialized"); }
        public void CleanScreen() { Console.WriteLine("[Simple2D] Screen cleaned"); }
        public void UpdateView() { Console.WriteLine("[Simple2D] View updated"); }
        public void RenderMap(List<Region> regions) { Console.WriteLine($"[Simple2D] Rendering {regions.Count} regions"); }
        public void RenderArmyDetails(Army army) { Console.WriteLine($"[Simple2D] Rendering {army.ArmyName}"); }
        public void ShowMessage(string message) { Console.WriteLine($"[Simple2D] {message}"); }
        public string GetUserInput() { return "simple_input"; }
    }

    public class SimpleViewManager3D : IViewManager
    {
        public bool IsInitialized { get; private set; }
        public void Initialize() { IsInitialized = true; Console.WriteLine("[Simple3D] Initialized"); }
        public void CleanScreen() { Console.WriteLine("[Simple3D] Screen cleaned"); }
        public void UpdateView() { Console.WriteLine("[Simple3D] View updated"); }
        public void RenderMap(List<Region> regions) { Console.WriteLine($"[Simple3D] Rendering {regions.Count} regions"); }
        public void RenderArmyDetails(Army army) { Console.WriteLine($"[Simple3D] Rendering {army.ArmyName}"); }
        public void ShowMessage(string message) { Console.WriteLine($"[Simple3D] {message}"); }
        public string GetUserInput() { return "simple_input"; }
    }
    private IViewManager CreateViewManager2D()
    {
        try
        {
            // المحاولة الأولى: استخدام Reflection لتحميل ViewManager2D الحقيقي
            var assembly = Assembly.Load("Presentation");
            var type = assembly.GetType("WarRegions.Presentation.Interface2D.Scripts.ViewManager2D");
            if (type != null)
            {
                var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProperty != null)
                {
                    var realViewManager = (IViewManager)instanceProperty.GetValue(null);
                    Console.WriteLine("[GAME] Loaded real ViewManager2D from Presentation");
                    return realViewManager;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GAME] Failed to load ViewManager2D: {ex.Message}");
        }
    
        // Fallback: استخدام الـ SimpleViewManager إذا فشل التحميل
        Console.WriteLine("[GAME] Using SimpleViewManager2D as fallback");
        return new SimpleViewManager2D();
    }

    private IViewManager CreateViewManager3D()
    {
        try
       {
            // المحاولة الأولى: استخدام Reflection لتحميل ViewManager3D الحقيقي
            var assembly = Assembly.Load("Presentation");
            var type = assembly.GetType("WarRegions.Presentation.Interface3D.Scripts.ViewManager3D");
        if (type != null)
        {
            var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceProperty != null)
            {
                var realViewManager = (IViewManager)instanceProperty.GetValue(null);
                Console.WriteLine("[GAME] Loaded real ViewManager3D from Presentation");
                return realViewManager;
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GAME] Failed to load ViewManager3D: {ex.Message}");
    }
    
    // Fallback: استخدام الـ SimpleViewManager إذا فشل التحميل
    Console.WriteLine("[GAME] Using SimpleViewManager3D as fallback");
    return new SimpleViewManager3D();
}
            
    public enum ViewMode
    {
        View2D,
        View3D
    }
}
