using System;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Models.Units
{
    // Core/Models/Units/UnitAttributes.cs
    // Dependencies: None - this is a basic data structure
    
    using System;
    
    namespace WarRegionsClone.Models.Units
    {
        public class UnitAttributes
        {
            // Core combat stats
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int MaxHealth { get; set; }
            public int Speed { get; set; } // Affects turn order
            
            // Special abilities
            public int Range { get; set; } = 1; // Attack range
            public int Vision { get; set; } = 2; // How far the unit can see
            
            // Resource costs
            public int SilverCost { get; set; }
            public int GoldCost { get; set; }
            public int UpkeepCost { get; set; } // Cost per turn
            
            // Experience and leveling
            public int ExperienceValue { get; set; } // XP gained when defeating this unit
            
            public UnitAttributes()
            {
                // Default values for a basic unit
                Attack = 10;
                Defense = 10;
                MaxHealth = 20;
                Speed = 100;
                SilverCost = 100;
                GoldCost = 10;
                UpkeepCost = 5;
                ExperienceValue = 10;
            }
            
            public UnitAttributes(int attack, int defense, int health, int speed) : this()
            {
                Attack = attack;
                Defense = defense;
                MaxHealth = health;
                Speed = speed;
            }
            
            // Method to create attributes based on unit type and rarity
            public static UnitAttributes CreateForType(string unitType, int rarityMultiplier = 1)
            {
                var attributes = new UnitAttributes();
                
                switch (unitType.ToLower())
                {
                    case "infantry":
                        attributes.Attack = 12 * rarityMultiplier;
                        attributes.Defense = 15 * rarityMultiplier;
                        attributes.MaxHealth = 25 * rarityMultiplier;
                        attributes.Speed = 80;
                        attributes.SilverCost = 80 * rarityMultiplier;
                        break;
                        
                    case "archer":
                        attributes.Attack = 15 * rarityMultiplier;
                        attributes.Defense = 8 * rarityMultiplier;
                        attributes.MaxHealth = 18 * rarityMultiplier;
                        attributes.Range = 2;
                        attributes.Speed = 90;
                        attributes.SilverCost = 120 * rarityMultiplier;
                        break;
                        
                    case "cavalry":
                        attributes.Attack = 18 * rarityMultiplier;
                        attributes.Defense = 12 * rarityMultiplier;
                        attributes.MaxHealth = 22 * rarityMultiplier;
                        attributes.Speed = 120;
                        attributes.SilverCost = 150 * rarityMultiplier;
                        break;
                        
                    case "siege":
                        attributes.Attack = 25 * rarityMultiplier;
                        attributes.Defense = 5 * rarityMultiplier;
                        attributes.MaxHealth = 30 * rarityMultiplier;
                        attributes.Range = 3;
                        attributes.Speed = 40;
                        attributes.SilverCost = 200 * rarityMultiplier;
                        break;
                        
                    case "naval":
                        attributes.Attack = 14 * rarityMultiplier;
                        attributes.Defense = 16 * rarityMultiplier;
                        attributes.MaxHealth = 28 * rarityMultiplier;
                        attributes.Speed = 110;
                        attributes.SilverCost = 180 * rarityMultiplier;
                        break;
                        
                    case "flying":
                        attributes.Attack = 16 * rarityMultiplier;
                        attributes.Defense = 10 * rarityMultiplier;
                        attributes.MaxHealth = 20 * rarityMultiplier;
                        attributes.Speed = 150;
                        attributes.SilverCost = 220 * rarityMultiplier;
                        break;
                }
                
                attributes.GoldCost = attributes.SilverCost / 10;
                attributes.ExperienceValue = attributes.Attack + attributes.Defense;
                
                return attributes;
            }
            
            public void Upgrade(int level)
            {
                // Improve stats based on upgrade level
                double multiplier = 1.0 + (level * 0.1); // 10% improvement per level
                
                Attack = (int)(Attack * multiplier);
                Defense = (int)(Defense * multiplier);
                MaxHealth = (int)(MaxHealth * multiplier);
                Speed = (int)(Speed * (1.0 + (level * 0.05))); // 5% speed improvement
                
                Console.WriteLine($"Unit attributes upgraded to level {level}");
            }
            
            public int CalculateDamageAgainst(UnitAttributes defender)
            {
                // Simple damage calculation
                int baseDamage = Attack - defender.Defense;
                return Math.Max(1, baseDamage); // Minimum 1 damage
            }
            
            public override string ToString()
            {
                return $"ATK: {Attack} | DEF: {Defense} | HP: {MaxHealth} | SPD: {Speed} | RNG: {Range}";
            }
            
            public string ToShortString()
            {
                return $"{Attack}A/{Defense}D/{MaxHealth}HP";
            }
        }
    }}
