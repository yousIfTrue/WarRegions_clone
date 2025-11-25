using System;
using System.Collections.Generic;
using System.Linq;
    
    namespace WarRegions.Core.Models.Level
    {
            // Core/Models/Level/SpawnPoint.cs
    // Dependencies:
    // - Units/UnitCard.cs (for unit creation)
        public class SpawnPoint
        {
            public string SpawnId { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string UnitType { get; set; }
            public string UnitVariant { get; set; }
            public int UnitLevel { get; set; } = 1;
            
            // Spawn conditions
            public int SpawnTurn { get; set; } = 1;
            public bool IsReinforcement { get; set; } = false;
            public string RequiredCondition { get; set; } // e.g., "player_losing", "turn_5"
            
            // Unit customization
            public int CustomHealth { get; set; } = 0;
            public int CustomAttack { get; set; } = 0;
            public int CustomDefense { get; set; } = 0;
            
            public SpawnPoint()
            {
                SpawnId = Guid.NewGuid().ToString();
            }
            
            public SpawnPoint(int x, int y, string unitType) : this()
            {
                X = x;
                Y = y;
                UnitType = unitType;
            }
            
            public SpawnPoint(int x, int y, string unitType, int spawnTurn, bool isReinforcement = false) : this(x, y, unitType)
            {
                SpawnTurn = spawnTurn;
                IsReinforcement = isReinforcement;
            }
            
            public UnitCard CreateUnit(Player owner)
            {
                // Determine rarity based on level and type
                UnitRarity rarity = DetermineRarity();
                MovementType movementType = DetermineMovementType();
                
                // Create the unit
                var unit = new UnitCard(GetUnitName(), rarity, movementType);
                unit.Owner = owner;
                
                // Apply custom stats if specified
                if (CustomHealth > 0) unit.Stats.MaxHealth = CustomHealth;
                if (CustomAttack > 0) unit.Stats.Attack = CustomAttack;
                if (CustomDefense > 0) unit.Stats.Defense = CustomDefense;
                
                // Set level
                unit.Level = UnitLevel;
                if (UnitLevel > 1)
                {
                    unit.Stats.Upgrade(UnitLevel - 1);
                }
                
                // Apply variant-specific modifications
                ApplyVariantModifications(unit);
                
                Console.WriteLine($"Spawned {unit.UnitName} at ({X}, {Y}) for {owner.PlayerName}");
                
                return unit;
            }
            
            private UnitRarity DetermineRarity()
            {
                return UnitLevel switch
                {
                    1 => UnitRarity.Common,
                    2 => UnitRarity.Common,
                    3 => UnitRarity.Rare,
                    4 => UnitRarity.Rare,
                    5 => UnitRarity.Epic,
                    _ => UnitRarity.Legendary
                };
            }
            
            private MovementType DetermineMovementType()
            {
                return UnitType.ToLower() switch
                {
                    "infantry" => MovementType.Infantry,
                    "archer" => MovementType.Archer,
                    "cavalry" => MovementType.Cavalry,
                    "siege" => MovementType.Siege,
                    "naval" => MovementType.Naval,
                    "flying" => MovementType.Flying,
                    _ => MovementType.Infantry
                };
            }
            
            private string GetUnitName()
            {
                string baseName = UnitType switch
                {
                    "infantry" => "Foot Soldier",
                    "archer" => "Archer",
                    "cavalry" => "Cavalry Knight", 
                    "siege" => "Catapult",
                    "naval" => "Warship",
                    "flying" => "Griffin Rider",
                    _ => "Militia"
                };
                
                if (!string.IsNullOrEmpty(UnitVariant))
                {
                    baseName = $"{UnitVariant} {baseName}";
                }
                
                return baseName;
            }
            
            private void ApplyVariantModifications(UnitCard unit)
            {
                if (string.IsNullOrEmpty(UnitVariant))
                    return;
                    
                switch (UnitVariant.ToLower())
                {
                    case "elite":
                        unit.Stats.Attack += 5;
                        unit.Stats.Defense += 5;
                        unit.Stats.MaxHealth += 10;
                        break;
                        
                    case "veteran":
                        unit.Stats.Attack += 3;
                        unit.Stats.Defense += 3;
                        unit.Stats.MaxHealth += 5;
                        unit.Experience = 50; // Start with some experience
                        break;
                        
                    case "heavy":
                        unit.Stats.Defense += 8;
                        unit.Stats.MaxHealth += 15;
                        unit.Stats.Speed -= 20;
                        break;
                        
                    case "light":
                        unit.Stats.Speed += 30;
                        unit.Stats.Attack += 2;
                        unit.Stats.Defense -= 3;
                        break;
                        
                    case "royal":
                        unit.Stats.Attack += 8;
                        unit.Stats.Defense += 6;
                        unit.Stats.MaxHealth += 12;
                        unit.Stats.Speed += 10;
                        break;
                }
            }
            
            public bool ShouldSpawn(int currentTurn, GameState gameState)
            {
                if (currentTurn < SpawnTurn)
                    return false;
                    
                if (string.IsNullOrEmpty(RequiredCondition))
                    return true;
                    
                // Check conditional spawn requirements
                return RequiredCondition.ToLower() switch
                {
                    "player_losing" => IsPlayerLosing(gameState),
                    "player_winning" => IsPlayerWinning(gameState),
                    "turn_5" => currentTurn >= 5,
                    "turn_10" => currentTurn >= 10,
                    "half_units_lost" => HasHalfUnitsLost(gameState),
                    _ => true
                };
            }
            
            private bool IsPlayerLosing(GameState gameState)
            {
                var playerUnits = gameState.Armies
                    .Where(a => a.Owner == gameState.CurrentPlayer)
                    .Sum(a => a.GetAliveUnitCount());
                    
                var enemyUnits = gameState.Armies
                    .Where(a => a.Owner != gameState.CurrentPlayer)
                    .Sum(a => a.GetAliveUnitCount());
                    
                return playerUnits < enemyUnits * 0.7; // Player has less than 70% of enemy units
            }
            
            private bool IsPlayerWinning(GameState gameState)
            {
                var playerUnits = gameState.Armies
                    .Where(a => a.Owner == gameState.CurrentPlayer)
                    .Sum(a => a.GetAliveUnitCount());
                    
                var enemyUnits = gameState.Armies
                    .Where(a => a.Owner != gameState.CurrentPlayer)
                    .Sum(a => a.GetAliveUnitCount());
                    
                return playerUnits > enemyUnits * 1.3; // Player has more than 130% of enemy units
            }
            
            private bool HasHalfUnitsLost(GameState gameState)
            {
                var initialUnits = 6; // This should come from level configuration
                var currentUnits = gameState.Armies
                    .Where(a => a.Owner == gameState.CurrentPlayer)
                    .Sum(a => a.GetAliveUnitCount());
                    
                return currentUnits <= initialUnits / 2;
            }
            
            public override string ToString()
            {
                string reinforcement = IsReinforcement ? " (Reinforcement)" : "";
                string condition = !string.IsNullOrEmpty(RequiredCondition) ? $" [{RequiredCondition}]" : "";
                return $"{UnitType} at ({X},{Y}) - Turn {SpawnTurn}{reinforcement}{condition}";
            }
            
            public string ToShortString()
            {
                return $"{UnitType}@({X},{Y})";
            }
        }
    }
