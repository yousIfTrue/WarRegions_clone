using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.CLI
{
    // CLI/Program.cs
    // Dependencies:
    // - Core/Controllers/GameManager.cs
    // - Core/Models/Player.cs
    // - Core/Models/Level/LevelData.cs
    // - Development/DefaultUnits.cs
    // - CLI/CLIViewManager.cs
    
    using System;
    using System.Threading;
    
    namespace WarRegionsClone.CLI
    {
        class Program
        {
            private static GameManager _gameManager;
            private static CLIViewManager _viewManager;
            private static bool _isRunning = true;
    
            static void Main(string[] args)
            {
                try
                {
                    InitializeGame();
                    RunGameLoop();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Fatal error: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                finally
                {
                    Cleanup();
                }
            }
    
            static void InitializeGame()
            {
                Console.Clear();
                Console.WriteLine("🎮 Initializing War Regions Clone...");
                
                // Initialize view manager
                _viewManager = new CLIViewManager();
                _viewManager.ShowLoadingScreen("Initializing game systems", 10);
                
                // Initialize game manager
                _gameManager = new GameManager();
                _gameManager.Initialize(ViewMode.View2D);
                
                _viewManager.ShowLoadingScreen("Creating player", 40);
                
                // Create player
                var player = new Player();
                player.PlayerName = "Player1";
                
                // Add default units for development
                DefaultUnits.AddDefaultUnitsToPlayer(player);
                
                _viewManager.ShowLoadingScreen("Loading first level", 70);
                
                // Start new game
                _gameManager.StartNewGame(player, "level_01");
                
                _viewManager.ShowLoadingScreen("Ready to play", 100);
                Thread.Sleep(500);
                
                Console.Clear();
                _viewManager.ShowMessage("🚀 War Regions Clone Started Successfully!", MessageType.Success);
                _viewManager.ShowMessage("Type 'HELP' for available commands", MessageType.Info);
            }
    
            static void RunGameLoop()
            {
                while (_isRunning && _gameManager.IsGameRunning)
                {
                    try
                    {
                        DisplayGameState();
                        ProcessPlayerInput();
                        
                        // Small delay to prevent CPU overuse
                        Thread.Sleep(50);
                    }
                    catch (Exception ex)
                    {
                        _viewManager.ShowMessage($"Game loop error: {ex.Message}", MessageType.Error);
                    }
                }
                
                if (_gameManager.IsGameOver)
                {
                    DisplayGameOver();
                }
            }
    
            static void DisplayGameState()
            {
                _viewManager.ClearScreen();
                
                // Display game header
                Console.WriteLine("═".PadRight(60, '═'));
                Console.WriteLine("🎯 WAR REGIONS CLONE - COMMAND LINE VERSION");
                Console.WriteLine("═".PadRight(60, '═'));
                
                // Display current game status
                var status = _gameManager.GetGameStatus();
                Console.WriteLine($"📊 {status}");
                
                // Display map
                if (_gameManager.CurrentGame?.Regions != null)
                {
                    _viewManager.DisplayMap(_gameManager.CurrentGame.Regions, _gameManager.CurrentGame.Armies);
                }
                
                // Display player status
                if (_gameManager.CurrentGame?.CurrentPlayer != null)
                {
                    _viewManager.DisplayPlayerStatus(_gameManager.CurrentGame.CurrentPlayer);
                }
                
                Console.WriteLine("─".PadRight(60, '─'));
                _viewManager.ShowAvailableCommands();
            }
    
            static void ProcessPlayerInput()
            {
                Console.Write("🎮 Command > ");
                var input = _viewManager.GetPlayerInput()?.ToUpper().Trim();
                
                if (string.IsNullOrEmpty(input))
                    return;
    
                switch (input)
                {
                    case "HELP":
                    case "H":
                        ShowHelp();
                        break;
                        
                    case "MOVE":
                    case "M":
                        ProcessMoveCommand();
                        break;
                        
                    case "ATTACK":
                    case "A":
                        ProcessAttackCommand();
                        break;
                        
                    case "END TURN":
                    case "E":
                        _gameManager.EndPlayerTurn();
                        _viewManager.ShowMessage("Turn ended. Processing AI turn...", MessageType.Info);
                        break;
                        
                    case "STATUS":
                    case "S":
                        ShowDetailedStatus();
                        break;
                        
                    case "UNITS":
                    case "U":
                        ShowUnits();
                        break;
                        
                    case "MAP":
                        ShowMapDetails();
                        break;
                        
                    case "SAVE":
                        _gameManager.SaveGame("quick_save");
                        break;
                        
                    case "LOAD":
                        _gameManager.LoadGame("quick_save");
                        break;
                        
                    case "DEBUG":
                        ShowDebugInfo();
                        break;
                        
                    case "QUIT":
                    case "Q":
                        _isRunning = false;
                        _viewManager.ShowMessage("Thanks for playing!", MessageType.Info);
                        break;
                        
                    default:
                        _viewManager.ShowMessage($"Unknown command: {input}. Type 'HELP' for available commands.", MessageType.Warning);
                        break;
                }
            }
    
            static void ProcessMoveCommand()
            {
                try
                {
                    Console.Write("Enter army index to move: ");
                    if (!int.TryParse(Console.ReadLine(), out int armyIndex))
                    {
                        _viewManager.ShowMessage("Invalid army index", MessageType.Error);
                        return;
                    }
    
                    Console.Write("Enter target X coordinate: ");
                    if (!int.TryParse(Console.ReadLine(), out int targetX))
                    {
                        _viewManager.ShowMessage("Invalid X coordinate", MessageType.Error);
                        return;
                    }
    
                    Console.Write("Enter target Y coordinate: ");
                    if (!int.TryParse(Console.ReadLine(), out int targetY))
                    {
                        _viewManager.ShowMessage("Invalid Y coordinate", MessageType.Error);
                        return;
                    }
    
                    var army = _gameManager.CurrentGame?.GetPlayerArmies(_gameManager.CurrentGame.CurrentPlayer)?.ElementAtOrDefault(armyIndex);
                    var targetRegion = _gameManager.CurrentGame?.GetRegionAt(targetX, targetY);
    
                    if (army == null || targetRegion == null)
                    {
                        _viewManager.ShowMessage("Invalid army or target position", MessageType.Error);
                        return;
                    }
    
                    bool success = _gameManager.MoveArmy(army, targetRegion);
                    if (success)
                    {
                        _viewManager.ShowMessage($"Army moved to {targetRegion.RegionName}", MessageType.Success);
                    }
                    else
                    {
                        _viewManager.ShowMessage("Move failed - check movement points or terrain", MessageType.Error);
                    }
                }
                catch (Exception ex)
                {
                    _viewManager.ShowMessage($"Move error: {ex.Message}", MessageType.Error);
                }
            }
    
            static void ProcessAttackCommand()
            {
                _viewManager.ShowMessage("Attack command - implementation in progress", MessageType.Info);
                // Similar implementation to move command
            }
    
            static void ShowHelp()
            {
                _viewManager.ClearScreen();
                Console.WriteLine("🎮 AVAILABLE COMMANDS");
                Console.WriteLine("═".PadRight(40, '═'));
                Console.WriteLine("HELP (H)     - Show this help screen");
                Console.WriteLine("MOVE (M)     - Move an army to new position");
                Console.WriteLine("ATTACK (A)   - Attack an enemy army");
                Console.WriteLine("END TURN (E) - End current turn");
                Console.WriteLine("STATUS (S)   - Show detailed game status");
                Console.WriteLine("UNITS (U)    - Show player's units");
                Console.WriteLine("MAP          - Show detailed map information");
                Console.WriteLine("SAVE         - Save current game");
                Console.WriteLine("LOAD         - Load saved game");
                Console.WriteLine("DEBUG        - Show debug information");
                Console.WriteLine("QUIT (Q)     - Exit the game");
                Console.WriteLine("═".PadRight(40, '═'));
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
    
            static void ShowDetailedStatus()
            {
                _viewManager.ClearScreen();
                
                if (_gameManager.CurrentGame == null)
                {
                    _viewManager.ShowMessage("No active game", MessageType.Error);
                    return;
                }
    
                var player = _gameManager.CurrentGame.CurrentPlayer;
                var game = _gameManager.CurrentGame;
    
                Console.WriteLine("📊 DETAILED GAME STATUS");
                Console.WriteLine("═".PadRight(50, '═'));
                
                Console.WriteLine($"Player: {player.PlayerName}");
                Console.WriteLine($"Resources: {Currency.GetPlayerBalance(player)}");
                Console.WriteLine($"Level Progress: {player.LevelProgress}");
                Console.WriteLine($"Available Units: {player.AvailableUnits.Count}");
                
                Console.WriteLine("\n🎯 Game State:");
                Console.WriteLine($"Turn: {game.TurnNumber}");
                Console.WriteLine($"Phase: {game.CurrentPhase}");
                Console.WriteLine($"Regions: {game.Regions.Count}");
                Console.WriteLine($"Armies: {game.Armies.Count}");
                Console.WriteLine($"Players: {game.Players.Count}");
                
                Console.WriteLine("\n⚔️ Player Armies:");
                var playerArmies = game.GetPlayerArmies(player);
                for (int i = 0; i < playerArmies.Count; i++)
                {
                    var army = playerArmies[i];
                    Console.WriteLine($"  {i}. {army.ArmyName} - {army.GetAliveUnitCount()} units - MP: {army.MovementPoints}/{army.MaxMovementPoints}");
                    Console.WriteLine($"     Position: {army.CurrentRegion.RegionName} ({army.CurrentRegion.X},{army.CurrentRegion.Y})");
                }
    
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
    
            static void ShowUnits()
            {
                _viewManager.ClearScreen();
                
                var player = _gameManager.CurrentGame?.CurrentPlayer;
                if (player == null) return;
    
                Console.WriteLine("🛡️ PLAYER UNITS");
                Console.WriteLine("═".PadRight(60, '═'));
                
                foreach (var unit in player.AvailableUnits)
                {
                    _viewManager.DisplayUnitCard(unit);
                    Console.WriteLine();
                }
                
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
    
            static void ShowMapDetails()
            {
                _viewManager.ClearScreen();
                
                if (_gameManager.CurrentGame?.Regions == null) return;
    
                Console.WriteLine("🗺️ DETAILED MAP INFORMATION");
                Console.WriteLine("═".PadRight(50, '═'));
                
                foreach (var region in _gameManager.CurrentGame.Regions)
                {
                    string owner = region.Owner?.PlayerName ?? "Neutral";
                    string army = region.OccupyingArmy?.ArmyName ?? "None";
                    
                    Console.WriteLine($"{region.RegionName} ({region.X},{region.Y})");
                    Console.WriteLine($"  Terrain: {region.Terrain} | Owner: {owner}");
                    Console.WriteLine($"  Army: {army} | Resources: {region.SilverProduction}S {region.GoldProduction}G");
                    Console.WriteLine();
                }
                
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
    
            static void ShowDebugInfo()
            {
                _viewManager.ClearScreen();
                
                Console.WriteLine("🐛 DEBUG INFORMATION");
                Console.WriteLine("═".PadRight(50, '═'));
                
                // Development config
                Console.WriteLine(DevConfig.GetConfigSummary());
                Console.WriteLine();
                
                // Game state debug
                if (_gameManager.CurrentGame != null)
                {
                    Console.WriteLine("Game State:");
                    Console.WriteLine($"  Turn: {_gameManager.CurrentGame.TurnNumber}");
                    Console.WriteLine($"  Phase: {_gameManager.CurrentGame.CurrentPhase}");
                    Console.WriteLine($"  Game Over: {_gameManager.CurrentGame.IsGameOver}");
                    Console.WriteLine($"  Winner: {_gameManager.CurrentGame.Winner?.PlayerName}");
                }
                
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
    
            static void DisplayGameOver()
            {
                _viewManager.ClearScreen();
                
                Console.WriteLine("🎉 GAME OVER");
                Console.WriteLine("═".PadRight(40, '═'));
                
                if (_gameManager.CurrentGame?.Winner != null)
                {
                    Console.WriteLine($"🏆 WINNER: {_gameManager.CurrentGame.Winner.PlayerName}");
                }
                else
                {
                    Console.WriteLine("🤝 Draw game");
                }
                
                Console.WriteLine($"📅 Total turns: {_gameManager.CurrentGame?.TurnNumber}");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
    
            static void Cleanup()
            {
                _gameManager?.Dispose();
                _viewManager?.Cleanup();
                Console.WriteLine("🎮 Game shutdown complete. Goodbye!");
            }
        }
    }}
