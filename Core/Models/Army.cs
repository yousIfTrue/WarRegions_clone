// Army.cs
// Dependencies:
// - Player.cs (for Owner property)
// - Units/UnitCard.cs (for Units list)
// - Region.cs (for CurrentRegion property)

using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegionsClone.Models
{
    public class Army
    {
        public string ArmyId { get; set; }
        public string ArmyName { get; set; }
        public Player Owner { get; set; }
        public List<UnitCard> Units { get; set; }
        public Region CurrentRegion { get; set; }
        
        // Army state
        public bool IsMoving { get; set; }
        public int MovementPoints { get; set; }
        public int MaxMovementPoints { get; set; } = 10;
        
        // Battle modifiers
        public double Morale { get; set; } = 1.0;
        public double Supply { get; set; } = 1.0;
        
        public Army()
        {
            ArmyId = Guid.NewGuid().ToString();
            Units = new List<UnitCard>();
            MovementPoints = MaxMovementPoints;
        }
        
        public Army(Player owner, string name) : this()
        {
            Owner = owner;
            ArmyName = name;
        }
        
        public void AddUnit(UnitCard unit)
        {
            Units.Add(unit);
            Console.WriteLine($"Added {unit.UnitName} to army {ArmyName}");
        }
        
        public void RemoveUnit(UnitCard unit)
        {
            Units.Remove(unit);
            Console.WriteLine($"Removed {unit.UnitName} from army {ArmyName}");
        }
        
        public bool CanMoveTo(Region targetRegion)
        {
            if (targetRegion == null) return false;
            if (!targetRegion.CanBeEnteredBy(this)) return false;
            
            // Check movement points
            int movementCost = targetRegion.CalculateMovementCost(GetSlowestMovementType());
            return MovementPoints >= movementCost;
        }
        
        public void MoveTo(Region targetRegion)
        {
            if (!CanMoveTo(targetRegion))
            {
                Console.WriteLine($"Cannot move to {targetRegion.RegionName}");
                return;
            }
            
            int movementCost = targetRegion.CalculateMovementCost(GetSlowestMovementType());
            MovementPoints -= movementCost;
            
            // Leave current region
            if (CurrentRegion != null)
            {
                CurrentRegion.OccupyingArmy = null;
            }
            
            // Enter new region
            CurrentRegion = targetRegion;
            targetRegion.OccupyingArmy = this;
            
            Console.WriteLine($"Army {ArmyName} moved to {targetRegion.RegionName} (Cost: {movementCost}, MP left: {MovementPoints})");
            
            // If region has enemy owner, capture it
            if (targetRegion.Owner != Owner && targetRegion.Owner != null)
            {
                targetRegion.Capture(Owner);
            }
        }
        
        public void ResetMovementPoints()
        {
            MovementPoints = MaxMovementPoints;
            Console.WriteLine($"Army {ArmyName} movement points reset to {MaxMovementPoints}");
        }
        
        public MovementType GetSlowestMovementType()
        {
            if (Units.Count == 0) return MovementType.Infantry;
            
            return Units.Select(u => u.MovementType)
                       .OrderBy(mt => (int)mt)
                       .First();
        }
        
        public int GetTotalAttackPower()
        {
            return Units.Where(u => u.IsAlive)
                       .Sum(u => u.Stats.Attack);
        }
        
        public int GetTotalDefensePower()
        {
            return Units.Where(u => u.IsAlive)
                       .Sum(u => u.Stats.Defense);
        }
        
        public int GetAliveUnitCount()
        {
            return Units.Count(u => u.IsAlive);
        }
        
        public void TakeCasualties(int damage)
        {
            if (Units.Count == 0) return;
            
            // Simple damage distribution - in real implementation, use more complex logic
            int damagePerUnit = Math.Max(1, damage / GetAliveUnitCount());
            
            foreach (var unit in Units.Where(u => u.IsAlive))
            {
                unit.TakeDamage(damagePerUnit);
            }
            
            // Update morale based on casualties
            double casualtyRate = (double)(Units.Count - GetAliveUnitCount()) / Units.Count;
            Morale = Math.Max(0.1, 1.0 - casualtyRate);
        }
        
        public override string ToString()
        {
            return $"{ArmyName} - Units: {GetAliveUnitCount()}/{Units.Count} - MP: {MovementPoints}/{MaxMovementPoints}";
        }
    }
}