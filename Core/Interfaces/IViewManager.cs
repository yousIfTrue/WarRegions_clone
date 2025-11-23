using System;
using WarRegions.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Interfaces
{
    // Core/Interfaces/IViewManager.cs
    // Dependencies:
    // - Models/Region.cs
    // - Models/Army.cs
    
    namespace WarRegions.Interfaces
    {
        public interface IViewManager
        {
            /// <summary>
            /// Initialize the view manager and set up any required resources
            /// </summary>
            void Initialize();
            
            /// Update the game view with current game state
            void UpdateView();
            /// Display a specific region on the view
            /// <param name="region">The region to display</param>
            void ShowRegion(Region region);
            /// Display a specific army on the view
            /// <param name="army">The army to display</param>
            void ShowArmy(Army army);
            /// Clean up resources and shutdown the view manager
            void Cleanup();
            /// Gets whether this view manager uses 3D graphics
            bool Is3D { get; }
            /// Gets the display mode name
            string ViewMode { get; }
            /// Display a message to the player
            /// <param name="message">The message to display</param>
            /// <param name="messageType">Type of message (info, warning, error, etc.)</param>
            void ShowMessage(string message, MessageType messageType = MessageType.Info);
            /// Display the game map
            /// <param name="regions">All regions to display</param>
            /// <param name="armies">All armies to display</param>
            void DisplayMap(List<Region> regions, List<Army> armies);
            /// Display unit information
            /// <param name="unit">The unit to display</param>
            void DisplayUnitInfo(UnitCard unit);
            /// Display battle results
            /// <param name="result">The battle result to display</param>
            void DisplayBattleResult(BattleResult result);
            /// Display player statistics and resources
            /// <param name="player">The player to display</param>
            void DisplayPlayerStatus(Player player);
            /// Get player input for game commands
            /// <returns>The player's input command</returns>
            string GetPlayerInput();
            /// Display available commands to the player
            void ShowAvailableCommands();
            /// Clear the current display
            void ClearScreen();
            /// Display a loading screen or progress indicator
            /// <param name="message">Loading message to display</param>
            /// <param name="progress">Progress percentage (0-100)</param>
            void ShowLoadingScreen(string message, int progress = 0);
        }
        
        public enum MessageType
            Info,
            Warning,
            Error,
            Success,
            Battle,
            System
        public static class ViewManagerExtensions
            /// Display multiple messages at once
            public static void ShowMessages(this IViewManager viewManager, List<string> messages, MessageType messageType = MessageType.Info)
            {
                foreach (var message in messages)
                {
                    viewManager.ShowMessage(message, messageType);
                }
            }
            /// Display a formatted battle report
            public static void DisplayBattleReport(this IViewManager viewManager, BattleResult result, bool showDetails = true)
                var messages = new List<string>();
                
                messages.Add($"=== BATTLE REPORT ===");
                messages.Add($"Location: {result.BattleRegion.RegionName}");
                messages.Add($"Combatants: {result.Attacker.ArmyName} vs {result.Defender.ArmyName}");
                messages.Add($"Result: {(result.AttackerWon ? "ATTACKER VICTORY" : "DEFENDER VICTORY")}");
                if (showDetails)
                    messages.Add($"Casualties: {result.AttackerCasualties} (Attacker) / {result.DefenderCasualties} (Defender)");
                    messages.Add($"Rewards: {result.SilverReward} silver, {result.GoldReward} gold");
                viewManager.ShowMessages(messages, MessageType.Battle);
            /// Display a formatted unit card
            public static void DisplayUnitCard(this IViewManager viewManager, UnitCard unit)
                messages.Add($"=== {unit.UnitName} ===");
                messages.Add($"Level: {unit.Level} | Rarity: {unit.Rarity}");
                messages.Add($"Health: {unit.CurrentHealth}/{unit.Stats.MaxHealth}");
                messages.Add($"Attack: {unit.Stats.Attack} | Defense: {unit.Stats.Defense}");
                messages.Add($"Speed: {unit.Stats.Speed} | Range: {unit.Stats.Range}");
                messages.Add($"Movement: {unit.MovementType}");
                if (!string.IsNullOrEmpty(unit.Description))
                    messages.Add($"Description: {unit.Description}");
                viewManager.ShowMessages(messages, MessageType.Info);
            /// Display a quick status overview
            public static void DisplayQuickStatus(this IViewManager viewManager, Player player, GameState gameState)
                messages.Add($"=== QUICK STATUS ===");
                messages.Add($"Player: {player.PlayerName}");
                messages.Add($"Resources: {Currency.GetPlayerBalance(player)}");
                messages.Add($"Turn: {gameState.TurnNumber} | Phase: {gameState.CurrentPhase}");
                messages.Add($"Regions Controlled: {gameState.Regions.Count(r => r.Owner == player)}");
                messages.Add($"Total Armies: {gameState.Armies.Count(a => a.Owner == player)}");
            /// Display available movement options for an army
            public static void DisplayMovementOptions(this IViewManager viewManager, Army army, List<Region> reachableRegions)
                messages.Add($"=== MOVEMENT OPTIONS ===");
                messages.Add($"Army: {army.ArmyName}");
                messages.Add($"Movement Points: {army.MovementPoints}/{army.MaxMovementPoints}");
                messages.Add($"Current Position: {army.CurrentRegion.RegionName} ({army.CurrentRegion.X},{army.CurrentRegion.Y})");
                if (reachableRegions.Any())
                    messages.Add("Reachable Regions:");
                    foreach (var region in reachableRegions.Take(10)) // Limit display
                    {
                        string ownerInfo = region.Owner != null ? $"[{region.Owner.PlayerName}]" : "[Neutral]";
                        string armyInfo = region.OccupyingArmy != null ? $"[Occupied]" : "[Empty]";
                        messages.Add($"  {region.RegionName} ({region.X},{region.Y}) {ownerInfo} {armyInfo}");
                    }
                    
                    if (reachableRegions.Count > 10)
                        messages.Add($"  ... and {reachableRegions.Count - 10} more regions");
                else
                    messages.Add("No reachable regions - check movement points or terrain");
    }}
