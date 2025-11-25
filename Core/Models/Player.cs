using System;
using System.Collections.Generic;
using System.Linq;
using WarRegions.Core.Models.Units;    // ✅ أضفناها للـ UnitCard
// namespace: WarRegions.Core.Models

namespace WarRegions.Core.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int SilverCoins { get; set; }
        public int GoldCoins { get; set; }
        public List<UnitCard> AvailableUnits { get; set; }
        public UnitDeck CurrentDeck { get; set; }

        public Player(string name)
        {
            Name = name;
            SilverCoins = 0;
            GoldCoins = 0;
            AvailableUnits = new List<UnitCard>();
            CurrentDeck = new UnitDeck();
        }
    }
}
