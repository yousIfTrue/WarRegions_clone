// Core/Models/Economy/ShopItem.cs
namespace WarRegions.Core.Models.Economy
{
    public class ShopItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; // ✅ أضفنا Description
        public int Cost { get; set; }
        public string CurrencyType { get; set; } = "silver";
        public string ItemType { get; set; } = "unit";
        public int Quantity { get; set; } = 1;
        public bool IsAvailable { get; set; } = true;
        
        public ShopItem() { }
        
        public ShopItem(string id, string name, string description, int cost)
        {
            Id = id;
            Name = name;
            Description = description;
            Cost = cost;
        }

        public int GetActualSilverCost()
        {
            return CurrencyType == "silver" ? Cost : Cost * 100;
        }

        public int GetActualGoldCost()
        {
            return CurrencyType == "gold" ? Cost : Cost / 100;
        }
    }
    public enum ShopItemType
    {
        Unit,
        Equipment,
        Resource,
        Boost,
        Cosmetic,
        Special
    }

}
