// Core/Models/Development/DefaultUnits.cs
// Dependencies:
// - Units/UnitCard.cs
// - Units/UnitAttributes.cs
// - Units/MovementType.cs
// - Units/UnitRarity.cs

using System.Collections.Generic;
using WarRegions.Core.Models.Units;

namespace WarRegions.Models.Development
{
    public static class DefaultUnits
    {
        public static List<UnitCard> CreateStarterUnits()
        {
            var units = new List<UnitCard>();
            
            // Basic infantry unit
            var infantry = new UnitCard("Foot Soldier", UnitRarity.Common, MovementType.Infantry);
            infantry.Stats = UnitAttributes.CreateForType("infantry");
            infantry.Description = "A reliable infantry unit good for holding positions.";
            units.Add(infantry);
            
            // Archer unit
            var archer = new UnitCard("Archer", UnitRarity.Common, MovementType.Archer);
            archer.Stats = UnitAttributes.CreateForType("archer");
            archer.Description = "Ranged unit that can attack from distance.";
            units.Add(archer);
            
            // Cavalry unit
            var cavalry = new UnitCard("Cavalry Knight", UnitRarity.Rare, MovementType.Cavalry);
            cavalry.Stats = UnitAttributes.CreateForType("cavalry");
            cavalry.Description = "Fast moving unit excellent for flanking maneuvers.";
            units.Add(cavalry);
            
            // Siege unit
            var siege = new UnitCard("Catapult", UnitRarity.Rare, MovementType.Siege);
            siege.Stats = UnitAttributes.CreateForType("siege");
            siege.Description = "Heavy artillery unit with long range but slow movement.";
            units.Add(siege);
            
            // Naval unit
            var naval = new UnitCard("Warship", UnitRarity.Epic, MovementType.Naval);
            naval.Stats = UnitAttributes.CreateForType("naval");
            naval.Description = "Powerful naval unit that dominates river territories.";
            units.Add(naval);
            
            // Flying unit
            var flying = new UnitCard("Griffin Rider", UnitRarity.Epic, MovementType.Flying);
            flying.Stats = UnitAttributes.CreateForType("flying");
            flying.Description = "Elite flying unit that ignores terrain obstacles.";
            units.Add(flying);
            
            return units;
        }
        
        public static List<UnitCard> CreateAdvancedUnits()
        {
            var units = new List<UnitCard>();
            
            // Elite infantry
            var eliteInfantry = new UnitCard("Royal Guard", UnitRarity.Epic, MovementType.Infantry);
            eliteInfantry.Stats = UnitAttributes.CreateForType("infantry");
            eliteInfantry.Stats.Attack += 10;
            eliteInfantry.Stats.Defense += 15;
            eliteInfantry.Stats.MaxHealth += 20;
            eliteInfantry.Level = 3;
            eliteInfantry.Description = "Elite infantry unit with superior defense.";
            units.Add(eliteInfantry);
            
            // Master archer
            var masterArcher = new UnitCard("Master Archer", UnitRarity.Epic, MovementType.Archer);
            masterArcher.Stats = UnitAttributes.CreateForType("archer");
            masterArcher.Stats.Attack += 15;
            masterArcher.Stats.Range += 1;
            masterArcher.Level = 3;
            masterArcher.Description = "Expert archer with extended range and precision.";
            units.Add(masterArcher);
            
            // Heavy cavalry
            var heavyCavalry = new UnitCard("Heavy Knight", UnitRarity.Epic, MovementType.Cavalry);
            heavyCavalry.Stats = UnitAttributes.CreateForType("cavalry");
            heavyCavalry.Stats.Attack += 12;
            heavyCavalry.Stats.Defense += 10;
            heavyCavalry.Stats.Speed -= 20; // Slower but stronger
            heavyCavalry.Level = 3;
            heavyCavalry.Description = "Heavily armored cavalry for breaking enemy lines.";
            units.Add(heavyCavalry);
            
            return units;
        }
        
        public static List<UnitCard> CreateLegendaryUnits()
        {
            var units = new List<UnitCard>();
            
            // Dragon unit
            var dragon = new UnitCard("Ancient Dragon", UnitRarity.Legendary, MovementType.Flying);
            dragon.Stats.Attack = 50;
            dragon.Stats.Defense = 30;
            dragon.Stats.MaxHealth = 100;
            dragon.Stats.Speed = 180;
            dragon.Stats.Range = 2;
            dragon.Stats.SilverCost = 1000;
            dragon.Stats.GoldCost = 200;
            dragon.Level = 5;
            dragon.Description = "Ancient dragon of immense power. Can devastate entire armies.";
            units.Add(dragon);
            
            // Titan unit
            var titan = new UnitCard("Stone Titan", UnitRarity.Legendary, MovementType.Siege);
            titan.Stats.Attack = 40;
            titan.Stats.Defense = 50;
            titan.Stats.MaxHealth = 150;
            titan.Stats.Speed = 40;
            titan.Stats.Range = 3;
            titan.Stats.SilverCost = 1200;
            titan.Stats.GoldCost = 250;
            titan.Level = 5;
            titan.Description = "Massive stone titan that can destroy fortifications with ease.";
            units.Add(titan);
            
            // Phoenix unit
            var phoenix = new UnitCard("Phoenix", UnitRarity.Legendary, MovementType.Flying);
            phoenix.Stats.Attack = 35;
            phoenix.Stats.Defense = 25;
            phoenix.Stats.MaxHealth = 80;
            phoenix.Stats.Speed = 200;
            phoenix.Stats.Range = 1;
            phoenix.Stats.SilverCost = 900;
            phoenix.Stats.GoldCost = 180;
            phoenix.Level = 5;
            phoenix.Description = "Mythical bird that can resurrect once per battle.";
            units.Add(phoenix);
            
            return units;
        }
        
