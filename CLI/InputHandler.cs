// CLI/InputHandler.cs
// Dependencies:
// - Core/Controllers/GameManager.cs
// - Core/Models/GameState.cs

using System;
using System.Collections.Generic;

namespace WarRegionsClone.CLI
{
    public class InputHandler
    {
        private GameManager _gameManager;
        private CLIViewManager _viewManager;
        private GameRenderer _renderer;

        public InputHandler(GameManager gameManager, CLIViewManager viewManager, GameRenderer renderer)
        {
            _gameManager = gameManager;
            _viewManager = viewManager;
            _renderer = renderer;
        }

        public void ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            var parts = command.ToUpper().Split(' ');
            var mainCommand = parts[0];

            try
            {
                switch (mainCommand)
                {
                    case "HELP":
                    case "H":
                        ShowHelp();
                        break;

                    case "MOVE":
                    case "M":
                        HandleMoveCommand(parts);
                        break;

                    case "ATTACK":
                    case "A":
                        HandleAttackCommand(parts);
                        break;

                    case "STATUS":
                    case "S":
                        HandleStatusCommand(parts);
                        break;

                    case "UNITS":
                    case "U":
                        HandleUnitsCommand(parts);
                        break;

                    case "MAP":
                        HandleMapCommand(parts);
                        break;

                    case "END":
                    case "E":
                        HandleEndTurnCommand();
                        break;

                    case "SAVE":
                        HandleSaveCommand(parts);
                        break;

                    case "LOAD":
                        HandleLoadCommand(parts);
                        break;

                    case "DEBUG":
                        HandleDebugCommand(parts);
                        break;

                    case "QUIT":
                    case "Q":
                        HandleQuitCommand();
                        break;

                    default:
                        _viewManager.ShowMessage($"Unknown command: {command}", MessageType.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                _viewManager.ShowMessage($"Command error: {ex.Message}", MessageType.Error);
            }
        }

        private void HandleMoveCommand(string[] parts)
        {
            if (_gameManager.CurrentGame?.CurrentPhase != GamePhase.PlayerTurn)
            {
                _viewManager.ShowMessage("Can only move during player turn", MessageType.Warning);
                return;
            }

            if (parts.Length < 4)
            {
                _viewManager.ShowMessage("Usage: MOVE [army_index] [target_x] [target_y]", MessageType.Info);
                return;
            }

            if (!int.TryParse(parts[1], out int armyIndex) ||
                !int.TryParse(parts[2], out int targetX) ||
                !int.TryParse(parts[3], out int targetY))
            {
                _viewManager.ShowMessage("Invalid parameters. Use numbers for army index and coordinates", MessageType.Error);
                return;
            }

            var playerArmies = _gameManager.CurrentGame.GetPlayerArmies(_gameManager.CurrentGame.CurrentPlayer);
            if (armyIndex < 0 || armyIndex >= playerArmies.Count)
            {
                _viewManager.ShowMessage($"Invalid army index. Available: 0-{playerArmies.Count - 1}", MessageType.Error);
                return;
            }

            var army = playerArmies[armyIndex];
            var targetRegion = _gameManager.CurrentGame.GetRegionAt(targetX, targetY);

            if (targetRegion == null)
            {
                _viewManager.ShowMessage($"No region at coordinates ({targetX}, {targetY})", MessageType.Error);
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

        private void HandleAttackCommand(string[] parts)
        {
            _viewManager.ShowMessage("Attack system - implementation in progress", MessageType.Info);
            // Implementation similar to move command
        }

        private void HandleStatusCommand(string[] parts)
        {
            if (_gameManager.CurrentGame == null)
            {
                _viewManager.ShowMessage("No active game", MessageType.Error);
                return;
            }

            if (parts.Length > 1)
            {
                switch (parts[1])
                {
                    case "DETAILED":
                    case "D":
                        ShowDetailedStatus();
                        break;
                    case "ECONOMY":
                    case "E":
                        ShowEconomyStatus();
                        break;
                    case "ARMIES":
                    case "A":
                        ShowArmiesStatus();
                        break;
                    default:
                        ShowQuickStatus();
                        break;
                }
            }
            else
            {
                ShowQuickStatus();
            }
        }

        private void ShowQuickStatus()
        {
            var game = _gameManager.CurrentGame;
            var player = game.CurrentPlayer;

            _viewManager.DisplayQuickStats(player, game);
        }

        private void ShowDetailedStatus()
        {
            var game = _gameManager.CurrentGame;
            var player = game.CurrentPlayer;

            Console.WriteLine("\n📊 DETAILED STATUS");
            Console.WriteLine("".PadRight(40, '='));
            
            Console.WriteLine($"Player: {player.PlayerName}");
            Console.WriteLine($"Resources: {Currency.GetPlayerBalance(player)}");
            Console.WriteLine($"Level: {player.LevelProgress}");
            Console.WriteLine($"Units: {player.AvailableUnits.Count}");
            
            Console.WriteLine($"\nTurn: {game.TurnNumber}");
            Console.WriteLine($"Phase: {game.CurrentPhase}");
            Console.WriteLine($"Regions Controlled: {game.Regions.Count(r => r.Owner == player)}");
            Console.WriteLine($"Total Armies: {game.Armies.Count(a => a.Owner == player)}");

            Console.WriteLine("".PadRight(40, '='));
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ShowEconomyStatus()
        {
            var player = _gameManager.CurrentGame?.CurrentPlayer;
            if (player == null) return;

            Console.WriteLine("\n💰 ECONOMY STATUS");
            Console.WriteLine("".PadRight(30, '='));
            
            Console.WriteLine($"Silver: {player.SilverCoins}🪙");
            Console.WriteLine($"Gold: {player.GoldCoins}💰");
            Console.WriteLine($"Units Owned: {player.AvailableUnits.Count}");
            
            if (player.CurrentDeck != null)
            {
                Console.WriteLine($"Current Deck: {player.CurrentDeck.DeckName}");
                Console.WriteLine($"Deck Value: {player.CurrentDeck.TotalSilverCost} silver");
            }
            
            Console.WriteLine("".PadRight(30, '='));
        }

        private void ShowArmiesStatus()
        {
            var game = _gameManager.CurrentGame;
            var player = game.CurrentPlayer;
            var armies = game.GetPlayerArmies(player);

            Console.WriteLine("\n⚔️ ARMIES STATUS");
            Console.WriteLine("".PadRight(35, '='));
            
            for (int i = 0; i < armies.Count; i++)
            {
                var army = armies[i];
                Console.WriteLine($"{i}. {army.ArmyName}");
                Console.WriteLine($"   Position: {army.CurrentRegion.RegionName} ({army.CurrentRegion.X},{army.CurrentRegion.Y})");
                Console.WriteLine($"   Units: {army.GetAliveUnitCount()}/{army.Units.Count}");
                Console.WriteLine($"   Movement: {army.MovementPoints}/{army.MaxMovementPoints}");
                Console.WriteLine();
            }
            
            Console.WriteLine("".PadRight(35, '='));
        }

        private void HandleUnitsCommand(string[] parts)
        {
            var player = _gameManager.CurrentGame?.CurrentPlayer;
            if (player == null) return;

            if (parts.Length > 1 && int.TryParse(parts[1], out int unitIndex))
            {
                // Show specific unit details
                if (unitIndex >= 0 && unitIndex < player.AvailableUnits.Count)
                {
                    _renderer.RenderUnitDetails(player.AvailableUnits[unitIndex]);
                }
                else
                {
                    _viewManager.ShowMessage($"Invalid unit index. Available: 0-{player.AvailableUnits.Count - 1}", MessageType.Error);
                }
            }
            else
            {
                // Show all units
                Console.WriteLine("\n🛡️ PLAYER UNITS");
                Console.WriteLine("".PadRight(35, '='));
                
                for (int i = 0; i < player.AvailableUnits.Count; i++)
                {
                    var unit = player.AvailableUnits[i];
                    Console.WriteLine($"{i}. {unit.UnitName} (Lvl {unit.Level}) - {unit.Stats.ToShortString()}");
                }
                
                Console.WriteLine("".PadRight(35, '='));
            }
        }

        private void HandleMapCommand(string[] parts)
        {
            var game = _gameManager.CurrentGame;
            if (game == null) return;

            if (parts.Length > 1)
            {
                switch (parts[1])
                {
                    case "DETAILED":
                    case "D":
                        ShowDetailedMap();
                        break;
                    case "REGIONS":
                    case "R":
                        ShowAllRegions();
                        break;
                    default:
                        _renderer.RenderMap(game.Regions, game.Armies);
                        break;
                }
            }
            else
            {
                _renderer.RenderMap(game.Regions, game.Armies);
            }
        }

        private void ShowDetailedMap()
        {
            var game = _gameManager.CurrentGame;
            Console.WriteLine("\n🗺️ DETAILED MAP INFO");
            Console.WriteLine("".PadRight(50, '='));
            
            foreach (var region in game.Regions)
            {
                _renderer.RenderRegionInfo(region);
            }
            
            Console.WriteLine("".PadRight(50, '='));
        }

        private void ShowAllRegions()
        {
            var game = _gameManager.CurrentGame;
            Console.WriteLine("\n📋 ALL REGIONS");
            Console.WriteLine("".PadRight(40, '='));
            
            foreach (var region in game.Regions)
            {
                string owner = region.Owner?.PlayerName ?? "Neutral";
                string army = region.OccupyingArmy?.ArmyName ?? "None";
                Console.WriteLine($"{region.RegionName} ({region.X},{region.Y}) - {region.Terrain} - {owner} - {army}");
            }
            
            Console.WriteLine("".PadRight(40, '='));
        }

        private void HandleEndTurnCommand()
        {
            if (_gameManager.CurrentGame?.CurrentPhase == GamePhase.PlayerTurn)
            {
                _gameManager.EndPlayerTurn();
                _viewManager.ShowMessage("Turn ended. AI is thinking...", MessageType.Info);
            }
            else
            {
                _viewManager.ShowMessage("Not your turn to end", MessageType.Warning);
            }
        }

        private void HandleSaveCommand(string[] parts)
        {
            string saveName = parts.Length > 1 ? parts[1] : "quicksave";
            _gameManager.SaveGame(saveName);
            _viewManager.ShowMessage($"Game saved as: {saveName}", MessageType.Success);
        }

        private void HandleLoadCommand(string[] parts)
        {
            string saveName = parts.Length > 1 ? parts[1] : "quicksave";
            _gameManager.LoadGame(saveName);
            _viewManager.ShowMessage($"Game loaded: {saveName}", MessageType.Success);
        }

        private void HandleDebugCommand(string[] parts)
        {
            Console.WriteLine("\n🐛 DEBUG INFORMATION");
            Console.WriteLine("".PadRight(35, '='));
            
            // Show development configuration
            Console.WriteLine(DevConfig.GetConfigSummary());
            Console.WriteLine();
            
            // Show game state
            if (_gameManager.CurrentGame != null)
            {
                _renderer.RenderGameStats(_gameManager.CurrentGame);
            }
            
            Console.WriteLine("".PadRight(35, '='));
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void HandleQuitCommand()
        {
            Console.Write("Are you sure you want to quit? (y/n): ");
            var response = Console.ReadLine()?.ToLower();
            
            if (response == "y" || response == "yes")
            {
                _viewManager.ShowMessage("Thanks for playing War Regions Clone!", MessageType.Info);
                Environment.Exit(0);
            }
        }

        private void ShowHelp()
        {
            Console.Clear();
            Console.WriteLine("🎮 WAR REGIONS CLONE - COMMAND HELP");
            Console.WriteLine("".PadRight(50, '='));
            
            Console.WriteLine("MOVEMENT & COMBAT:");
            Console.WriteLine("  MOVE [army] [x] [y]    - Move army to coordinates");
            Console.WriteLine("  ATTACK [army] [target] - Attack enemy army");
            Console.WriteLine("  END                    - End current turn");
            
            Console.WriteLine("\nINFORMATION:");
            Console.WriteLine("  STATUS                 - Show game status");
            Console.WriteLine("  STATUS DETAILED        - Detailed status");
            Console.WriteLine("  STATUS ECONOMY         - Economy status");
            Console.WriteLine("  UNITS                  - List all units");
            Console.WriteLine("  UNITS [index]          - Show unit details");
            Console.WriteLine("  MAP                    - Show game map");
            Console.WriteLine("  MAP DETAILED           - Detailed map info");
            
            Console.WriteLine("\nSYSTEM:");
            Console.WriteLine("  SAVE [name]            - Save game");
            Console.WriteLine("  LOAD [name]            - Load game");
            Console.WriteLine("  DEBUG                  - Debug information");
            Console.WriteLine("  HELP                   - This help screen");
            Console.WriteLine("  QUIT                   - Exit game");
            
            Console.WriteLine("".PadRight(50, '='));
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public string WaitForInput()
        {
            Console.Write("\n🎮 Command > ");
            return Console.ReadLine()?.Trim();
        }

        public int GetNumberInput(string prompt, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int result))
                {
                    if (result >= minValue && result <= maxValue)
                    {
                        return result;
                    }
                    _viewManager.ShowMessage($"Please enter a number between {minValue} and {maxValue}", MessageType.Warning);
                }
                else
                {
                    _viewManager.ShowMessage("Please enter a valid number", MessageType.Warning);
                }
            }
        }

        public string GetTextInput(string prompt, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(prompt);
                var input = Console.ReadLine()?.Trim();
                
                if (allowEmpty || !string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }
                _viewManager.ShowMessage("Please enter a valid value", MessageType.Warning);
            }
        }

        public bool GetConfirmation(string prompt)
        {
            Console.Write($"{prompt} (y/n): ");
            var response = Console.ReadLine()?.ToLower().Trim();
            return response == "y" || response == "yes";
        }
    }
}