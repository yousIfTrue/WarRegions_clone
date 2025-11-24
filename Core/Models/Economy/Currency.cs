using System;
using System.Collections.Generic;
using System.Linq;
    
    namespace WarRegions.Models.Economy
    {
        public static class Currency
        {
            // Currency types
            public const string SILVER = "Silver";
            public const string GOLD = "Gold";
            
            // Exchange rates
            public const int SilverToGoldRate = 10;
            public const int GoldToSilverRate = 10;
            
            // Income formulas
            public static int CalculateBaseIncome(int regionsOwned, int playerLevel)
            {
                int baseIncome = 50 + (regionsOwned * 5) + (playerLevel * 2);
                return baseIncome;
            }
            
            public static int CalculateBattleReward(int enemyPower, int playerLevel, double victoryMultiplier = 1.0)
            {
                int baseReward = enemyPower / 2;
                int levelBonus = playerLevel * 5;
                int total = (int)((baseReward + levelBonus) * victoryMultiplier);
                return Math.Max(10, total); // Minimum reward
            }
            
            public static int CalculateUnitUpkeep(List<UnitCard> units)
            {
                int totalUpkeep = 0;
                foreach (var unit in units)
                {
                    totalUpkeep += unit.Stats.UpkeepCost;
                }
                return totalUpkeep;
            }
            
            public static bool CanAfford(Player player, int silverCost, int goldCost = 0)
            {
                return player.SilverCoins >= silverCost && player.GoldCoins >= goldCost;
            }
            
            public static TransactionResult SpendCurrency(Player player, int silverCost, int goldCost = 0, string reason = "")
            {
                if (!CanAfford(player, silverCost, goldCost))
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Insufficient funds! Need {silverCost} silver and {goldCost} gold."
                    };
                }
                
                player.SilverCoins -= silverCost;
                player.GoldCoins -= goldCost;
                
                string message = $"Spent {silverCost} silver";
                if (goldCost > 0) message += $" and {goldCost} gold";
                if (!string.IsNullOrEmpty(reason)) message += $" for {reason}";
                message += $". Remaining: {player.SilverCoins}S {player.GoldCoins}G";
                
                Console.WriteLine($"[ECONOMY] {message}");
                
                return new TransactionResult
                {
                    Success = true,
                    Message = message,
                    SilverSpent = silverCost,
                    GoldSpent = goldCost
                };
            }
            
            public static void AddCurrency(Player player, int silverAmount, int goldAmount = 0, string reason = "")
            {
                player.SilverCoins += silverAmount;
                player.GoldCoins += goldAmount;
                
                string message = $"Received {silverAmount} silver";
                if (goldAmount > 0) message += $" and {goldAmount} gold";
                if (!string.IsNullOrEmpty(reason)) message += $" from {reason}";
                message += $". Total: {player.SilverCoins}S {player.GoldCoins}G";
                
                Console.WriteLine($"[ECONOMY] {message}");
            }
            
            public static TransactionResult ConvertSilverToGold(Player player, int silverAmount)
            {
                if (silverAmount < SilverToGoldRate)
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Need at least {SilverToGoldRate} silver to convert to gold."
                    };
                }
                
                if (!CanAfford(player, silverAmount, 0))
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Not enough silver! Have {player.SilverCoins}, need {silverAmount}."
                    };
                }
                
                int goldGained = silverAmount / SilverToGoldRate;
                int silverRemainder = silverAmount % SilverToGoldRate;
                
                player.SilverCoins -= silverAmount;
                player.GoldCoins += goldGained;
                
                string message = $"Converted {silverAmount} silver to {goldGained} gold";
                if (silverRemainder > 0) message += $" with {silverRemainder} silver change";
                
                Console.WriteLine($"[ECONOMY] {message}");
                
                return new TransactionResult
                {
                    Success = true,
                    Message = message,
                    SilverSpent = silverAmount - silverRemainder,
                    GoldGained = goldGained
                };
            }
            
            public static TransactionResult ConvertGoldToSilver(Player player, int goldAmount)
            {
                if (!CanAfford(player, 0, goldAmount))
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Not enough gold! Have {player.GoldCoins}, need {goldAmount}."
                    };
                }
                
                int silverGained = goldAmount * GoldToSilverRate;
                
                player.GoldCoins -= goldAmount;
                player.SilverCoins += silverGained;
                
                string message = $"Converted {goldAmount} gold to {silverGained} silver";
                
                Console.WriteLine($"[ECONOMY] {message}");
                
                return new TransactionResult
                {
                    Success = true,
                    Message = message,
                    GoldSpent = goldAmount,
                    SilverGained = silverGained
                };
            }
            
            public static string FormatCurrency(int silver, int gold = 0)
            {
                if (gold > 0)
                    return $"{silver}🪙 {gold}💰";
                else
                    return $"{silver}🪙";
            }
            
            public static string GetPlayerBalance(Player player)
            {
                return FormatCurrency(player.SilverCoins, player.GoldCoins);
            }
            
            public static void ApplyDailyIncome(Player player, int regionsControlled)
            {
                int baseIncome = CalculateBaseIncome(regionsControlled, player.LevelProgress);
                int upkeepCost = CalculateUnitUpkeep(player.AvailableUnits);
                int netIncome = baseIncome - upkeepCost;
                
                if (netIncome > 0)
                {
                    AddCurrency(player, netIncome, 0, "daily income");
                }
                else
                {
                    // If upkeep exceeds income, still pay but show warning
                    SpendCurrency(player, Math.Abs(netIncome), 0, "unit upkeep");
                    Console.WriteLine($"[WARNING] Upkeep costs exceed income! Lost {Math.Abs(netIncome)} silver.");
                }
            }
        }
        
        public class TransactionResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public int SilverSpent { get; set; }
            public int GoldSpent { get; set; }
            public int SilverGained { get; set; }
            public int GoldGained { get; set; }
            
            public TransactionResult()
            {
                Success = false;
                Message = string.Empty;
            }
        }
    }
