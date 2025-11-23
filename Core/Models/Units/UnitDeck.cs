// Core/Models/Units/UnitDeck.cs
// Dependencies:
// - UnitCard.cs (for Units list)
// - UnitRarity.cs (for rarity restrictions)

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegionsClone.Models.Units
{
    public class UnitDeck
    {
        public string DeckId { get; set; }
        public string DeckName { get; set; }
        public List<UnitCard> Units { get; set; }
        
        // Deck constraints
        public int MaxDeckSize { get; set; } = 6;
        public int MaxSameUnitType { get; set; } = 2;
        public int MaxRarityCount { get; set; } = 1; // Max legendary/epic units
        
        // Deck statistics
        public int TotalAttack => Units.Sum(u => u.Stats.Attack);
        public int TotalDefense => Units.Sum(u => u.Stats.Defense);
        public int TotalHealth => Units.Sum(u => u.Stats.MaxHealth);
        public double AverageSpeed => Units.Average(u => u.Stats.Speed);
        
        // Cost management
        public int TotalSilverCost => Units.Sum(u => u.Stats.SilverCost);
        public int TotalGoldCost => Units.Sum(u => u.Stats.GoldCost);
        public int TotalUpkeep => Units.Sum(u => u.Stats.UpkeepCost);
        
        public UnitDeck()
        {
            DeckId = Guid.NewGuid().ToString();
            DeckName = "Default Deck";
            Units = new List<UnitCard>();
        }
        
        public UnitDeck(string name) : this()
        {
            DeckName = name;
        }
        
        public bool CanAddUnit(UnitCard unit)
        {
            if (Units.Count >= MaxDeckSize)
            {
                Console.WriteLine($"Deck is full! Maximum {MaxDeckSize} units allowed.");
                return false;
            }
            
            // Check unit type limit
            var sameTypeCount = Units.Count(u => u.UnitName.Split(' ').Last() == unit.UnitName.Split(' ').Last());
            if (sameTypeCount >= MaxSameUnitType)
            {
                Console.WriteLine($"Too many {unit.UnitName.Split(' ').Last()} units! Maximum {MaxSameUnitType} allowed.");
                return false;
            }
            
            // Check rarity limit for epic/legendary
            if (unit.Rarity == UnitRarity.Epic || unit.Rarity == UnitRarity.Legendary)
            {
                var highRarityCount = Units.Count(u => u.Rarity == UnitRarity.Epic || u.Rarity == UnitRarity.Legendary);
                if (highRarityCount >= MaxRarityCount)
                {
                    Console.WriteLine($"Too many {unit.Rarity} units! Maximum {MaxRarityCount} allowed.");
                    return false;
                }
            }
            
            return true;
        }
        
        public bool AddUnit(UnitCard unit)
        {
            if (!CanAddUnit(unit))
                return false;
                
            Units.Add(unit);
            Console.WriteLine($"Added {unit.UnitName} to deck '{DeckName}'");
            return true;
        }
        
        public bool RemoveUnit(UnitCard unit)
        {
            bool removed = Units.Remove(unit);
            if (removed)
            {
                Console.WriteLine($"Removed {unit.UnitName} from deck '{DeckName}'");
            }
            return removed;
        }
        
        public UnitCard RemoveUnitAt(int index)
        {
            if (index < 0 || index >= Units.Count)
            {
                Console.WriteLine($"Invalid unit index: {index}");
                return null;
            }
            
            var unit = Units[index];
            Units.RemoveAt(index);
            Console.WriteLine($"Removed {unit.UnitName} from deck '{DeckName}'");
            return unit;
        }
        
        public void ClearDeck()
        {
            Units.Clear();
            Console.WriteLine($"Cleared all units from deck '{DeckName}'");
        }
        
        public bool SwapUnits(int index1, int index2)
        {
            if (index1 < 0 || index1 >= Units.Count || index2 < 0 || index2 >= Units.Count)
            {
                Console.WriteLine("Invalid unit indices for swap");
                return false;
            }
            
            var temp = Units[index1];
            Units[index1] = Units[index2];
            Units[index2] = temp;
            
            Console.WriteLine($"Swapped positions of {Units[index1].UnitName} and {Units[index2].UnitName}");
            return true;
        }
        
        public UnitCard GetUnitAt(int index)
        {
            if (index < 0 || index >= Units.Count)
                return null;
                
            return Units[index];
        }
        
        public List<UnitCard> GetUnitsByType(string unitType)
        {
            return Units.Where(u => u.UnitName.ToLower().Contains(unitType.ToLower())).ToList();
        }
        
        public List<UnitCard> GetUnitsByRarity(UnitRarity rarity)
        {
            return Units.Where(u => u.Rarity == rarity).ToList();
        }
        
        public int GetUnitTypeCount(string unitType)
        {
            return Units.Count(u => u.UnitName.ToLower().Contains(unitType.ToLower()));
        }
        
        public bool IsValid()
        {
            if (Units.Count < 3)
            {
                Console.WriteLine("Deck must have at least 3 units");
                return false;
            }
            
            if (Units.Count > MaxDeckSize)
            {
                Console.WriteLine($"Deck cannot have more than {MaxDeckSize} units");
                return false;
            }
            
            // Check unit type limits
            var unitTypes = Units.GroupBy(u => u.UnitName.Split(' ').Last());
            foreach (var group in unitTypes)
            {
                if (group.Count() > MaxSameUnitType)
                {
                    Console.WriteLine($"Too many {group.Key} units: {group.Count()} (max {MaxSameUnitType})");
                    return false;
                }
            }
            
            // Check rarity limits
            var highRarityCount = Units.Count(u => u.Rarity == UnitRarity.Epic || u.Rarity == UnitRarity.Legendary);
            if (highRarityCount > MaxRarityCount)
            {
                Console.WriteLine($"Too many high rarity units: {highRarityCount} (max {MaxRarityCount})");
                return false;
            }
            
            return true;
        }
        
        public string GetDeckSummary()
        {
            var unitCounts = Units.GroupBy(u => u.UnitName.Split(' ').Last())
                                .ToDictionary(g => g.Key, g => g.Count());
            
            var rarityCounts = Units.GroupBy(u => u.Rarity)
                                  .ToDictionary(g => g.Key, g => g.Count());
            
            string unitSummary = string.Join(", ", unitCounts.Select(kvp => $"{kvp.Value}x {kvp.Key}"));
            string raritySummary = string.Join(", ", rarityCounts.Select(kvp => $"{kvp.Value} {kvp.Key}"));
            
            return $"""
            Deck: {DeckName}
            Units: {Units.Count}/{MaxDeckSize}
            Composition: {unitSummary}
            Rarity: {raritySummary}
            Stats: {TotalAttack} ATK, {TotalDefense} DEF, {TotalHealth} HP
            Cost: {TotalSilverCost} silver, {TotalGoldCost} gold
            """;
        }
        
        public UnitDeck Clone()
        {
            var clone = new UnitDeck(DeckName + " Copy")
            {
                MaxDeckSize = MaxDeckSize,
                MaxSameUnitType = MaxSameUnitType,
                MaxRarityCount = MaxRarityCount
            };
            
            foreach (var unit in Units)
            {
                clone.Units.Add(unit); // Note: This creates a shallow copy
            }
            
            return clone;
        }
        
        public void SortByAttack()
        {
            Units = Units.OrderByDescending(u => u.Stats.Attack).ToList();
            Console.WriteLine("Deck sorted by attack power");
        }
        
        public void SortByDefense()
        {
            Units = Units.OrderByDescending(u => u.Stats.Defense).ToList();
            Console.WriteLine("Deck sorted by defense");
        }
        
        public void SortBySpeed()
        {
            Units = Units.OrderByDescending(u => u.Stats.Speed).ToList();
            Console.WriteLine("Deck sorted by speed");
        }
        
        public void SortByRarity()
        {
            Units = Units.OrderByDescending(u => u.Rarity).ThenBy(u => u.UnitName).ToList();
            Console.WriteLine("Deck sorted by rarity");
        }
        
        public override string ToString()
        {
            return $"{DeckName} - {Units.Count}/{MaxDeckSize} units - {TotalAttack} ATK";
        }
    }
}