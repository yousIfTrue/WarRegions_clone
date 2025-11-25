
    namespace WarRegions.Core.Models.Economy
    {
            // Core/Models/Economy/UpgradeCost.cs
    // Dependencies:
    // - Units/UnitRarity.cs (for rarity-based costs)
    // - Currency.cs (for currency calculations)
        public class UpgradeCost
        {
            public string UpgradeId { get; set; }
            public string UpgradeName { get; set; }
            public UpgradeType Type { get; set; }
            
            // Cost in currency
            public int SilverCost { get; set; }
            public int GoldCost { get; set; }
            
            // Level-based scaling
            public int BaseLevel { get; set; } = 1;
            public double CostMultiplierPerLevel { get; set; } = 1.5;
            
            // Time requirements
            public int RequiredTimeMinutes { get; set; }
            public bool CanInstantComplete { get; set; }
            public int InstantCompleteGoldCost { get; set; }
            
            // Requirements
            public int RequiredPlayerLevel { get; set; } = 1;
            public List<string> RequiredItems { get; set; }
            public string RequiredBuilding { get; set; }
            
            // ✅ إصلاح طرق الحساب مع التحويل الصحيح
            public int GetSilverCost(int currentLevel = 1, int targetLevel = 2)
            {
                double baseCost = 100 * Math.Pow(1.5, currentLevel - 1);
                double totalCost = 0;
            
                for (int level = currentLevel; level < targetLevel; level++)
                {
                    totalCost += baseCost * Math.Pow(1.5, level - currentLevel);
                }
            
                return (int)totalCost; // ✅ تحويل صريح
            }
        
            public int GetGoldCost(int currentLevel = 1, int targetLevel = 2)
            {
                double baseCost = 10 * Math.Pow(1.3, currentLevel - 1);
                double totalCost = 0;
            
                for (int level = currentLevel; level < targetLevel; level++)
                {
                    totalCost += baseCost * Math.Pow(1.3, level - currentLevel);
                }
            
                return (int)Math.Ceiling(totalCost); // ✅ تحويل صريح مع تقريب
            }
        
            public int GetTotalTimeSeconds(int currentLevel = 1, int targetLevel = 2)
            {
                double baseTime = 60 * Math.Pow(1.2, currentLevel - 1);
                double totalTime = 0;
            
                for (int level = currentLevel; level < targetLevel; level++)
                {
                    totalTime += baseTime * Math.Pow(1.2, level - currentLevel);
                }
            
                return (int)Math.Ceiling(totalTime); // ✅ تحويل صريح مع تقريب
            }
        
            // Results
            public int StatIncrease { get; set; }
            public string NewAbility { get; set; }
            public string Description { get; set; }
            
            public UpgradeCost()
            {
                UpgradeId = Guid.NewGuid().ToString();
                RequiredItems = new List<string>();
            }
            
            public UpgradeCost(string name, UpgradeType type, int silverCost, int goldCost = 0) : this()
            {
                UpgradeName = name;
                Type = type;
                SilverCost = silverCost;
                GoldCost = goldCost;
            }
            
            public int CalculateCostForLevel(int currentLevel, int targetLevel)
            {
                if (targetLevel <= currentLevel)
                    return 0;
                    
                int levelsToUpgrade = targetLevel - currentLevel;
                double multiplier = Math.Pow(CostMultiplierPerLevel, levelsToUpgrade - 1);
                
                int totalSilver = (int)(SilverCost * multiplier * levelsToUpgrade);
                int totalGold = (int)(GoldCost * multiplier * levelsToUpgrade);
                
                // Apply development config multiplier if in debug mode
                if (DevConfig.DebugMode)
                {
                    totalSilver *= DevConfig.UnitUpgradeCostMultiplier;
                    totalGold *= DevConfig.UnitUpgradeCostMultiplier;
                }
                
                return totalSilver + (totalGold * Currency.SilverToGoldRate);
            }
            
            public (int silver, int gold) CalculateCurrencyCostForLevel(int currentLevel, int targetLevel)
            {
                if (targetLevel <= currentLevel)
                    return (0, 0);
                    
                int levelsToUpgrade = targetLevel - currentLevel;
                double multiplier = Math.Pow(CostMultiplierPerLevel, levelsToUpgrade - 1);
                
                int totalSilver = (int)(SilverCost * multiplier * levelsToUpgrade);
                int totalGold = (int)(GoldCost * multiplier * levelsToUpgrade);
                
                // Apply development config multiplier if in debug mode
                if (DevConfig.DebugMode)
                {
                    totalSilver *= DevConfig.UnitUpgradeCostMultiplier;
                    totalGold *= DevConfig.UnitUpgradeCostMultiplier;
                }
                
                return (totalSilver, totalGold);
            }
            
            public static UpgradeCost CreateUnitUpgradeCost(UnitRarity rarity, int currentLevel)
            {
                var cost = new UpgradeCost
                {
                    Type = UpgradeType.Unit,
                    BaseLevel = currentLevel,
                    CostMultiplierPerLevel = GetRarityMultiplier(rarity)
                };
                
                // Set base costs based on rarity
                (cost.SilverCost, cost.GoldCost) = GetBaseCostsByRarity(rarity);
                
                cost.Description = $"Upgrade {rarity} unit from level {currentLevel}";
                
                return cost;
            }
            
            public static UpgradeCost CreateBuildingUpgradeCost(string buildingType, int currentLevel)
            {
                var cost = new UpgradeCost
                {
                    Type = UpgradeType.Building,
                    UpgradeName = $"{buildingType} Upgrade",
                    BaseLevel = currentLevel,
                    CostMultiplierPerLevel = 2.0,
                    SilverCost = 200 * currentLevel,
                    GoldCost = 20 * currentLevel,
                    RequiredTimeMinutes = 30 * currentLevel
                };
                
                cost.Description = $"Upgrade {buildingType} to level {currentLevel + 1}";
                
                return cost;
            }
            
            private static double GetRarityMultiplier(UnitRarity rarity)
            {
                return rarity switch
                {
                    UnitRarity.Common => 1.2,
                    UnitRarity.Rare => 1.5,
                    UnitRarity.Epic => 2.0,
                    UnitRarity.Legendary => 3.0,
                    _ => 1.2
                };
            }
            
            private static (int silver, int gold) GetBaseCostsByRarity(UnitRarity rarity)
            {
                return rarity switch
                {
                    UnitRarity.Common => (50, 5),
                    UnitRarity.Rare => (100, 10),
                    UnitRarity.Epic => (200, 20),
                    UnitRarity.Legendary => (500, 50),
                    _ => (50, 5)
                };
            }
            
            public bool CanAffordUpgrade(Player player, int currentLevel, int targetLevel)
            {
                var (silverCost, goldCost) = CalculateCurrencyCostForLevel(currentLevel, targetLevel);
                return Currency.CanAfford(player, silverCost, goldCost);
            }
            
            public bool MeetsRequirements(Player player)
            {
                if (player.LevelProgress < RequiredPlayerLevel)
                {
                    Console.WriteLine($"[UPGRADE] Requires player level {RequiredPlayerLevel}");
                    return false;
                }
                
                if (!string.IsNullOrEmpty(RequiredBuilding))
                {
                    // Check if player has required building
                    // This would be implemented when building system is added
                    Console.WriteLine($"[UPGRADE] Requires {RequiredBuilding} building");
                    return false;
                }
                
                foreach (var item in RequiredItems)
                {
                    // Check if player has required items
                    // This would be implemented when inventory system is expanded
                    Console.WriteLine($"[UPGRADE] Requires {item}");
                    return false;
                }
                
                return true;
            }
            
            public TransactionResult PurchaseUpgrade(Player player, int currentLevel, int targetLevel)
            {
                if (!MeetsRequirements(player))
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = "Upgrade requirements not met"
                    };
                }
                
                var (silverCost, goldCost) = CalculateCurrencyCostForLevel(currentLevel, targetLevel);
                
                if (!CanAffordUpgrade(player, currentLevel, targetLevel))
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Cannot afford upgrade! Need {silverCost} silver and {goldCost} gold"
                    };
                }
                
                // Process payment
                var transaction = Currency.SpendCurrency(player, silverCost, goldCost, $"{UpgradeName} upgrade");
                if (!transaction.Success)
                {
                    return transaction;
                }
                
                // Record transaction
                var upgradeTransaction = Transaction.CreatePurchase(
                    player.PlayerId, 
                    $"{UpgradeName} (Lvl {currentLevel}→{targetLevel})", 
                    silverCost, 
                    goldCost
                );
                upgradeTransaction.MarkAsSuccessful();
                upgradeTransaction.SetBalances(
                    player.SilverCoins + silverCost, 
                    player.GoldCoins + goldCost,
                    player.SilverCoins, 
                    player.GoldCoins
                );
                TransactionManager.RecordTransaction(upgradeTransaction);
                
                Console.WriteLine($"[UPGRADE] {player.PlayerName} purchased {UpgradeName} from level {currentLevel} to {targetLevel}");
                
                return new TransactionResult
                {
                    Success = true,
                    Message = $"Upgraded {UpgradeName} to level {targetLevel} for {silverCost} silver and {goldCost} gold",
                    SilverSpent = silverCost,
                    GoldSpent = goldCost
                };
            }
            
            public int GetInstantCompleteCost()
            {
                if (!CanInstantComplete) return 0;
                    
                int baseCost = RequiredTimeMinutes / 10; // 1 gold per 10 minutes
                return Math.Max(1, baseCost);
            }
            
            public TransactionResult InstantComplete(Player player)
            {
                if (!CanInstantComplete)
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = "Instant completion not available for this upgrade"
                    };
                }
                
                int goldCost = GetInstantCompleteCost();
                
                if (!Currency.CanAfford(player, 0, goldCost))
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = $"Need {goldCost} gold for instant completion"
                    };
                }
                
                var transaction = Currency.SpendCurrency(player, 0, goldCost, $"Instant complete {UpgradeName}");
                
                Console.WriteLine($"[UPGRADE] {player.PlayerName} instantly completed {UpgradeName} for {goldCost} gold");
                
                return transaction;
            }
            
            public override string ToString()
            {
                string cost = $"{SilverCost} silver";
                if (GoldCost > 0) cost += $", {GoldCost} gold";
                
                string time = RequiredTimeMinutes > 0 ? $", {RequiredTimeMinutes} min" : "";
                string instant = CanInstantComplete ? $", Instant: {GetInstantCompleteCost()} gold" : "";
                
                return $"{UpgradeName} - {cost}{time}{instant}";
            }
            
            public string GetDetailedInfo(int currentLevel = 1, int targetLevel = 2)
            {
                var (silverCost, goldCost) = CalculateCurrencyCostForLevel(currentLevel, targetLevel);
                
                return $"""
                {UpgradeName}
                Type: {Type}
                Level: {currentLevel} → {targetLevel}
                Cost: {silverCost} silver{(goldCost > 0 ? $", {goldCost} gold" : "")}
                Time: {RequiredTimeMinutes} minutes
                {(CanInstantComplete ? $"Instant: {GetInstantCompleteCost()} gold" : "")}
                Requirements: Level {RequiredPlayerLevel}{(RequiredItems.Count > 0 ? $", {string.Join(", ", RequiredItems)}" : "")}
                Effect: {Description}
                {(StatIncrease > 0 ? $"Stat Increase: +{StatIncrease}" : "")}
                {(NewAbility != null ? $"New Ability: {NewAbility}" : "")}
                """;
            }
        }
        public enum UpgradeType
        {
            Unit,
            Building,
            Technology,
            Weapon,
            Armor,
            Special
        }
        
    }