        public static UnitCard CreateUnitByType(string unitType, UnitRarity rarity = UnitRarity.Common, int level = 1)
        {
            MovementType movementType = unitType.ToLower() switch
            {
                "infantry" => MovementType.Infantry,
                "archer" => MovementType.Archer,
                "cavalry" => MovementType.Cavalry,
                "siege" => MovementType.Siege,
                "naval" => MovementType.Naval,
                "flying" => MovementType.Flying,
                _ => MovementType.Infantry
            };
            
            string unitName = GetUnitNameByType(unitType, rarity);
            
            var unit = new UnitCard(unitName, rarity, movementType);
            unit.Stats = UnitAttributes.CreateForType(unitType, (int)rarity + 1);
            unit.Level = level;
            
            // Apply level upgrades
            if (level > 1)
            {
                unit.Stats.Upgrade(level - 1);
            }
            
            unit.Description = GetUnitDescriptionByType(unitType, rarity);
            
            return unit;
        }
        
        private static string GetUnitNameByType(string unitType, UnitRarity rarity)
        {
            string baseName = unitType.ToLower() switch
            {
                "infantry" => "Soldier",
                "archer" => "Archer",
                "cavalry" => "Knight",
                "siege" => "Catapult",
                "naval" => "Warship",
                "flying" => "Griffin",
                _ => "Militia"
            };
            
            string prefix = rarity switch
            {
                UnitRarity.Common => "",
                UnitRarity.Rare => "Veteran ",
                UnitRarity.Epic => "Elite ",
                UnitRarity.Legendary => "Ancient ",
                _ => ""
            };
            
            return prefix + baseName;
        }
        
        private static string GetUnitDescriptionByType(string unitType, UnitRarity rarity)
        {
            string baseDescription = unitType.ToLower() switch
            {
                "infantry" => "Reliable ground unit for frontline combat.",
                "archer" => "Ranged unit that attacks from a distance.",
                "cavalry" => "Fast unit excellent for flanking and chasing.",
                "siege" => "Heavy unit for destroying fortifications.",
                "naval" => "Specialized unit for river combat.",
                "flying" => "Unit that ignores terrain obstacles.",
                _ => "Basic military unit."
            };
            
            string rarityDescription = rarity switch
            {
                UnitRarity.Common => " A standard unit.",
                UnitRarity.Rare => " An improved unit with better stats.",
                UnitRarity.Epic => " An elite unit with superior capabilities.",
                UnitRarity.Legendary => " A legendary unit of immense power.",
                _ => ""
            };
            
            return baseDescription + rarityDescription;
        }
        
        public static void AddDefaultUnitsToPlayer(Player player)
        {
            if (DevConfig.AllUnitsUnlocked)
            {
                // Add all unit types for testing
                var allUnits = CreateStarterUnits();
                allUnits.AddRange(CreateAdvancedUnits());
                allUnits.AddRange(CreateLegendaryUnits());
                
                foreach (var unit in allUnits)
                {
                    player.AvailableUnits.Add(unit);
                }
                
                Console.WriteLine($"[DEBUG] Added {allUnits.Count} default units to player");
            }
            else
            {
                // Add only starter units
                var starterUnits = CreateStarterUnits();
                foreach (var unit in starterUnits)
                {
                    player.AvailableUnits.Add(unit);
                }
                
                Console.WriteLine($"[DEBUG] Added {starterUnits.Count} starter units to player");
            }
            
            // Add some units to the default deck
            if (player.CurrentDeck != null)
            {
                foreach (var unit in player.AvailableUnits.Take(4))
                {
                    player.CurrentDeck.AddUnit(unit);
                }
            }
        }
        
        public static string GetUnitCatalog()
        {
            var catalog = "=== Unit Catalog ===\n";
            
            var starterUnits = CreateStarterUnits();
            catalog += "Starter Units:\n";
            foreach (var unit in starterUnits)
            {
                catalog += $"- {unit.UnitName} ({unit.Rarity}): {unit.Stats.ToShortString()}\n";
            }
            
            var advancedUnits = CreateAdvancedUnits();
            catalog += "\nAdvanced Units:\n";
            foreach (var unit in advancedUnits)
            {
                catalog += $"- {unit.UnitName} ({unit.Rarity}): {unit.Stats.ToShortString()}\n";
            }
            
            var legendaryUnits = CreateLegendaryUnits();
            catalog += "\nLegendary Units:\n";
            foreach (var unit in legendaryUnits)
            {
                catalog += $"- {unit.UnitName} ({unit.Rarity}): {unit.Stats.ToShortString()}\n";
            }
            
            return catalog;
        }
    }
