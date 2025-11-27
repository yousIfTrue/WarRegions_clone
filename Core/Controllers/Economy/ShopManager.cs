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
