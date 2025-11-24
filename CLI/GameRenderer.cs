using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.CLI
{
    // CLI/GameRenderer.cs
    // Dependencies:
    // - Core/Models/Region.cs
    // - Core/Models/Army.cs
    // - Core/Models/Terrain/TerrainType.cs
    
    namespace WarRegions.CLI
    {
        public class GameRenderer
        {
            public void RenderMap(List<Region> regions, List<Army> armies)
            {
                if (regions == null || !regions.Any())
                {
                    Console.WriteLine("No map data available");
                    return;
                }
    
                // Calculate map boundaries
                int minX = regions.Min(r => r.X);
                int maxX = regions.Max(r => r.X);
                int minY = regions.Min(r => r.Y);
                int maxY = regions.Max(r => r.Y);
    
                Console.WriteLine("\n🗺️ BATTLE MAP");
                RenderMapBorder(minX, maxX);
    
                for (int y = minY; y <= maxY; y++)
                {
                    Console.Write("│");
                    for (int x = minX; x <= maxX; x++)
                    {
                        RenderMapTile(x, y, regions, armies);
                    }
                    Console.WriteLine("│");
                }
                
                RenderMapBorder(minX, maxX);
                RenderMapLegend();
            }
    
            private void RenderMapTile(int x, int y, List<Region> regions, List<Army> armies)
            {
                var region = regions.FirstOrDefault(r => r.X == x && r.Y == y);
                var army = armies.FirstOrDefault(a => a.CurrentRegion?.X == x && a.CurrentRegion?.Y == y);
    
                if (region == null)
                {
                    Console.Write("   "); // Empty space
                    return;
                }
    
                string tileContent = GetTileContent(region, army);
                ConsoleColor color = GetTileColor(region, army);
                
                Console.ForegroundColor = color;
                Console.Write(tileContent);
                Console.ResetColor();
            }
    
            private string GetTileContent(Region region, Army army)
            {
                if (army != null)
                {
                    // Show army with owner indicator
                    return army.Owner?.PlayerId.StartsWith("AI") == true ? " ○ " : " █ ";
                }
    
                // Show terrain
                return region.Terrain switch
                {
                    TerrainType.Plains => " ▒ ",
                    TerrainType.Mountains => " ▲ ",
                    TerrainType.Forest => " ♣ ",
                    TerrainType.River => " ≈ ",
                    TerrainType.Desert => " ░ ",
                    TerrainType.Swamp => " ~ ",
                    TerrainType.Fortress => " ⌂ ",
                    _ => " ? "
                };
            }
    
            private ConsoleColor GetTileColor(Region region, Army army)
            {
                if (army != null)
                {
                    return army.Owner?.PlayerId.StartsWith("AI") == true ? ConsoleColor.Red : ConsoleColor.Green;
                }
    
                return region.Terrain.GetConsoleColor();
            }
    
            private void RenderMapBorder(int minX, int maxX)
            {
                int width = (maxX - minX + 1) * 3;
                Console.WriteLine("".PadRight(width + 2, '─'));
            }
    
            private void RenderMapLegend()
            {
                Console.WriteLine("\n📖 LEGEND:");
                Console.WriteLine("  █ Your Army  ○ Enemy  ▒ Plains  ▲ Mountains");
                Console.WriteLine("  ♣ Forest     ≈ River  ░ Desert  ~ Swamp");
                Console.WriteLine("  ⌂ Fortress");
            }
    
            public void RenderBattleAnimation(Army attacker, Army defender, BattleResult result)
            {
                Console.Clear();
                Console.WriteLine("⚔️ BATTLE IN PROGRESS ⚔️");
                Console.WriteLine("".PadRight(40, '='));
                
                // Simple ASCII animation
                string[] animationFrames = {
                    $"{attacker.ArmyName} → → → {defender.ArmyName}",
                    $"{attacker.ArmyName} → → 🏹 {defender.ArmyName}",
                    $"{attacker.ArmyName} → ⚔️ → {defender.ArmyName}",
                    $"{attacker.ArmyName} 🗡️ ⚔️ 🛡️ {defender.ArmyName}"
                };
    
                foreach (var frame in animationFrames)
                {
                    Console.WriteLine($"\r{frame}");
                    System.Threading.Thread.Sleep(300);
                }
    
                Console.WriteLine("\n" + "".PadRight(40, '='));
            }
    
            public void RenderUnitDetails(UnitCard unit)
            {
                Console.WriteLine("\n🛡️ UNIT DETAILS");
                Console.WriteLine("".PadRight(30, '─'));
                
                Console.WriteLine($"Name: {unit.UnitName}");
                Console.WriteLine($"Level: {unit.Level} | Rarity: {unit.Rarity}");
                Console.WriteLine($"Health: {unit.CurrentHealth}/{unit.Stats.MaxHealth} {(unit.IsAlive ? "✅" : "❌")}");
                Console.WriteLine($"Attack: {unit.Stats.Attack} | Defense: {unit.Stats.Defense}");
                Console.WriteLine($"Speed: {unit.Stats.Speed} | Range: {unit.Stats.Range}");
                Console.WriteLine($"Movement: {unit.MovementType}");
                
                if (!string.IsNullOrEmpty(unit.Description))
                {
                    Console.WriteLine($"Description: {unit.Description}");
                }
                
                Console.WriteLine("".PadRight(30, '─'));
            }
    
            public void RenderArmyStatus(Army army)
            {
                Console.WriteLine("\n⚔️ ARMY STATUS");
                Console.WriteLine("".PadRight(35, '─'));
                
                Console.WriteLine($"Name: {army.ArmyName}");
                Console.WriteLine($"Owner: {army.Owner?.PlayerName}");
                Console.WriteLine($"Position: {army.CurrentRegion?.RegionName}");
                Console.WriteLine($"Movement: {army.MovementPoints}/{army.MaxMovementPoints} MP");
                Console.WriteLine($"Units: {army.GetAliveUnitCount()}/{army.Units.Count} alive");
                Console.WriteLine($"Morale: {army.Morale:P0} | Supply: {army.Supply:P0}");
                
                if (army.Units.Any())
                {
                    Console.WriteLine("\nUnits:");
                    foreach (var unit in army.Units.Where(u => u.IsAlive))
                    {
                        Console.WriteLine($"  {unit.UnitName} - {unit.CurrentHealth}/{unit.Stats.MaxHealth} HP");
                    }
                }
                
                Console.WriteLine("".PadRight(35, '─'));
            }
    
            public void RenderRegionInfo(Region region)
            {
                Console.WriteLine("\n🗺️ REGION INFO");
                Console.WriteLine("".PadRight(30, '─'));
                
                Console.WriteLine($"Name: {region.RegionName}");
                Console.WriteLine($"Position: ({region.X}, {region.Y})");
                Console.WriteLine($"Terrain: {region.Terrain}");
                Console.WriteLine($"Owner: {region.Owner?.PlayerName ?? "Neutral"}");
                Console.WriteLine($"Resources: {region.SilverProduction}🪙 {region.GoldProduction}💰");
                Console.WriteLine($"Defense Bonus: {region.DefenseBonus}");
                Console.WriteLine($"Passable: {(region.IsPassable ? "Yes" : "No")}");
                
                if (region.OccupyingArmy != null)
                {
                    Console.WriteLine($"Occupying Army: {region.OccupyingArmy.ArmyName}");
                }
                
                Console.WriteLine("".PadRight(30, '─'));
            }
    
            public void RenderGameStats(GameState gameState)
            {
                Console.WriteLine("\n📊 GAME STATISTICS");
                Console.WriteLine("".PadRight(25, '─'));
                
                Console.WriteLine($"Turn: {gameState.TurnNumber}");
                Console.WriteLine($"Phase: {gameState.CurrentPhase}");
                Console.WriteLine($"Regions: {gameState.Regions.Count}");
                Console.WriteLine($"Armies: {gameState.Armies.Count}");
                Console.WriteLine($"Players: {gameState.Players.Count}");
                Console.WriteLine($"Game Over: {gameState.IsGameOver}");
                
                if (gameState.Winner != null)
                {
                    Console.WriteLine($"Winner: {gameState.Winner.PlayerName}");
                }
                
                Console.WriteLine("".PadRight(25, '─'));
            }
    
            public void RenderLoadingBar(string message, int progress, int total = 100)
            {
                int width = 30;
                int bars = (int)((double)progress / total * width);
                
                string bar = "[" + new string('█', bars) + new string('░', width - bars) + "]";
                Console.Write($"\r{message} {bar} {progress}%");
                
                if (progress >= total)
                {
                    Console.WriteLine(); // New line when complete
                }
            }
    
            public void RenderVictoryScreen(Player winner)
            {
                Console.Clear();
                Console.WriteLine("\n\n");
                Console.WriteLine("    🎉 VICTORY! 🎉");
                Console.WriteLine("".PadRight(30, '═'));
                Console.WriteLine($"   Champion: {winner.PlayerName}");
                Console.WriteLine($"   Resources: {Currency.GetPlayerBalance(winner)}");
                Console.WriteLine("".PadRight(30, '═'));
                Console.WriteLine("\n\n");
            }
    
            public void RenderDefeatScreen()
            {
                Console.Clear();
                Console.WriteLine("\n\n");
                Console.WriteLine("    💀 DEFEAT 💀");
                Console.WriteLine("".PadRight(25, '═'));
                Console.WriteLine("   Your forces have been");
                Console.WriteLine("        overwhelmed");
                Console.WriteLine("".PadRight(25, '═'));
                Console.WriteLine("\n\n");
            }
        }
    }
}
