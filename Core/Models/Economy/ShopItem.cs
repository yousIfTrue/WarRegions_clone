// Core/Models/Economy/ShopItem.cs
// Dependencies:
// - Units/UnitCard.cs (for UnitCard property)
// - Currency.cs (for pricing)

using System;

namespace WarRegionsClone.Models.Economy
{
    public class ShopItem
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public ShopItemType ItemType { get; set; }
        
        // For unit items
        public UnitCard UnitCard { get; set; }
        
        // For equipment/consumables
        public string ItemEffect { get; set; }
        public int EffectValue { get; set; }
        
        // Pricing
        public int SilverCost { get; set; }
        public int GoldCost { get; set; }
        public bool IsOnSale { get; set; }
        public int SaleDiscount { get; set; } // Percentage
        
        // Stock and availability
        public int StockQuantity { get; set; } = 1;
        public int DailyStock { get; set; } = 1;
        public DateTime AvailableUntil { get; set; }
        public bool IsLimited { get; set; }
        
        // Requirements
        public int RequiredLevel { get; set; } = 1;
        public string RequiredAchievement { get; set; }
        
        public ShopItem()
        {
            ItemId = Guid.NewGuid().ToString();
            AvailableUntil = DateTime.Now.AddDays(1);
        }
        
        public ShopItem(string name, ShopItemType type, int silverCost, int goldCost = 0) : this()
        {
            ItemName = name;
            ItemType = type;
            SilverCost = silverCost;
            GoldCost = goldCost;
        }
        
        public int GetActualSilverCost()
        {
            if (IsOnSale && SaleDiscount > 0)
            {
                return SilverCost - (SilverCost * SaleDiscount / 100);
            }
            return SilverCost;
        }
        
        public int GetActualGoldCost()
        {
            if (IsOnSale && SaleDiscount > 0)
            {
                return GoldCost - (GoldCost * SaleDiscount / 100);
            }
            return GoldCost;
        }
        
        public bool CanPlayerPurchase(Player player)
        {
            if (StockQuantity <= 0)
            {
                Console.WriteLine($"[SHOP] {ItemName} is out of stock!");
                return false;
            }
            
            if (DateTime.Now > AvailableUntil)
            {
                Console.WriteLine($"[SHOP] {ItemName} is no longer available!");
                return false;
            }
            
            if (player.LevelProgress < RequiredLevel)
            {
                Console.WriteLine($"[SHOP] Requires level {RequiredLevel} to purchase {ItemName}");
                return false;
            }
            
            if (!string.IsNullOrEmpty(RequiredAchievement))
            {
                // Achievement check would go here
                Console.WriteLine($"[SHOP] Requires achievement '{RequiredAchievement}' to purchase {ItemName}");
                return false;
            }
            
            if (!Currency.CanAfford(player, GetActualSilverCost(), GetActualGoldCost()))
            {
                Console.WriteLine($"[SHOP] Cannot afford {ItemName}");
                return false;
            }
            
            return true;
        }
        
        public TransactionResult Purchase(Player player)
        {
            if (!CanPlayerPurchase(player))
            {
                return new TransactionResult
                {
                    Success = false,
                    Message = $"Cannot purchase {ItemName}"
                };
            }
            
            // Process payment
            var transaction = Currency.SpendCurrency(player, GetActualSilverCost(), GetActualGoldCost(), ItemName);
            if (!transaction.Success)
            {
                return transaction;
            }
            
            // Deliver item
            DeliverItem(player);
            
            // Reduce stock
            StockQuantity--;
            
            Console.WriteLine($"[SHOP] {player.PlayerName} purchased {ItemName}");
            
            return new TransactionResult
            {
                Success = true,
                Message = $"Purchased {ItemName} for {GetActualSilverCost()} silver" + 
                         (GetActualGoldCost() > 0 ? $" and {GetActualGoldCost()} gold" : ""),
                SilverSpent = GetActualSilverCost(),
                GoldSpent = GetActualGoldCost()
            };
        }
        
        private void DeliverItem(Player player)
        {
            switch (ItemType)
            {
                case ShopItemType.Unit:
                    if (UnitCard != null)
                    {
                        player.AvailableUnits.Add(UnitCard);
                        Console.WriteLine($"[SHOP] Added {UnitCard.UnitName} to player's inventory");
                    }
                    break;
                    
                case ShopItemType.CurrencyPack:
                    Currency.AddCurrency(player, EffectValue, 0, "currency pack");
                    break;
                    
                case ShopItemType.Consumable:
                    // Apply consumable effect
                    Console.WriteLine($"[SHOP] Used consumable: {ItemEffect} (+{EffectValue})");
                    break;
                    
                case ShopItemType.Equipment:
                    // Equip item to unit
                    Console.WriteLine($"[SHOP] Equipped {ItemName} to unit");
                    break;
                    
                case ShopItemType.Upgrade:
                    // Apply upgrade
                    Console.WriteLine($"[SHOP] Applied upgrade: {ItemName}");
                    break;
            }
        }
        
        public void ApplySale(int discountPercent, DateTime saleEnd)
        {
            IsOnSale = true;
            SaleDiscount = discountPercent;
            AvailableUntil = saleEnd;
            Console.WriteLine($"[SHOP] {ItemName} is now {discountPercent}% off! Sale ends {saleEnd:MM/dd}");
        }
        
        public void Restock(int quantity)
        {
            StockQuantity = Math.Min(quantity, DailyStock);
            Console.WriteLine($"[SHOP] Restocked {ItemName} to {StockQuantity}");
        }
        
        public void SetAsDailyDeal()
        {
            ApplySale(25, DateTime.Now.AddDays(1));
            StockQuantity = 1;
            IsLimited = true;
            Console.WriteLine($"[SHOP] {ItemName} is today's daily deal!");
        }
        
        public override string ToString()
        {
            string price = $"{GetActualSilverCost()} silver";
            if (GetActualGoldCost() > 0) price += $", {GetActualGoldCost()} gold";
            
            if (IsOnSale)
            {
                price += $" ({SaleDiscount}% OFF!)";
            }
            
            string stockInfo = StockQuantity > 0 ? $"{StockQuantity} in stock" : "OUT OF STOCK";
            string limited = IsLimited ? " [LIMITED]" : "";
            
            return $"{ItemName} - {price} - {stockInfo}{limited}";
        }
        
        public string GetDetailedInfo()
        {
            string info = $"""
            {ItemName}
            Type: {ItemType}
            Cost: {GetActualSilverCost()} silver{(GetActualGoldCost() > 0 ? $", {GetActualGoldCost()} gold" : "")}
            Stock: {StockQuantity}
            {(IsOnSale ? $"SALE: {SaleDiscount}% off!" : "")}
            """;
            
            if (ItemType == ShopItemType.Unit && UnitCard != null)
            {
                info += $"\nUnit: {UnitCard.UnitName} ({UnitCard.Rarity})\nStats: {UnitCard.Stats.ToShortString()}";
            }
            else if (!string.IsNullOrEmpty(ItemEffect))
            {
                info += $"\nEffect: {ItemEffect} (+{EffectValue})";
            }
            
            if (RequiredLevel > 1)
            {
                info += $"\nRequires Level: {RequiredLevel}";
            }
            
            return info;
        }
    }
    
    public enum ShopItemType
    {
        Unit,
        CurrencyPack,
        Consumable,
        Equipment,
        Upgrade,
        Special
    }
}