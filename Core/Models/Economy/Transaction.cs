using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Models.Economy
{
    // Core/Models/Economy/Transaction.cs
    // Dependencies:
    // - Currency.cs (for currency amounts)
    // - Player.cs (for player reference)
    
    using System;
    
    namespace WarRegionsClone.Models.Economy
    {
        public class Transaction
        {
            public string TransactionId { get; set; }
            public string PlayerId { get; set; }
            public TransactionType Type { get; set; }
            
            // Currency amounts
            public int SilverAmount { get; set; }
            public int GoldAmount { get; set; }
            
            // Item information
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public int ItemQuantity { get; set; } = 1;
            
            // Transaction metadata
            public DateTime Timestamp { get; set; }
            public string Description { get; set; }
            public bool IsSuccessful { get; set; }
            public string ErrorMessage { get; set; }
            
            // Balance before and after
            public int SilverBalanceBefore { get; set; }
            public int GoldBalanceBefore { get; set; }
            public int SilverBalanceAfter { get; set; }
            public int GoldBalanceAfter { get; set; }
            
            public Transaction()
            {
                TransactionId = Guid.NewGuid().ToString();
                Timestamp = DateTime.Now;
                IsSuccessful = false;
            }
            
            public Transaction(string playerId, TransactionType type, int silverAmount, int goldAmount = 0) : this()
            {
                PlayerId = playerId;
                Type = type;
                SilverAmount = silverAmount;
                GoldAmount = goldAmount;
            }
            
            public static Transaction CreatePurchase(string playerId, string itemName, int silverCost, int goldCost = 0)
            {
                return new Transaction(playerId, TransactionType.Purchase, silverCost, goldCost)
                {
                    ItemName = itemName,
                    Description = $"Purchased {itemName}",
                    SilverAmount = silverCost,
                    GoldAmount = goldCost
                };
            }
            
            public static Transaction CreateSale(string playerId, string itemName, int silverGained, int goldGained = 0)
            {
                return new Transaction(playerId, TransactionType.Sale, silverGained, goldGained)
                {
                    ItemName = itemName,
                    Description = $"Sold {itemName}",
                    SilverAmount = silverGained,
                    GoldAmount = goldGained
                };
            }
            
            public static Transaction CreateReward(string playerId, string reason, int silverAmount, int goldAmount = 0)
            {
                return new Transaction(playerId, TransactionType.Reward, silverAmount, goldAmount)
                {
                    Description = reason,
                    SilverAmount = silverAmount,
                    GoldAmount = goldAmount
                };
            }
            
            public static Transaction CreateUpkeep(string playerId, int silverCost, string reason = "Unit upkeep")
            {
                return new Transaction(playerId, TransactionType.Upkeep, silverCost)
                {
                    Description = reason,
                    SilverAmount = silverCost
                };
            }
            
            public void SetBalances(int silverBefore, int goldBefore, int silverAfter, int goldAfter)
            {
                SilverBalanceBefore = silverBefore;
                GoldBalanceBefore = goldBefore;
                SilverBalanceAfter = silverAfter;
                GoldBalanceAfter = goldAfter;
            }
            
            public void MarkAsSuccessful()
            {
                IsSuccessful = true;
                ErrorMessage = string.Empty;
            }
            
            public void MarkAsFailed(string error)
            {
                IsSuccessful = false;
                ErrorMessage = error;
            }
            
            public int GetNetSilver()
            {
                return Type switch
                {
                    TransactionType.Purchase or TransactionType.Upkeep => -SilverAmount,
                    TransactionType.Sale or TransactionType.Reward or TransactionType.Income => SilverAmount,
                    _ => 0
                };
            }
            
            public int GetNetGold()
            {
                return Type switch
                {
                    TransactionType.Purchase or TransactionType.Upkeep => -GoldAmount,
                    TransactionType.Sale or TransactionType.Reward or TransactionType.Income => GoldAmount,
                    _ => 0
                };
            }
            
            public string GetFormattedAmount()
            {
                string silver = SilverAmount != 0 ? $"{Math.Abs(SilverAmount)} silver" : "";
                string gold = GoldAmount != 0 ? $"{Math.Abs(GoldAmount)} gold" : "";
                
                if (!string.IsNullOrEmpty(silver) && !string.IsNullOrEmpty(gold))
                    return $"{silver}, {gold}";
                else if (!string.IsNullOrEmpty(silver))
                    return silver;
                else if (!string.IsNullOrEmpty(gold))
                    return gold;
                else
                    return "No currency";
            }
            
            public string GetFormattedNetAmount()
            {
                int netSilver = GetNetSilver();
                int netGold = GetNetGold();
                
                string silver = netSilver != 0 ? $"{netSilver} silver" : "";
                string gold = netGold != 0 ? $"{netGold} gold" : "";
                
                if (!string.IsNullOrEmpty(silver) && !string.IsNullOrEmpty(gold))
                    return $"{silver}, {gold}";
                else if (!string.IsNullOrEmpty(silver))
                    return silver;
                else if (!string.IsNullOrEmpty(gold))
                    return gold;
                else
                    return "No change";
            }
            
            public override string ToString()
            {
                string status = IsSuccessful ? "✅" : "❌";
                string amount = GetFormattedAmount();
                string netAmount = GetFormattedNetAmount();
                
                return $"{status} {Timestamp:HH:mm} - {Type}: {Description} - {amount} (Net: {netAmount})";
            }
            
            public string GetDetailedReport()
            {
                return $"""
                Transaction: {TransactionId}
                Player: {PlayerId}
                Type: {Type}
                Time: {Timestamp:yyyy-MM-dd HH:mm:ss}
                Description: {Description}
                Amount: {GetFormattedAmount()}
                Net Change: {GetFormattedNetAmount()}
                Status: {(IsSuccessful ? "Success" : "Failed")}
                {(IsSuccessful ? "" : $"Error: {ErrorMessage}")}
                Balance: {SilverBalanceBefore}S {GoldBalanceBefore}G → {SilverBalanceAfter}S {GoldBalanceAfter}G
                """;
            }
            
            public bool IsRecent(TimeSpan timeSpan)
            {
                return DateTime.Now - Timestamp <= timeSpan;
            }
            
            public static void PrintTransactionHistory(List<Transaction> transactions, int maxToShow = 10)
            {
                if (transactions == null || transactions.Count == 0)
                {
                    Console.WriteLine("No transactions found.");
                    return;
                }
                
                var recentTransactions = transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Take(maxToShow);
                
                Console.WriteLine("=== Recent Transactions ===");
                foreach (var transaction in recentTransactions)
                {
                    Console.WriteLine(transaction.ToString());
                }
                Console.WriteLine("===========================");
            }
        }
        
        public enum TransactionType
        {
            Purchase,   // Buying items/units
            Sale,       // Selling items/units  
            Reward,     // Battle rewards, achievements
            Income,     // Daily income, region production
            Upkeep,     // Unit maintenance costs
            Conversion, // Currency exchange
            Refund,     // Returned funds
            Penalty     // Fees, penalties
        }
        
        public static class TransactionManager
        {
            private static List<Transaction> _transactionHistory = new List<Transaction>();
            
            public static void RecordTransaction(Transaction transaction)
            {
                _transactionHistory.Add(transaction);
                
                if (transaction.IsSuccessful)
                {
                    Console.WriteLine($"[TRANSACTION] Recorded: {transaction.Description}");
                }
                else
                {
                    Console.WriteLine($"[TRANSACTION] Failed: {transaction.Description} - {transaction.ErrorMessage}");
                }
            }
            
            public static List<Transaction> GetPlayerTransactions(string playerId, int maxCount = 50)
            {
                return _transactionHistory
                    .Where(t => t.PlayerId == playerId)
                    .OrderByDescending(t => t.Timestamp)
                    .Take(maxCount)
                    .ToList();
            }
            
            public static List<Transaction> GetRecentTransactions(TimeSpan timeSpan)
            {
                return _transactionHistory
                    .Where(t => t.IsRecent(timeSpan))
                    .OrderByDescending(t => t.Timestamp)
                    .ToList();
            }
            
            public static int GetTotalSilverSpent(string playerId, TransactionType? type = null)
            {
                var query = _transactionHistory.Where(t => t.PlayerId == playerId && t.GetNetSilver() < 0);
                
                if (type.HasValue)
                    query = query.Where(t => t.Type == type.Value);
                    
                return Math.Abs(query.Sum(t => t.GetNetSilver()));
            }
            
            public static int GetTotalSilverEarned(string playerId, TransactionType? type = null)
            {
                var query = _transactionHistory.Where(t => t.PlayerId == playerId && t.GetNetSilver() > 0);
                
                if (type.HasValue)
                    query = query.Where(t => t.Type == type.Value);
                    
                return query.Sum(t => t.GetNetSilver());
            }
            
            public static void ClearHistory()
            {
                _transactionHistory.Clear();
                Console.WriteLine("[TRANSACTION] Transaction history cleared.");
            }
            
            public static string GetFinancialReport(string playerId)
            {
                var playerTransactions = GetPlayerTransactions(playerId);
                
                int totalEarned = GetTotalSilverEarned(playerId);
                int totalSpent = GetTotalSilverSpent(playerId);
                int netProfit = totalEarned - totalSpent;
                
                return $"""
                Financial Report for Player {playerId}:
                Total Silver Earned: {totalEarned}
                Total Silver Spent: {totalSpent}
                Net Profit: {netProfit}
                Transaction Count: {playerTransactions.Count}
                Success Rate: {playerTransactions.Count(t => t.IsSuccessful)}/{playerTransactions.Count}
                """;
            }
        }
    }}
