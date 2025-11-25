//old code is down
namespace WarRegions.Core.Controllers.Economy
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
            _availableItems.Clear();
            
            // إنشاء الـ ShopItems باستخدام constructor الصحيح (4 parameters)
            _availableItems.Add(new ShopItem("foot_soldier", "Foot Soldier", "Basic infantry unit", 100));
            _availableItems.Add(new ShopItem("archer_01", "Archer", "Ranged unit with good attack", 150));
            _availableItems.Add(new ShopItem("cavalry_01", "Cavalry Knight", "Fast moving cavalry unit", 200));
            _availableItems.Add(new ShopItem("silver_pack", "Silver Pack", "500 silver coins", 0));
            _availableItems.Add(new ShopItem("gold_pack", "Gold Pack", "50 gold coins", 500));
            
            // إذا كنت تحتاج خصائص إضافية، استخدم object initializer
            foreach (var item in _availableItems)
            {
                // تعيين الخصائص الإضافية
                if (item.Name.Contains("Silver"))
                {
                    item.CurrencyType = "silver";
                    item.ItemType = "currency";
                }
                else if (item.Name.Contains("Gold"))
                {
                    item.CurrencyType = "gold";
                    item.ItemType = "currency";
                }
                else
                {
                    item.ItemType = "unit";
                }
            }
            
            Console.WriteLine($"[SHOP] Initialized {_availableItems.Count} shop items");
        }
        
        public void RefreshDailyOffers()
        {
            if (!_isEnabled) return;
            
            _dailyOffers.Clear();
            var random = new Random();
            
            // اختيار 3 عناصر عشوائية للعروض اليومية
            var availableForOffers = _availableItems
                .Where(item => item.IsAvailable)
                .OrderBy(x => random.Next())
                .Take(3)
                .ToList();
            
            _dailyOffers.AddRange(availableForOffers);
            _lastShopRefresh = DateTime.Now;
            
            Console.WriteLine($"[SHOP] Daily offers refreshed: {_dailyOffers.Count} items");
        }
        
        public List<ShopItem> GetAvailableItems(Player player)
        {
            if (!_isEnabled) 
                return GetDefaultItemsForDevelopment();
            
            return _availableItems
                .Where(item => item.IsAvailable)
                .ToList();
        }
        
        public List<ShopItem> GetDailyOffers(Player player)
        {
            if (!_isEnabled)
                return GetDefaultItemsForDevelopment();
            
            // التحقق إذا كانت العروض اليومية تحتاج تحديث
            if (DateTime.Now - _lastShopRefresh > TimeSpan.FromHours(24))
            {
                RefreshDailyOffers();
            }
            
            return _dailyOffers
                .Where(item => item.IsAvailable)
                .ToList();
        }
        
        private List<ShopItem> GetDefaultItemsForDevelopment()
        {
            // خلال التطوير، إرجاع عناصر أساسية للاختبار
            return new List<ShopItem>
            {
                new ShopItem("test_infantry", "Test Infantry", "Free unit for testing", 0),
                new ShopItem("test_archer", "Test Archer", "Free unit for testing", 0)
            };
        }
        
        public bool PurchaseItem(Player player, string itemId)
        {
            if (!_isEnabled)
            {
                Console.WriteLine($"[SHOP] Shop disabled - purchase granted for testing");
                return true;
            }
            
            var item = _availableItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                Console.WriteLine($"[SHOP] Item not found: {itemId}");
                return false;
            }
            
            if (!item.IsAvailable)
            {
                Console.WriteLine($"[SHOP] Item not available: {item.Name}");
                return false;
            }
            
            // محاكاة عملية الشراء (ستحتاج لتطبيق منطق الدفع الحقيقي)
            Console.WriteLine($"[SHOP] {player.PlayerName} purchased {item.Name} for {item.Cost} {item.CurrencyType}");
            return true;
        }
        
        // باقي الدوال تبقى كما هي مع تعديلات بسيطة
        public void AddShopItem(ShopItem item)
        {
            _availableItems.Add(item);
            Console.WriteLine($"[SHOP] Added new item: {item.Name}");
        }
        
        public void RemoveShopItem(string itemId)
        {
            var item = _availableItems.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                _availableItems.Remove(item);
                Console.WriteLine($"[SHOP] Removed item: {item.Name}");
            }
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
            Available: {_availableItems.Count(i => i.IsAvailable)}/{_availableItems.Count}
            """;
        }
        
        public ShopItem GetItem(string itemId)
        {
            return _availableItems.FirstOrDefault(i => i.Id == itemId);
        }
    }
}




/*
    namespace WarRegions.Core.Controllers.Economy
    {
        // Core/Controllers/Economy/ShopManager.cs
    // Dependencies:
    // - Models/Economy/ShopItem.cs
    // - Models/Economy/Currency.cs
    // - Models/Player.cs
    // - Models/Units/UnitCard.cs
    // - Development/DevConfig.cs

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
                _availableItems.Add(new ShopItem("foot_soldier", "Foot Soldier", "Basic infantry unit", ShopItemType.Unit, 100)
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
    }
*/