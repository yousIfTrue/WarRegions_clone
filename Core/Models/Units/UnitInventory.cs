// Core/Models/Units/UnitInventory.cs
// Dependencies:
// - UnitCard.cs (for AvailableUnits list)
// - UnitDeck.cs (for Decks list)
// - UnitRarity.cs (for rarity tracking)

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegionsClone.Models.Units
{
    public class UnitInventory
    {
        public string InventoryId { get; set; }
        public List<UnitCard> AvailableUnits { get; set; }
        public List<UnitDeck> Decks { get; set; }
        public UnitDeck ActiveDeck { get; set; }
        
        // Inventory limits
        public int MaxUnitCapacity { get; set; } = 50;
        public int MaxDeckCapacity { get; set; } = 5;
        
        // Collection statistics
        public int TotalUnits => AvailableUnits.Count;
        public int CommonCount => AvailableUnits.Count(u => u.Rarity == UnitRarity.Common);
        public int RareCount => AvailableUnits.Count(u => u.Rarity == UnitRarity.Rare);
        public int EpicCount => AvailableUnits.Count(u => u.Rarity == UnitRarity.Epic);
        public int LegendaryCount => AvailableUnits.Count(u => u.Rarity == UnitRarity.Legendary);
        
        public UnitInventory()
        {
            InventoryId = Guid.NewGuid().ToString();
            AvailableUnits = new List<UnitCard>();
            Decks = new List<UnitDeck>();
            InitializeDefaultDeck();
        }
        
        private void InitializeDefaultDeck()
        {
            var defaultDeck = new UnitDeck("Starter Deck");
            Decks.Add(defaultDeck);
            ActiveDeck = defaultDeck;
        }
        
        public bool AddUnit(UnitCard unit)
        {
            if (AvailableUnits.Count >= MaxUnitCapacity)
            {
                Console.WriteLine($"Inventory full! Maximum {MaxUnitCapacity} units allowed.");
                return false;
            }
            
            AvailableUnits.Add(unit);
            Console.WriteLine($"Added {unit.UnitName} to inventory");
            return true;
        }
        
        public bool RemoveUnit(UnitCard unit)
        {
            bool removed = AvailableUnits.Remove(unit);
            
            // Also remove from all decks
            foreach (var deck in Decks)
            {
                deck.RemoveUnit(unit);
            }
            
            if (removed)
            {
                Console.WriteLine($"Removed {unit.UnitName} from inventory");
            }
            
            return removed;
        }
        
        public UnitCard FindUnitById(string unitId)
        {
            return AvailableUnits.FirstOrDefault(u => u.UnitId == unitId);
        }
        
        public List<UnitCard> FindUnitsByName(string name)
        {
            return AvailableUnits.Where(u => u.UnitName.ToLower().Contains(name.ToLower())).ToList();
        }
        
        public List<UnitCard> GetUnitsByRarity(UnitRarity rarity)
        {
            return AvailableUnits.Where(u => u.Rarity == rarity).ToList();
        }
        
        public List<UnitCard> GetUnitsByType(string unitType)
        {
            return AvailableUnits.Where(u => u.UnitName.ToLower().Contains(unitType.ToLower())).ToList();
        }
        
        public bool CreateNewDeck(string deckName)
        {
            if (Decks.Count >= MaxDeckCapacity)
            {
                Console.WriteLine($"Maximum {MaxDeckCapacity} decks allowed!");
                return false;
            }
            
            if (Decks.Any(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"Deck with name '{deckName}' already exists!");
                return false;
            }
            
            var newDeck = new UnitDeck(deckName);
            Decks.Add(newDeck);
            Console.WriteLine($"Created new deck: {deckName}");
            return true;
        }
        
        public bool DeleteDeck(string deckName)
        {
            var deck = Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
            if (deck == null)
            {
                Console.WriteLine($"Deck '{deckName}' not found!");
                return false;
            }
            
            if (deck == ActiveDeck)
            {
                Console.WriteLine("Cannot delete active deck! Switch to another deck first.");
                return false;
            }
            
            Decks.Remove(deck);
            Console.WriteLine($"Deleted deck: {deckName}");
            return true;
        }
        
        public bool SetActiveDeck(string deckName)
        {
            var deck = Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
            if (deck == null)
            {
                Console.WriteLine($"Deck '{deckName}' not found!");
                return false;
            }
            
            if (!deck.IsValid())
            {
                Console.WriteLine($"Deck '{deckName}' is not valid! Please check deck requirements.");
                return false;
            }
            
            ActiveDeck = deck;
            Console.WriteLine($"Activated deck: {deckName}");
            return true;
        }
        
        public bool AddUnitToDeck(UnitCard unit, string deckName = null)
        {
            var targetDeck = deckName == null ? ActiveDeck : 
                           Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
            
            if (targetDeck == null)
            {
                Console.WriteLine($"Deck '{deckName}' not found!");
                return false;
            }
            
            if (!AvailableUnits.Contains(unit))
            {
                Console.WriteLine($"Unit {unit.UnitName} not found in inventory!");
                return false;
            }
            
            return targetDeck.AddUnit(unit);
        }
        
        public bool RemoveUnitFromDeck(UnitCard unit, string deckName = null)
        {
            var targetDeck = deckName == null ? ActiveDeck : 
                           Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
            
            if (targetDeck == null)
            {
                Console.WriteLine($"Deck '{deckName}' not found!");
                return false;
            }
            
            return targetDeck.RemoveUnit(unit);
        }
        
        public List<UnitCard> GetUnitsNotInDeck(string deckName = null)
        {
            var targetDeck = deckName == null ? ActiveDeck : 
                           Decks.FirstOrDefault(d => d.DeckName.Equals(deckName, StringComparison.OrdinalIgnoreCase));
            
            if (targetDeck == null)
                return new List<UnitCard>();
                
            return AvailableUnits.Except(targetDeck.Units).ToList();
        }
        
        public string GetInventorySummary()
        {
            return $"""
            Inventory Summary:
            Total Units: {TotalUnits}/{MaxUnitCapacity}
            Common: {CommonCount} | Rare: {RareCount} | Epic: {EpicCount} | Legendary: {LegendaryCount}
            Decks: {Decks.Count}/{MaxDeckCapacity}
            Active Deck: {ActiveDeck?.DeckName ?? "None"}
            """;
        }
        
        public Dictionary<string, int> GetUnitTypeDistribution()
        {
            return AvailableUnits
                .GroupBy(u => u.UnitName.Split(' ').Last())
                .ToDictionary(g => g.Key, g => g.Count());
        }
        
        public Dictionary<UnitRarity, int> GetRarityDistribution()
        {
            return AvailableUnits
                .GroupBy(u => u.Rarity)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        
        public void UpgradeAllUnits(int levels = 1)
        {
            foreach (var unit in AvailableUnits)
            {
                for (int i = 0; i < levels; i++)
                {
                    unit.Stats.Upgrade(unit.Level + i);
                }
            }
            Console.WriteLine($"Upgraded all units by {levels} level(s)");
        }
        
        public void HealAllUnits()
        {
            foreach (var unit in AvailableUnits)
            {
                unit.Heal(unit.Stats.MaxHealth); // Full heal
            }
            Console.WriteLine("All units fully healed");
        }
        
        public int GetTotalCollectionValue()
        {
            return AvailableUnits.Sum(u => u.Stats.SilverCost + (u.Stats.GoldCost * 10));
        }
        
        public override string ToString()
        {
            return $"Inventory - {TotalUnits} units, {Decks.Count} decks, Active: {ActiveDeck?.DeckName}";
        }
    }
}