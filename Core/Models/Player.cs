using System;
using WarRegions.Core.Models.Units;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Models
{
    // Player.cs
    // Dependencies: 
    // - UnitCard.cs (for AvailableUnits)
    // - UnitDeck.cs (for CurrentDeck)
    // - Currency.cs (for SilverCoins, GoldCoins)
    
    using System;
    using System.Collections.Generic;
    namespace WarRegions.Models
    {
        public class Player
        {
            public string PlayerId { get; set; }
            public string PlayerName { get; set; }
            public List<UnitCard> AvailableUnits { get; set; }
            public UnitDeck CurrentDeck { get; set; }
            
            // Economy - default values for development
            public int SilverCoins { get; set; } = 1000;
            public int GoldCoins { get; set; } = 100;
            // Game progress
            public int LevelProgress { get; set; } = 1;
            public int TotalBattles { get; set; }
            public int BattlesWon { get; set; }
            // Development flags
            public bool ShopEnabled { get; set; } = false;
            public DateTime LastShopRefresh { get; set; }
            public Player()
            {
                PlayerId = Guid.NewGuid().ToString();
                PlayerName = "Player_" + DateTime.Now.ToString("yyyyMMdd");
                AvailableUnits = new List<UnitCard>();
                CurrentDeck = new UnitDeck();
            }
            public void AddUnitToDeck(UnitCard unit)
                if (CurrentDeck.Units.Count < CurrentDeck.MaxDeckSize)
                {
                    CurrentDeck.Units.Add(unit);
                    Console.WriteLine($"Added {unit.UnitName} to deck");
                }
                else
                    Console.WriteLine("Deck is full! Cannot add more units");
            public bool CanAfford(int silverCost, int goldCost = 0)
                return SilverCoins >= silverCost && GoldCoins >= goldCost;
            public void SpendCoins(int silver, int gold = 0)
                SilverCoins -= silver;
                GoldCoins -= gold;
                Console.WriteLine($"Spent {silver} silver, {gold} gold. Remaining: {SilverCoins}S {GoldCoins}G");
        }
    }}
