using System;
using System.Collections.Generic;
using System.Linq;
using WarRegions.Core.Models.Units;    // ✅ أضفناها للـ UnitCard
// namespace: WarRegions.Core.Models
namespace WarRegions.Core.Models
{
    public class Army
    {
        public Player Owner { get; set; }
        public List<UnitCard> Units { get; set; }
        public Region CurrentRegion { get; set; }

        public Army(Player owner)
        {
            Owner = owner;
            Units = new List<UnitCard>();
            CurrentRegion = null;
        }
    }
}
