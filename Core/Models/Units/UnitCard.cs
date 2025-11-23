using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Models.Units
{
    // UnitCard.cs
    // Dependencies:
    // - UnitAttributes.cs (for Stats property)
    // - MovementType.cs (for MovementType property)
    // - UnitRarity.cs (for Rarity property)
    // - TerrainType.cs (for terrain effects)
    
    using System;
    
    namespace WarRegions.Models
    {
        public class UnitCard
        {
            public string UnitId { get; set; }
            public string UnitName { get; set; }
            public string Description { get; set; }
            public UnitRarity Rarity { get; set; }
            public MovementType MovementType { get; set; }
            public UnitAttributes Stats { get; set; }
            
            // Upgrade system
            public int Level { get; set; } = 1;
            public int Experience { get; set; }
            public int UpgradeCost { get; set; }
            
            // Battle state
            public bool IsAlive { get; set; } = true;
            public int CurrentHealth { get; set; }
            
            public UnitCard()
            {
                UnitId = Guid.NewGuid().ToString();
                Stats = new UnitAttributes();
                CurrentHealth = Stats.MaxHealth;
            }
            
            public UnitCard(string name, UnitRarity rarity, MovementType moveType) : this()
            {
                UnitName = name;
                Rarity = rarity;
                MovementType = moveType;
                SetBaseStats();
            }
            
            private void SetBaseStats()
            {
                // Base stats based on rarity and movement type
                int basePower = (int)Rarity * 10;
                
                Stats.Attack = basePower;
                Stats.Defense = basePower;
                Stats.MaxHealth = basePower * 2;
                Stats.Speed = 100; // Default speed
                
                // Adjust based on movement type
                switch (MovementType)
                {
                    case MovementType.Infantry:
                        Stats.Defense += 5;
                        break;
                    case MovementType.Cavalry:
                        Stats.Speed += 20;
                        break;
                    case MovementType.Archer:
                        Stats.Attack += 5;
                        Stats.Defense -= 2;
                        break;
                    case MovementType.Siege:
                        Stats.Attack += 10;
                        Stats.Speed -= 20;
                        break;
                    case MovementType.Naval:
                        Stats.MaxHealth += 10;
                        break;
                    case MovementType.Flying:
                        Stats.Speed += 30;
                        Stats.Defense -= 3;
                        break;
                }
                
                CurrentHealth = Stats.MaxHealth;
                UpgradeCost = (int)Rarity * 50;
            }
            
            public void TakeDamage(int damage)
            {
                CurrentHealth -= damage;
                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;
                    IsAlive = false;
                    Console.WriteLine($"{UnitName} has been defeated!");
                }
                else
                {
                    Console.WriteLine($"{UnitName} took {damage} damage. Health: {CurrentHealth}/{Stats.MaxHealth}");
                }
            }
            
            public void Heal(int amount)
            {
                CurrentHealth = Math.Min(CurrentHealth + amount, Stats.MaxHealth);
                Console.WriteLine($"{UnitName} healed {amount}. Health: {CurrentHealth}/{Stats.MaxHealth}");
            }
            
            public bool CanMoveOnTerrain(TerrainType terrain)
            {
                // Check if unit can move on specific terrain
                // This will be expanded when terrain system is implemented
                return true; // Temporary implementation
            }
            
            public override string ToString()
            {
                return $"{UnitName} (Lvl {Level}) - {Rarity} - HP: {CurrentHealth}/{Stats.MaxHealth}";
            }
        }
    }}
