// Core/Models/Army.cs
namespace WarRegions.Core.Models
{
    public class Army
    {
        public string ArmyName { get; set; } = "Unnamed Army";
        public List<UnitCard> Units { get; set; } = new List<UnitCard>();
        public int Supply { get; set; } = 100;
        public int Morale { get; set; } = 100;
        public Player Owner { get; set; }
        public int Experience { get; set; } = 0;
        public bool IsDefeated => !Units.Any(u => u.IsAlive);

        public Army() { }

        public Army(string name, Player owner)
        {
            ArmyName = name;
            Owner = owner;
        }

        // طرق أساسية
        public int GetAliveUnitCount() => Units.Count(u => u.IsAlive);
        public int GetTotalUnitCount() => Units.Count;
        public double GetStrength() => Units.Sum(u => u.GetCombatStrength());

        public void AddUnit(UnitCard unit)
        {
            Units.Add(unit);
        }

        public void RemoveUnit(UnitCard unit)
        {
            Units.Remove(unit);
        }

        public void HealUnits(int healAmount)
        {
            foreach (var unit in Units)
            {
                // سيتم تطبيق العلاج عندما نضيف نظام الصحة
            }
        }

        public void RestoreSupply()
        {
            Supply = 100;
        }

        public override string ToString()
        {
            return $"{ArmyName} ({GetAliveUnitCount()}/{GetTotalUnitCount()} units)";
        }
    }
}