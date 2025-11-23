using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.CLI
{
    // CLI/CLIViewManager.cs
    // Dependencies:
    // - Core/Interfaces/IViewManager.cs
    // - Core/Models/Region.cs
    // - Core/Models/Army.cs
    // - Core/Models/Units/UnitCard.cs
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace WarRegionsClone.CLI
    {
        public class CLIViewManager : IViewManager
        {
            private List<string> _messageHistory;
            private const int MAX_HISTORY = 20;
    
            public bool Is3D => false;
            public string ViewMode => "Command Line Interface";
    
            public CLIViewManager()
            {
                _messageHistory = new List<string>();
                Console.WriteLine("[CLI] CLIViewManager initialized");
            }
    
            public void Initialize()
            {
                Console.Title = "War Regions Clone - CLI Version";
                Console.CursorVisible = true;
                Console.WriteLine("[CLI] View manager initialized successfully");
            }
    
            public void UpdateView()
            {
                // View updates are handled by the main game loop
                // This method is kept for interface compliance
            }
    
            public void ShowRegion(Region region)
            {
                if (region == null) return;
    
                Console.WriteLine($"\n🗺️ REGION: {region.RegionName}");
                Console.WriteLine($"  Position: ({region.X}, {region.Y})");
                Console.WriteLine($"  Terrain: {region.Terrain}");
                Console.WriteLine($"  Owner: {region.Owner?.PlayerName ?? "Neutral"}");
                Console.WriteLine($"  Resources: {region.SilverProduction}🪙 {region.GoldProduction}💰");
                Console.WriteLine($"  Defense Bonus: {region.DefenseBonus}");
                Console.WriteLine($"  Passable: {region.IsPassable}");
            }
    
            public void ShowArmy(Army army)
            {
                if (army == null) return;
    
                Console.WriteLine($"\n⚔️ ARMY: {army.ArmyName}");
                Console.WriteLine($"  Owner: {army.Owner?.PlayerName}");
                Console.WriteLine($"  Position: {army.CurrentRegion?.RegionName ?? "Unknown"}");
                Console.WriteLine($"  Units: {army.GetAliveUnitCount()}/{army.Units.Count} alive");
                Console.WriteLine($"  Movement: {army.MovementPoints}/{army.MaxMovementPoints}");
                Console.WriteLine($"  Morale: {army.Morale:P0} | Supply: {army.Supply:P0}");
            }
    
            public void Cleanup()
            {
                Console.CursorVisible = true;
                Console.WriteLine("[CLI] View manager cleanup complete");
            }
    
            public void ShowMessage(string message, MessageType messageType = MessageType.Info)
            {
                string prefix = messageType switch
                {
                    MessageType.Info => "💡",
                    MessageType.Warning => "⚠️",
                    MessageType.Error => "❌",
                    MessageType.Success => "✅",
                    MessageType.Battle => "⚔️",
                    MessageType.System => "🔧",
                    _ => "💬"
                };
    
                string formattedMessage = $"{prefix} {message}";
                Console.WriteLine(formattedMessage);
                
                // Add to history
                _messageHistory.Add(formattedMessage);
                if (_messageHistory.Count > MAX_HISTORY)
                {
                    _messageHistory.RemoveAt(0);
                }
            }
    
            public void DisplayMap(List<Region> regions, List<Army> armies)
            {
                if (regions == null || regions.Count == 0)
                {
                    ShowMessage("No map data available", MessageType.Warning);
                    return;
                }
    
                // Find map boundaries
                int minX = regions.Min(r => r.X);
                int maxX = regions.Max(r => r.X);
                int minY = regions.Min(r => r.Y);
                int maxY = regions.Max(r => r.Y);
    
                Console.WriteLine("\n🗺️ GAME MAP");
                Console.WriteLine("".PadRight((maxX - minX + 1) * 3 + 2, '─'));
    
                for (int y = minY; y <= maxY; y++)
                {
                    Console.Write("│");
                    for (int x = minX; x <= maxX; x++)
                    {
                        var region = regions.FirstOrDefault(r => r.X == x && r.Y == y);
                        var army = armies.FirstOrDefault(a => a.CurrentRegion == region);
    
                        if (region != null)
                        {
                            // Choose symbol based on terrain and occupation
                            char symbol = GetMapSymbol(region, army);
                            ConsoleColor color = GetRegionColor(region);
    
                            Console.ForegroundColor = color;
                            Console.Write($" {symbol} ");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.Write("   "); // Empty space
                        }
                    }
                    Console.WriteLine("│");
                }
                Console.WriteLine("".PadRight((maxX - minX + 1) * 3 + 2, '─'));
    
                // Legend
                Console.WriteLine("\n📖 MAP LEGEND:");
                Console.WriteLine("  █ Player  ○ Enemy  ▲ Mountains  ≈ River");
                Console.WriteLine("  ♣ Forest  ░ Desert ~ Swamp      ⌂ Fortress");
                Console.WriteLine("  ▒ Plains");
            }
    
            private char GetMapSymbol(Region region, Army army)
            {
                if (army != null)
                {
                    return army.Owner?.PlayerId.StartsWith("AI") == true ? '○' : '█';
                }
    
                return region.Terrain.GetMapSymbol();
            }
    
            private ConsoleColor GetRegionColor(Region region)
            {
                return region.Terrain.GetConsoleColor();
            }
    
            public void DisplayUnitInfo(UnitCard unit)
            {
                if (unit == null) return;
    
                Console.WriteLine($"\n🛡️ UNIT: {unit.UnitName}");
                Console.WriteLine($"  Level: {unit.Level} | Rarity: {unit.Rarity}");
                Console.WriteLine($"  Health: {unit.CurrentHealth}/{unit.Stats.MaxHealth} {(unit.IsAlive ? "✅" : "❌")}");
                Console.WriteLine($"  Attack: {unit.Stats.Attack} | Defense: {unit.Stats.Defense}");
                Console.WriteLine($"  Speed: {unit.Stats.Speed} | Range: {unit.Stats.Range}");
                Console.WriteLine($"  Movement: {unit.MovementType}");
                
                if (!string.IsNullOrEmpty(unit.Description))
                {
                    Console.WriteLine($"  Description: {unit.Description}");
                }
            }
    
            public void DisplayBattleResult(BattleResult result)
            {
                if (result == null || !result.BattleWasFought) return;
    
                Console.WriteLine("\n⚔️ BATTLE RESULT");
                Console.WriteLine("".PadRight(40, '='));
                Console.WriteLine($"Location: {result.BattleRegion.RegionName}");
                Console.WriteLine($"Combatants: {result.Attacker.ArmyName} vs {result.Defender.ArmyName}");
                Console.WriteLine($"Result: {(result.AttackerWon ? "ATTACKER VICTORY 🎉" : "DEFENDER VICTORY 🛡️")}");
                Console.WriteLine($"Victory Margin: {result.VictoryMargin}");
                Console.WriteLine($"Casualties: {result.AttackerCasualties} (Attacker) / {result.DefenderCasualties} (Defender)");
                Console.WriteLine($"Rewards: {result.SilverReward}🪙 {result.GoldReward}💰");
                Console.WriteLine("".PadRight(40, '='));
            }
    
            public void DisplayPlayerStatus(Player player)
            {
                if (player == null) return;
    
                Console.WriteLine($"\n👤 PLAYER: {player.PlayerName}");
                Console.WriteLine($"  Resources: {Currency.GetPlayerBalance(player)}");
                Console.WriteLine($"  Level Progress: {player.LevelProgress}");
                Console.WriteLine($"  Available Units: {player.AvailableUnits.Count}");
                Console.WriteLine($"  Current Deck: {player.CurrentDeck?.DeckName ?? "None"}");
                
                // Battle statistics
                Console.WriteLine($"  Battles: {player.BattlesWon}/{player.TotalBattles} won");
                
                if (player.ShopEnabled)
                {
                    Console.WriteLine($"  Shop: ✅ Enabled");
                }
            }
    
            public string GetPlayerInput()
            {
                return Console.ReadLine();
            }
    
            public void ShowAvailableCommands()
            {
                Console.WriteLine("🎮 Commands: [H]elp [M]ove [A]ttack [E]nd Turn [S]tatus [U]nits [Q]uit");
            }
    
            public void ClearScreen()
            {
                Console.Clear();
            }
    
            public void ShowLoadingScreen(string message, int progress = 0)
            {
                Console.Clear();
                Console.WriteLine("🚀 War Regions Clone - Loading...");
                Console.WriteLine("".PadRight(40, '─'));
                Console.WriteLine(message);
                
                if (progress > 0)
                {
                    int bars = (int)(progress / 5.0);
                    string progressBar = "[" + new string('█', bars) + new string('░', 20 - bars) + "]";
                    Console.WriteLine($"{progressBar} {progress}%");
                }
                
                Console.WriteLine("".PadRight(40, '─'));
            }
    
            // Additional CLI-specific methods
            public void DisplayMessageHistory()
            {
                Console.WriteLine("\n📜 MESSAGE HISTORY");
                Console.WriteLine("".PadRight(40, '─'));
                
                foreach (var message in _messageHistory.TakeLast(10))
                {
                    Console.WriteLine(message);
                }
            }
    
            public void ShowError(string errorMessage)
            {
                ShowMessage(errorMessage, MessageType.Error);
                Console.Beep(); // Audio feedback for errors
            }
    
            public void DisplayQuickStats(Player player, GameState gameState)
            {
                Console.WriteLine($"\n📊 Quick Stats:");
                Console.WriteLine($"  Turn: {gameState.TurnNumber} | Regions: {gameState.Regions.Count(r => r.Owner == player)}");
                Console.WriteLine($"  Armies: {gameState.Armies.Count(a => a.Owner == player)}");
                Console.WriteLine($"  Resources: {Currency.GetPlayerBalance(player)}");
            }
        }
    }}
