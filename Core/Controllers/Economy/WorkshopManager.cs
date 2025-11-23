using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Controllers.Economy
{
    // Core/Controllers/Economy/WorkshopManager.cs
    // Dependencies:
    // - Models/Economy/UpgradeCost.cs
    // - Models/Economy/Currency.cs
    // - Models/Player.cs
    // - Models/Units/UnitCard.cs
    // - Models/Units/UnitRarity.cs
    // - Development/DevConfig.cs
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace WarRegionsClone.Controllers.Economy
    {
        public class WorkshopManager
        {
            private List<UpgradeCost> _availableUpgrades;
            private Dictionary<string, DateTime> _upgradeCooldowns;
            private bool _isEnabled;
            
            public WorkshopManager()
            {
                _availableUpgrades = new List<UpgradeCost>();
                _upgradeCooldowns = new Dictionary<string, DateTime>();
                _isEnabled = DevConfig.EnableWorkshopSystem;
                
                InitializeUpgrades();
                Console.WriteLine($"[WORKSHOP] WorkshopManager initialized - Enabled: {_isEnabled}");
            }
            
            private void InitializeUpgrades()
            {
                if (!_isEnabled) return;
                
                // Unit stat upgrades
                _availableUpgrades.Add(new UpgradeCost("Attack Boost I", UpgradeType.Unit, 50, 5)
                {
                    Description = "Increase unit attack power by 10%",
                    StatIncrease = 10,
                    RequiredPlayerLevel = 2,
                    RequiredTimeMinutes = 5
                });
                
                _availableUpgrades.Add(new UpgradeCost("Defense Boost I", UpgradeType.Unit, 50, 5)
                {
                    Description = "Increase unit defense by 10%",
                    StatIncrease = 10, 
                    RequiredPlayerLevel = 2,
                    RequiredTimeMinutes = 5
                });
                
                _availableUpgrades.Add(new UpgradeCost("Health Boost I", UpgradeType.Unit, 75, 8)
                {
                    Description = "Increase unit health by 15%",
                    StatIncrease = 15,
                    RequiredPlayerLevel = 3,
                    RequiredTimeMinutes = 10
                });
                
                // Special ability upgrades
                _availableUpgrades.Add(new UpgradeCost("Double Strike", UpgradeType.Special, 150, 20)
                {
                    Description = "Unit can attack twice per turn",
                    NewAbility = "DoubleStrike",
                    RequiredPlayerLevel = 5,
                    RequiredTimeMinutes = 30,
                    CanInstantComplete = true,
                    InstantCompleteGoldCost = 50
                });
                
                _availableUpgrades.Add(new UpgradeCost("Healing Aura", UpgradeType.Special, 200, 25)
                {
                    Description = "Unit heals adjacent allies each turn",
                    NewAbility = "HealingAura", 
                    RequiredPlayerLevel = 6,
                    RequiredTimeMinutes = 45,
                    CanInstantComplete = true,
                    InstantCompleteGoldCost = 75
                });
                
                Console.WriteLine($"[WORKSHOP] Initialized {_availableUpgrades.Count} available upgrades");
            }
            
            public List<UpgradeCost> GetAvailableUpgrades(Player player)
            {
                if (!_isEnabled)
                    return GetDefaultUpgradesForDevelopment();
                
                return _availableUpgrades
                    .Where(upgrade => upgrade.MeetsRequirements(player))
                    .Where(upgrade => !IsOnCooldown(upgrade.UpgradeId))
                    .ToList();
            }
            
            private List<UpgradeCost> GetDefaultUpgradesForDevelopment()
            {
                // During development, return free instant upgrades
                var defaultUpgrades = new List<UpgradeCost>();
                
                defaultUpgrades.Add(new UpgradeCost("Dev Attack Boost", UpgradeType.Unit, 0, 0)
                {
                    Description = "Free attack upgrade for testing",
                    StatIncrease = 20,
                    RequiredTimeMinutes = 0,
                    CanInstantComplete = true
                });
                
                defaultUpgrades.Add(new UpgradeCost("Dev Health Boost", UpgradeType.Unit, 0, 0)
                {
                    Description = "Free health upgrade for testing", 
                    StatIncrease = 30,
                    RequiredTimeMinutes = 0,
                    CanInstantComplete = true
                });
                
                return defaultUpgrades;
            }
            
            public UpgradeResult PurchaseUpgrade(Player player, string upgradeId, UnitCard targetUnit = null)
            {
                if (!_isEnabled)
                {
                    return new UpgradeResult
                    {
                        Success = true,
                        Message = "Workshop disabled - free upgrade applied (development mode)"
                    };
                }
                
                var upgrade = _availableUpgrades.FirstOrDefault(u => u.UpgradeId == upgradeId);
                if (upgrade == null)
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = "Upgrade not found"
                    };
                }
                
                if (!upgrade.MeetsRequirements(player))
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = "Upgrade requirements not met"
                    };
                }
                
                if (IsOnCooldown(upgradeId))
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = "Upgrade is on cooldown"
                    };
                }
                
                // Process upgrade based on type
                switch (upgrade.Type)
                {
                    case UpgradeType.Unit:
                        return ApplyUnitUpgrade(player, upgrade, targetUnit);
                        
                    case UpgradeType.Special:
                        return ApplySpecialUpgrade(player, upgrade, targetUnit);
                        
                    default:
                        return new UpgradeResult
                        {
                            Success = false,
                            Message = "Unsupported upgrade type"
                        };
                }
            }
            
            private UpgradeResult ApplyUnitUpgrade(Player player, UpgradeCost upgrade, UnitCard targetUnit)
            {
                if (targetUnit == null)
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = "No target unit specified for unit upgrade"
                    };
                }
                
                // Calculate cost for next level
                var (silverCost, goldCost) = upgrade.CalculateCurrencyCostForLevel(targetUnit.Level, targetUnit.Level + 1);
                
                if (!Currency.CanAfford(player, silverCost, goldCost))
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = $"Cannot afford upgrade! Need {silverCost} silver and {goldCost} gold"
                    };
                }
                
                // Process payment
                var transaction = Currency.SpendCurrency(player, silverCost, goldCost, $"{upgrade.UpgradeName} for {targetUnit.UnitName}");
                if (!transaction.Success)
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = transaction.Message
                    };
                }
                
                // Apply upgrade
                targetUnit.Stats.Upgrade(targetUnit.Level);
                targetUnit.Level++;
                
                // Set cooldown
                SetCooldown(upgrade.UpgradeId, TimeSpan.FromMinutes(upgrade.RequiredTimeMinutes));
                
                Console.WriteLine($"[WORKSHOP] Applied {upgrade.UpgradeName} to {targetUnit.UnitName} (Level {targetUnit.Level})");
                
                return new UpgradeResult
                {
                    Success = true,
                    Message = $"Upgraded {targetUnit.UnitName} to level {targetUnit.Level}",
                    UpgradedUnit = targetUnit,
                    NewLevel = targetUnit.Level,
                    SilverCost = silverCost,
                    GoldCost = goldCost
                };
            }
            
            private UpgradeResult ApplySpecialUpgrade(Player player, UpgradeCost upgrade, UnitCard targetUnit)
            {
                if (targetUnit == null)
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = "No target unit specified for special upgrade"
                    };
                }
                
                if (!Currency.CanAfford(player, upgrade.SilverCost, upgrade.GoldCost))
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = $"Cannot afford special upgrade! Need {upgrade.SilverCost} silver and {upgrade.GoldCost} gold"
                    };
                }
                
                // Process payment
                var transaction = Currency.SpendCurrency(player, upgrade.SilverCost, upgrade.GoldCost, $"{upgrade.UpgradeName} for {targetUnit.UnitName}");
                if (!transaction.Success)
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = transaction.Message
                    };
                }
                
                // Apply special ability
                if (!string.IsNullOrEmpty(upgrade.NewAbility))
                {
                    // In real implementation, this would add the ability to the unit
                    Console.WriteLine($"[WORKSHOP] Added {upgrade.NewAbility} to {targetUnit.UnitName}");
                }
                
                // Set cooldown
                SetCooldown(upgrade.UpgradeId, TimeSpan.FromMinutes(upgrade.RequiredTimeMinutes));
                
                return new UpgradeResult
                {
                    Success = true,
                    Message = $"Applied {upgrade.UpgradeName} to {targetUnit.UnitName}",
                    UpgradedUnit = targetUnit,
                    NewAbility = upgrade.NewAbility,
                    SilverCost = upgrade.SilverCost,
                    GoldCost = upgrade.GoldCost
                };
            }
            
            public UpgradeResult InstantCompleteUpgrade(Player player, string upgradeId)
            {
                var upgrade = _availableUpgrades.FirstOrDefault(u => u.UpgradeId == upgradeId);
                if (upgrade == null)
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = "Upgrade not found"
                    };
                }
                
                if (!upgrade.CanInstantComplete)
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = "This upgrade cannot be instantly completed"
                    };
                }
                
                var instantCost = upgrade.GetInstantCompleteCost();
                if (!Currency.CanAfford(player, 0, instantCost))
                {
                    return new UpgradeResult
                    {
                        Success = false,
                        Message = $"Need {instantCost} gold for instant completion"
                    };
                }
                
                var transaction = Currency.SpendCurrency(player, 0, instantCost, $"Instant complete {upgrade.UpgradeName}");
                
                // Remove cooldown
                RemoveCooldown(upgradeId);
                
                return new UpgradeResult
                {
                    Success = transaction.Success,
                    Message = transaction.Success ? $"Instantly completed {upgrade.UpgradeName}" : transaction.Message,
                    GoldCost = instantCost
                };
            }
            
            private bool IsOnCooldown(string upgradeId)
            {
                if (!_upgradeCooldowns.ContainsKey(upgradeId))
                    return false;
                    
                return _upgradeCooldowns[upgradeId] > DateTime.Now;
            }
            
            private void SetCooldown(string upgradeId, TimeSpan duration)
            {
                _upgradeCooldowns[upgradeId] = DateTime.Now.Add(duration);
            }
            
            private void RemoveCooldown(string upgradeId)
            {
                if (_upgradeCooldowns.ContainsKey(upgradeId))
                {
                    _upgradeCooldowns.Remove(upgradeId);
                }
            }
            
            public TimeSpan GetRemainingCooldown(string upgradeId)
            {
                if (!_upgradeCooldowns.ContainsKey(upgradeId))
                    return TimeSpan.Zero;
                    
                var remaining = _upgradeCooldowns[upgradeId] - DateTime.Now;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
            
            public void EnableWorkshop(bool enable)
            {
                _isEnabled = enable;
                Console.WriteLine($"[WORKSHOP] Workshop system {(enable ? "enabled" : "disabled")}");
            }
            
            public bool IsWorkshopEnabled()
            {
                return _isEnabled;
            }
            
            public void AddUpgrade(UpgradeCost upgrade)
            {
                _availableUpgrades.Add(upgrade);
                Console.WriteLine($"[WORKSHOP] Added new upgrade: {upgrade.UpgradeName}");
            }
            
            public void RemoveUpgrade(string upgradeId)
            {
                var upgrade = _availableUpgrades.FirstOrDefault(u => u.UpgradeId == upgradeId);
                if (upgrade != null)
                {
                    _availableUpgrades.Remove(upgrade);
                    Console.WriteLine($"[WORKSHOP] Removed upgrade: {upgrade.UpgradeName}");
                }
            }
            
            public List<UpgradeCost> GetUpgradesForUnit(UnitCard unit, Player player)
            {
                return GetAvailableUpgrades(player)
                    .Where(upgrade => upgrade.Type == UpgradeType.Unit || upgrade.Type == UpgradeType.Special)
                    .Where(upgrade => unit.Level >= upgrade.BaseLevel)
                    .ToList();
            }
            
            public string GetWorkshopStatus()
            {
                int availableCount = _availableUpgrades.Count;
                int onCooldown = _upgradeCooldowns.Count(kvp => kvp.Value > DateTime.Now);
                
                return $"""
                Workshop Status:
                Enabled: {_isEnabled}
                Available Upgrades: {availableCount}
                Currently on Cooldown: {onCooldown}
                Player Level Required: {_availableUpgrades.Min(u => u.RequiredPlayerLevel)}-{_availableUpgrades.Max(u => u.RequiredPlayerLevel)}
                """;
            }
            
            public void ProcessCooldowns()
            {
                var expiredUpgrades = _upgradeCooldowns
                    .Where(kvp => kvp.Value <= DateTime.Now)
                    .Select(kvp => kvp.Key)
                    .ToList();
                
                foreach (var upgradeId in expiredUpgrades)
                {
                    _upgradeCooldowns.Remove(upgradeId);
                }
                
                if (expiredUpgrades.Count > 0)
                {
                    Console.WriteLine($"[WORKSHOP] Cleared {expiredUpgrades.Count} expired cooldowns");
                }
            }
            
            public UpgradeCost GetUpgrade(string upgradeId)
            {
                return _availableUpgrades.FirstOrDefault(u => u.UpgradeId == upgradeId);
            }
            
            public void ResetAllCooldowns()
            {
                _upgradeCooldowns.Clear();
                Console.WriteLine($"[WORKSHOP] All upgrade cooldowns reset");
            }
        }
        
        public class UpgradeResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public UnitCard UpgradedUnit { get; set; }
            public int NewLevel { get; set; }
            public string NewAbility { get; set; }
            public int SilverCost { get; set; }
            public int GoldCost { get; set; }
            
            public UpgradeResult()
            {
                Success = false;
                Message = string.Empty;
            }
        }
    }}
