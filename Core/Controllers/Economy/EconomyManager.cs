using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Controllers.Economy
{
    // Core/Controllers/Economy/EconomyManager.cs
    // Dependencies:
    // - Models/Economy/Currency.cs
    // - Models/Economy/Transaction.cs
    // - Models/Player.cs
    // - Models/GameState.cs
    // - Development/DevConfig.cs
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace WarRegonsClone.Controllers.Economy
    {
        public class EconomyManager
        {
            private List<Player> _trackedPlayers;
            private Dictionary<string, List<Transaction>> _playerTransactions;
            
            public EconomyManager()
            {
                _trackedPlayers = new List<Player>();
                _playerTransactions = new Dictionary<string, List<Transaction>>();
                Console.WriteLine("[ECONOMY] EconomyManager initialized");
            }
            
            public void TrackPlayer(Player player)
            {
                if (!_trackedPlayers.Contains(player))
                {
                    _trackedPlayers.Add(player);
                    _playerTransactions[player.PlayerId] = new List<Transaction>();
                    Console.WriteLine($"[ECONOMY] Now tracking player: {player.PlayerName}");
                }
            }
            
            public void StopTrackingPlayer(Player player)
            {
                if (_trackedPlayers.Contains(player))
                {
                    _trackedPlayers.Remove(player);
                    _playerTransactions.Remove(player.PlayerId);
                    Console.WriteLine($"[ECONOMY] Stopped tracking player: {player.PlayerName}");
                }
            }
            
            public void ProcessDailyEconomy(GameState gameState)
            {
                if (gameState == null) return;
                
                Console.WriteLine($"[ECONOMY] Processing daily economy for turn {gameState.TurnNumber}");
                
                foreach (var player in gameState.Players)
                {
                    ProcessPlayerEconomy(player, gameState);
                }
            }
            
            private void ProcessPlayerEconomy(Player player, GameState gameState)
            {
                // Calculate regions controlled by player
                int regionsControlled = gameState.Regions.Count(r => r.Owner == player);
                
                // Apply daily income
                Currency.ApplyDailyIncome(player, regionsControlled);
                
                // Record income transaction
                var incomeTransaction = Transaction.CreateIncome(
                    player.PlayerId,
                    "Daily region income",
                    Currency.CalculateBaseIncome(regionsControlled, player.LevelProgress),
                    0
                );
                incomeTransaction.MarkAsSuccessful();
                RecordTransaction(incomeTransaction);
                
                // Process unit upkeep
                int upkeepCost = Currency.CalculateUnitUpkeep(player.AvailableUnits);
                if (upkeepCost > 0)
                {
                    var upkeepTransaction = Transaction.CreateUpkeep(player.PlayerId, upkeepCost);
                    
                    if (Currency.CanAfford(player, upkeepCost, 0))
                    {
                        Currency.SpendCurrency(player, upkeepCost, 0, "unit upkeep");
                        upkeepTransaction.MarkAsSuccessful();
                    }
                    else
                    {
                        upkeepTransaction.MarkAsFailed("Insufficient funds for upkeep");
                        Console.WriteLine($"[ECONOMY WARNING] {player.PlayerName} cannot afford upkeep costs!");
                    }
                    
                    RecordTransaction(upkeepTransaction);
                }
                
                Console.WriteLine($"[ECONOMY] Processed economy for {player.PlayerName}: {regionsControlled} regions, {upkeepCost} upkeep");
            }
            
            public void AwardBattleRewards(Player winner, Player loser, int battleIntensity)
            {
                if (winner == null || loser == null) return;
                
                int silverReward = battleIntensity * 10;
                int goldReward = battleIntensity / 10;
                
                Currency.AddCurrency(winner, silverReward, goldReward, "battle victory");
                
                var rewardTransaction = Transaction.CreateReward(
                    winner.PlayerId,
                    $"Victory over {loser.PlayerName}",
                    silverReward,
                    goldReward
                );
                rewardTransaction.MarkAsSuccessful();
                RecordTransaction(rewardTransaction);
                
                Console.WriteLine($"[ECONOMY] Awarded battle rewards to {winner.PlayerName}: {silverReward} silver, {goldReward} gold");
            }
            
            public void AwardLevelCompletion(Player player, LevelData level, int completionSpeed)
            {
                if (player == null || level == null) return;
                
                // Base rewards
                int silverReward = level.SilverReward;
                int goldReward = level.GoldReward;
                
                // Speed bonus
                double speedMultiplier = Math.Max(0.5, 2.0 - (completionSpeed / (double)level.TurnsLimit));
                silverReward = (int)(silverReward * speedMultiplier);
                goldReward = (int)(goldReward * speedMultiplier);
                
                Currency.AddCurrency(player, silverReward, goldReward, $"level completion: {level.LevelName}");
                
                var rewardTransaction = Transaction.CreateReward(
                    player.PlayerId,
                    $"Completed {level.LevelName}",
                    silverReward,
                    goldReward
                );
                rewardTransaction.MarkAsSuccessful();
                RecordTransaction(rewardTransaction);
                
                Console.WriteLine($"[ECONOMY] Awarded level completion to {player.PlayerName}: {silverReward} silver, {goldReward} gold");
            }
            
            public TransactionResult ProcessPurchase(Player player, string itemName, int silverCost, int goldCost = 0)
            {
                var transaction = Currency.SpendCurrency(player, silverCost, goldCost, itemName);
                
                var purchaseTransaction = Transaction.CreatePurchase(
                    player.PlayerId,
                    itemName,
                    silverCost,
                    goldCost
                );
                
                if (transaction.Success)
                {
                    purchaseTransaction.MarkAsSuccessful();
                }
                else
                {
                    purchaseTransaction.MarkAsFailed(transaction.Message);
                }
                
                RecordTransaction(purchaseTransaction);
                return transaction;
            }
            
            public TransactionResult ProcessSale(Player player, string itemName, int silverGained, int goldGained = 0)
            {
                Currency.AddCurrency(player, silverGained, goldGained, $"sale of {itemName}");
                
                var saleTransaction = Transaction.CreateSale(
                    player.PlayerId,
                    itemName,
                    silverGained,
                    goldGained
                );
                saleTransaction.MarkAsSuccessful();
                RecordTransaction(saleTransaction);
                
                return new TransactionResult
                {
                    Success = true,
                    Message = $"Sold {itemName} for {silverGained} silver and {goldGained} gold"
                };
            }
            
            public void RecordTransaction(Transaction transaction)
            {
                if (!_playerTransactions.ContainsKey(transaction.PlayerId))
                {
                    _playerTransactions[transaction.PlayerId] = new List<Transaction>();
                }
                
                _playerTransactions[transaction.PlayerId].Add(transaction);
                TransactionManager.RecordTransaction(transaction);
            }
            
            public List<Transaction> GetPlayerTransactionHistory(string playerId, int maxCount = 50)
            {
                if (_playerTransactions.ContainsKey(playerId))
                {
                    return _playerTransactions[playerId]
                        .OrderByDescending(t => t.Timestamp)
                        .Take(maxCount)
                        .ToList();
                }
                
                return new List<Transaction>();
            }
            
            public string GetPlayerEconomyReport(string playerId)
            {
                var player = _trackedPlayers.FirstOrDefault(p => p.PlayerId == playerId);
                if (player == null) return "Player not found";
                
                var transactions = GetPlayerTransactionHistory(playerId, 100);
                var recentTransactions = transactions.Where(t => t.IsRecent(TimeSpan.FromDays(7))).ToList();
                
                int weeklyIncome = recentTransactions
                    .Where(t => t.GetNetSilver() > 0)
                    .Sum(t => t.GetNetSilver());
                    
                int weeklyExpenses = Math.Abs(recentTransactions
                    .Where(t => t.GetNetSilver() < 0)
                    .Sum(t => t.GetNetSilver()));
                
                int netWeekly = weeklyIncome - weeklyExpenses;
                
                return $"""
                Economy Report for {player.PlayerName}
                Current Balance: {Currency.GetPlayerBalance(player)}
                
                Weekly Summary (Last 7 days):
                Income: {weeklyIncome} silver
                Expenses: {weeklyExpenses} silver  
                Net: {netWeekly} silver
                
                Recent Transactions: {recentTransactions.Count}
                Total Transactions: {transactions.Count}
                
                Financial Health: {GetFinancialHealthRating(netWeekly)}
                """;
            }
            
            private string GetFinancialHealthRating(int netWeekly)
            {
                return netWeekly switch
                {
                    > 1000 => "Excellent 💰",
                    > 500 => "Good 💵",
                    > 100 => "Stable 📈",
                    > 0 => "Breaking Even ⚖️",
                    0 => "Balanced ⚖️",
                    < 0 => "Deficit 📉",
                    _ => "Unknown ❓"
                };
            }
            
            public void ApplyInflation(int turnNumber)
            {
                // Simulate economic inflation over time
                double inflationRate = 1.0 + (turnNumber * 0.01); // 1% inflation per turn
                
                foreach (var player in _trackedPlayers)
                {
                    // Adjust player's perception of costs (in real game, adjust actual prices)
                    Console.WriteLine($"[ECONOMY] Inflation rate at turn {turnNumber}: {inflationRate:P2}");
                }
            }
            
            public bool CanPlayerAffordGameplay(Player player, GameState gameState)
            {
                // Check if player can afford basic gameplay (unit upkeep, etc.)
                int upcomingUpkeep = Currency.CalculateUnitUpkeep(player.AvailableUnits);
                int regionsControlled = gameState?.Regions.Count(r => r.Owner == player) ?? 0;
                int estimatedIncome = Currency.CalculateBaseIncome(regionsControlled, player.LevelProgress);
                
                return player.SilverCoins + estimatedIncome >= upcomingUpkeep;
            }
            
            public void ProvideEconomicAssistance(Player player, string reason)
            {
                if (DevConfig.InfiniteResources)
                {
                    // In development mode, provide generous assistance
                    Currency.AddCurrency(player, 1000, 100, $"economic assistance: {reason}");
                    Console.WriteLine($"[ECONOMY] Provided development assistance to {player.PlayerName}");
                }
                else
                {
                    // In normal mode, provide minimal assistance
                    Currency.AddCurrency(player, 200, 20, $"economic assistance: {reason}");
                    Console.WriteLine($"[ECONOMY] Provided economic assistance to {player.PlayerName}");
                }
            }
            
            public string GetGlobalEconomyStatus()
            {
                var report = new System.Text.StringBuilder();
                report.AppendLine("Global Economy Status:");
                report.AppendLine($"Tracked Players: {_trackedPlayers.Count}");
                
                foreach (var player in _trackedPlayers)
                {
                    var transactions = GetPlayerTransactionHistory(player.PlayerId, 10);
                    int recentIncome = transactions.Where(t => t.GetNetSilver() > 0).Sum(t => t.GetNetSilver());
                    int recentExpenses = Math.Abs(transactions.Where(t => t.GetNetSilver() < 0).Sum(t => t.GetNetSilver()));
                    
                    report.AppendLine($"""
                    {player.PlayerName}:
                      Balance: {Currency.GetPlayerBalance(player)}
                      Recent Activity: +{recentIncome}/-{recentExpenses} silver
                    """);
                }
                
                return report.ToString();
            }
            
            public void ResetPlayerEconomy(Player player)
            {
                if (_playerTransactions.ContainsKey(player.PlayerId))
                {
                    _playerTransactions[player.PlayerId].Clear();
                }
                
                // Reset to starting values
                player.SilverCoins = DevConfig.StartingSilver;
                player.GoldCoins = DevConfig.StartingGold;
                
                Console.WriteLine($"[ECONOMY] Reset economy for {player.PlayerName}");
            }
        }
    }}
