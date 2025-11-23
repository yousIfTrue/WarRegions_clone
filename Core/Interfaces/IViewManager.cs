// Core/Interfaces/IViewManager.cs
// Dependencies:
// - Models/Region.cs
// - Models/Army.cs

namespace WarRegionsClone.Interfaces
{
    public interface IViewManager
    {
        /// <summary>
        /// Initialize the view manager and set up any required resources
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Update the game view with current game state
        /// </summary>
        void UpdateView();
        
        /// <summary>
        /// Display a specific region on the view
        /// </summary>
        /// <param name="region">The region to display</param>
        void ShowRegion(Region region);
        
        /// <summary>
        /// Display a specific army on the view
        /// </summary>
        /// <param name="army">The army to display</param>
        void ShowArmy(Army army);
        
        /// <summary>
        /// Clean up resources and shutdown the view manager
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Gets whether this view manager uses 3D graphics
        /// </summary>
        bool Is3D { get; }
        
        /// <summary>
        /// Gets the display mode name
        /// </summary>
        string ViewMode { get; }
        
        /// <summary>
        /// Display a message to the player
        /// </summary>
        /// <param name="message">The message to display</param>
        /// <param name="messageType">Type of message (info, warning, error, etc.)</param>
        void ShowMessage(string message, MessageType messageType = MessageType.Info);
        
        /// <summary>
        /// Display the game map
        /// </summary>
        /// <param name="regions">All regions to display</param>
        /// <param name="armies">All armies to display</param>
        void DisplayMap(List<Region> regions, List<Army> armies);
        
        /// <summary>
        /// Display unit information
        /// </summary>
        /// <param name="unit">The unit to display</param>
        void DisplayUnitInfo(UnitCard unit);
        
        /// <summary>
        /// Display battle results
        /// </summary>
        /// <param name="result">The battle result to display</param>
        void DisplayBattleResult(BattleResult result);
        
        /// <summary>
        /// Display player statistics and resources
        /// </summary>
        /// <param name="player">The player to display</param>
        void DisplayPlayerStatus(Player player);
        
        /// <summary>
        /// Get player input for game commands
        /// </summary>
        /// <returns>The player's input command</returns>
        string GetPlayerInput();
        
        /// <summary>
        /// Display available commands to the player
        /// </summary>
        void ShowAvailableCommands();
        
        /// <summary>
        /// Clear the current display
        /// </summary>
        void ClearScreen();
        
        /// <summary>
        /// Display a loading screen or progress indicator
        /// </summary>
        /// <param name="message">Loading message to display</param>
        /// <param name="progress">Progress percentage (0-100)</param>
        void ShowLoadingScreen(string message, int progress = 0);
    }
    
    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Success,
        Battle,
        System
    }
    
    public static class ViewManagerExtensions
    {
        /// <summary>
        /// Display multiple messages at once
        /// </summary>
        public static void ShowMessages(this IViewManager viewManager, List<string> messages, MessageType messageType = MessageType.Info)
        {
            foreach (var message in messages)
            {
                viewManager.ShowMessage(message, messageType);
            }
        }
        
        /// <summary>
        /// Display a formatted battle report
        /// </summary>
        public static void DisplayBattleReport(this IViewManager viewManager, BattleResult result, bool showDetails = true)
        {
            var messages = new List<string>();
            
            messages.Add($"=== BATTLE REPORT ===");
            messages.Add($"Location: {result.BattleRegion.RegionName}");
            messages.Add($"Combatants: {result.Attacker.ArmyName} vs {result.Defender.ArmyName}");
            messages.Add($"Result: {(result.AttackerWon ? "ATTACKER VICTORY" : "DEFENDER VICTORY")}");
            
            if (showDetails)
            {
                messages.Add($"Casualties: {result.AttackerCasualties} (Attacker) / {result.DefenderCasualties} (Defender)");
                messages.Add($"Rewards: {result.SilverReward} silver, {result.GoldReward} gold");
            }
            
            viewManager.ShowMessages(messages, MessageType.Battle);
        }
        
        /// <summary>
        /// Display a formatted unit card
        /// </summary>
        public static void DisplayUnitCard(this IViewManager viewManager, UnitCard unit)
        {
            var messages = new List<string>();
            
            messages.Add($"=== {unit.UnitName} ===");
            messages.Add($"Level: {unit.Level} | Rarity: {unit.Rarity}");
            messages.Add($"Health: {unit.CurrentHealth}/{unit.Stats.MaxHealth}");
            messages.Add($"Attack: {unit.Stats.Attack} | Defense: {unit.Stats.Defense}");
            messages.Add($"Speed: {unit.Stats.Speed} | Range: {unit.Stats.Range}");
            messages.Add($"Movement: {unit.MovementType}");
            
            if (!string.IsNullOrEmpty(unit.Description))
            {
                messages.Add($"Description: {unit.Description}");
            }
            
            viewManager.ShowMessages(messages, MessageType.Info);
        }
        
        /// <summary>
        /// Display a quick status overview
        /// </summary>
        public static void DisplayQuickStatus(this IViewManager viewManager, Player player, GameState gameState)
        {
            var messages = new List<string>();
            
            messages.Add($"=== QUICK STATUS ===");
            messages.Add($"Player: {player.PlayerName}");
            messages.Add($"Resources: {Currency.GetPlayerBalance(player)}");
            messages.Add($"Turn: {gameState.TurnNumber} | Phase: {gameState.CurrentPhase}");
            messages.Add($"Regions Controlled: {gameState.Regions.Count(r => r.Owner == player)}");
            messages.Add($"Total Armies: {gameState.Armies.Count(a => a.Owner == player)}");
            
            viewManager.ShowMessages(messages, MessageType.Info);
        }
        
        /// <summary>
        /// Display available movement options for an army
        /// </summary>
        public static void DisplayMovementOptions(this IViewManager viewManager, Army army, List<Region> reachableRegions)
        {
            var messages = new List<string>();
            
            messages.Add($"=== MOVEMENT OPTIONS ===");
            messages.Add($"Army: {army.ArmyName}");
            messages.Add($"Movement Points: {army.MovementPoints}/{army.MaxMovementPoints}");
            messages.Add($"Current Position: {army.CurrentRegion.RegionName} ({army.CurrentRegion.X},{army.CurrentRegion.Y})");
            
            if (reachableRegions.Any())
            {
                messages.Add("Reachable Regions:");
                foreach (var region in reachableRegions.Take(10)) // Limit display
                {
                    string ownerInfo = region.Owner != null ? $"[{region.Owner.PlayerName}]" : "[Neutral]";
                    string armyInfo = region.OccupyingArmy != null ? $"[Occupied]" : "[Empty]";
                    messages.Add($"  {region.RegionName} ({region.X},{region.Y}) {ownerInfo} {armyInfo}");
                }
                
                if (reachableRegions.Count > 10)
                {
                    messages.Add($"  ... and {reachableRegions.Count - 10} more regions");
                }
            }
            else
            {
                messages.Add("No reachable regions - check movement points or terrain");
            }
            
            viewManager.ShowMessages(messages, MessageType.Info);
        }
    }
}