using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Controllers.Economy
{
    // Core/Controllers/Economy/ShopManager.cs
    // Dependencies:
    // - Models/Economy/ShopItem.cs
    // - Models/Economy/Currency.cs
    // - Models/Player.cs
    // - Models/Units/UnitCard.cs
    // - Development/DevConfig.cs
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    namespace WarRegionsClone.Controllers.Economy
    {
        public class ShopManager
        {
            private List<ShopItem> _availableItems;
            private List<ShopItem> _dailyOffers;
            private DateTime _lastShopRefresh;
            private bool _isEnabled;
            
            public ShopManager()
            {
                _availableItems = new List<ShopItem>();
                _dailyOffers = new List<ShopItem>();
                _isEnabled = DevConfig.EnableShopSystem;
                _lastShopRefresh = DateTime.Now;
                
                InitializeShopItems();
                RefreshDailyOffers();
                
                Console.WriteLine($"[SHOP] ShopManager initialized - Enabled: {_isEnabled}");
            }
            
            private void InitializeShopItems()
            {
                if (!_isEnabled) return;
                
                // Basic units
                _availableItems.Add(new ShopItem("Foot Soldier", ShopItemType.Unit, 100)
                {
                    Description = "A reliable infantry unit",
                    RequiredLevel = 1
                });
                
                _availableItems.Add(new ShopItem("Archer", ShopItemType.Unit, 150)
                {
                    Description = "Ranged unit with good attack",
                    RequiredLevel = 2
                });
                
                _availableItems.Add(new ShopItem("Cavalry Knight", ShopItemType.Unit, 200)
                {
                    Description = "Fast moving cavalry unit", 
                    RequiredLevel = 3
                });
                
                // Currency packs
                _availableItems.Add(new ShopItem("Silver Pack", ShopItemType.CurrencyPack, 0, 10)
                {
                    Description = "500 silver coins",
                    EffectValue = 500,
                    RequiredLevel = 1
                });
                
                _availableItems.Add(new ShopItem("Gold Pack", ShopItemType.CurrencyPack, 500, 0)
                {
                    Description = "50 gold coins", 
                    EffectValue = 50,
                    RequiredLevel = 2
                });
                
                Console.WriteLine($"[SHOP] Initialized {_availableItems.Count} shop items");
            }
            
            public void RefreshDailyOffers()
            {
                if (!_isEnabled) return;
                
                _dailyOffers.Clear();
                var random = new Random();
                
                // Select 3 random items for daily offers
                var availableForOffers = _availableItems
                    .Where(item => item.StockQuantity > 0)
                    .OrderBy(x => random.Next())
                    .Take(3)
                    .ToList();
                
                foreach (var item in availableForOffers)
                {
                    var dailyItem = item; // In real implementation, this would be a copy
                    dailyItem.SetAsDailyDeal();
                    _dailyOffers.Add(dailyItem);
                }
                
                _lastShopRefresh = DateTime.Now;
                Console.WriteLine($"[SHOP] Daily offers refreshed: {_dailyOffers.Count} items");
            }
            
            public List<ShopItem> GetAvailableItems(Player player)
            {
                if (!_isEnabled) 
                    return GetDefaultItemsForDevelopment();
                
                return _availableItems
                    .Where(item => item.CanPlayerPurchase(player))
                    .ToList();
            }
            
            public List<ShopItem> GetDailyOffers(Player player)
            {
                if (!_isEnabled)
                    return GetDefaultItemsForDevelopment();
                
                // Check if daily offers need refresh
                if (DateTime.Now - _lastShopRefresh > TimeSpan.FromHours(24))
                {
                    RefreshDailyOffers();
                }
                
                return _dailyOffers
                    .Where(item => item.CanPlayerPurchase(player))
                    .ToList();
            }
            
            private List<ShopItem> GetDefaultItemsForDevelopment()
            {
                // During development, return some basic items for testing
                var defaultItems = new List<ShopItem>();
                
                defaultItems.Add(new ShopItem("Test Infantry", ShopItemType.Unit, 0)
                {
                    Description = "Free unit for testing",
                    StockQuantity = 99
                });
                
                defaultItems.Add(new ShopItem("Test Archer", ShopItemType.Unit, 0) 
                {
                    Description = "Free unit for testing",
                    StockQuantity = 99
                });
                
                return defaultItems;
            }
            
            public TransactionResult PurchaseItem(Player player, string itemId, int quantity = 1)
            {
                if (!_isEnabled)
                {
                    return new TransactionResult
                    {
                        Success = true,
                        Message = "Shop system disabled - development mode active"
                    };
                }
                
                var item = _availableItems.FirstOrDefault(i => i.ItemId == itemId);
                if (item == null)
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = "Item not found in shop"
                    };
                }
                
                if (quantity > item.StockQuantity)
                {
                    return new TransactionResult
                    {
                        Success = false, 
                        Message = $"Not enough stock. Available: {item.StockQuantity}"
                    };
                }
                
                var result = item.Purchase(player);
                if (result.Success)
                {
                    item.StockQuantity -= quantity;
                    Console.WriteLine($"[SHOP] {player.PlayerName} purchased {item.ItemName}");
                }
                
                return result;
            }
            
            public TransactionResult PurchaseDailyOffer(Player player, string itemId)
            {
                var offer = _dailyOffers.FirstOrDefault(i => i.ItemId == itemId);
                if (offer == null)
                {
                    return new TransactionResult
                    {
                        Success = false,
                        Message = "Daily offer not found"
                    };
                }
                
                return PurchaseItem(player, itemId);
            }
            
            public void AddShopItem(ShopItem item)
            {
                _availableItems.Add(item);
                Console.WriteLine($"[SHOP] Added new item: {item.ItemName}");
            }
            
            public void RemoveShopItem(string itemId)
            {
                var item = _availableItems.FirstOrDefault(i => i.ItemId == itemId);
                if (item != null)
                {
                    _availableItems.Remove(item);
                    Console.WriteLine($"[SHOP] Removed item: {item.ItemName}");
                }
            }
            
            public void RestockShop()
            {
                foreach (var item in _availableItems)
                {
                    item.Restock(item.DailyStock);
                }
                
                Console.WriteLine($"[SHOP] Shop restocked");
            }
            
            public void EnableShop(bool enable)
            {
                _isEnabled = enable;
                Console.WriteLine($"[SHOP] Shop system {(enable ? "enabled" : "disabled")}");
            }
            
            public bool IsShopEnabled()
            {
                return _isEnabled;
            }
            
            public string GetShopStatus()
            {
                return $"""
                Shop Status:
                Enabled: {_isEnabled}
                Total Items: {_availableItems.Count}
                Daily Offers: {_dailyOffers.Count}
                Last Refresh: {_lastShopRefresh:yyyy-MM-dd HH:mm}
                In Stock: {_availableItems.Count(i => i.StockQuantity > 0)}/{_availableItems.Count}
                """;
            }
            
            public List<ShopItem> GetItemsByType(ShopItemType type, Player player = null)
            {
                var items = _availableItems.Where(item => item.ItemType == type);
                
                if (player != null)
                {
                    items = items.Where(item => item.CanPlayerPurchase(player));
                }
                
                return items.ToList();
            }
            
            public ShopItem GetItem(string itemId)
            {
                return _availableItems.FirstOrDefault(i => i.ItemId == itemId);
            }
            
            public void ApplySaleToCategory(ShopItemType category, int discountPercent, DateTime saleEnd)
            {
                var categoryItems = _availableItems.Where(item => item.ItemType == category);
                
                foreach (var item in categoryItems)
                {
                    item.ApplySale(discountPercent, saleEnd);
                }
                
                Console.WriteLine($"[SHOP] Applied {discountPercent}% sale to {category} category");
            }
            
            public void UpdateItemPrice(string itemId, int newSilverCost, int newGoldCost = 0)
            {
                var item = GetItem(itemId);
                if (item != null)
                {
                    item.SilverCost = newSilverCost;
                    item.GoldCost = newGoldCost;
                    Console.WriteLine($"[SHOP] Updated price for {item.ItemName}: {newSilverCost} silver, {newGoldCost} gold");
                }
            }
        }
    }}
