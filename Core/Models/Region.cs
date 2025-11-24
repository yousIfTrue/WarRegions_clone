using System;
using WarRegions.Core.Models.Terrain;
using System.Collections.Generic;
using System.Linq;
    
    namespace WarRegions.Models
    {
            // Region.cs
    // Dependencies:
    // - Terrain/TerrainType.cs (for Terrain property)
    // - Army.cs (for OccupyingArmy property)
    // - Player.cs (for Owner property)
        public class Region
        {
            public string RegionId { get; set; }
            public string RegionName { get; set; }
            public int X { get; set; }  // Grid position
            public int Y { get; set; }  // Grid position
            public TerrainType Terrain { get; set; }
            
            // Ownership and control
            public Player Owner { get; set; }
            public Army OccupyingArmy { get; set; }
            // Resources and economy
            public int SilverProduction { get; set; }
            public int GoldProduction { get; set; }
            public bool HasWorkshop { get; set; }
            public bool HasMarket { get; set; }
            // Strategic value
            public int DefenseBonus { get; set; }
            public bool IsPassable { get; set; }
            public List<Region> ConnectedRegions { get; set; }
            // Visual representation for CLI
            public char MapSymbol { get; set; }
            public ConsoleColor MapColor { get; set; }
            public Region()
            {
                RegionId = Guid.NewGuid().ToString();
                ConnectedRegions = new List<Region>();
                IsPassable = true;
                MapSymbol = '■';
                MapColor = ConsoleColor.Gray;
            }
            public Region(string name, int x, int y, TerrainType terrain) : this()
            {
                RegionName = name;
                X = x;
                Y = y;
                Terrain = terrain;
                SetTerrainProperties();
            }
            private void SetTerrainProperties()
            {
                switch (Terrain)
                {
                    case TerrainType.Plains:
                        SilverProduction = 10;
                        GoldProduction = 1;
                        MapSymbol = '▒';
                        MapColor = ConsoleColor.Green;
                        DefenseBonus = 0;
                        break;
                        
                    case TerrainType.Mountains:
                        SilverProduction = 5;
                        GoldProduction = 3;
                        MapSymbol = '▲';
                        MapColor = ConsoleColor.DarkGray;
                        DefenseBonus = 5;
                        IsPassable = false; // Mountains block movement
                        break;
                    case TerrainType.Forest:
                        SilverProduction = 8;
                        GoldProduction = 2;
                        MapSymbol = '♣';
                        MapColor = ConsoleColor.DarkGreen;
                        DefenseBonus = 2;
                        break;
                    case TerrainType.River:
                        SilverProduction = 6;
                        MapSymbol = '≈';
                        MapColor = ConsoleColor.Blue;
                        DefenseBonus = -1;
                        break;
                    case TerrainType.Desert:
                        SilverProduction = 4;
                        GoldProduction = 0;
                        MapSymbol = '░';
                        MapColor = ConsoleColor.Yellow;
                        DefenseBonus = -2;
                        break;
                    case TerrainType.Swamp:
                        SilverProduction = 3;
                        MapSymbol = '~';
                        DefenseBonus = -3;
                        break;
                    case TerrainType.Fortress:
                        SilverProduction = 2;
                        GoldProduction = 5;
                        MapSymbol = '⌂';
                        MapColor = ConsoleColor.Red;
                        DefenseBonus = 10;
                        break;
                }
            }
            public void ConnectTo(Region otherRegion)
            {
                if (!ConnectedRegions.Contains(otherRegion))
                {
                    ConnectedRegions.Add(otherRegion);
                    otherRegion.ConnectedRegions.Add(this);
                }
            }
            public bool CanBeEnteredBy(Army army)
            {
                if (!IsPassable) return false;
                
                // Check if region is occupied by enemy
                if (OccupyingArmy != null && OccupyingArmy.Owner != army.Owner)
                {
                    return false; // Region occupied by enemy
                }
                return true;
            }
            public void Capture(Player newOwner)
            {
                Owner = newOwner;
                Console.WriteLine($"{RegionName} captured by {newOwner.PlayerName}!");
            }
            public int CalculateMovementCost(MovementType movementType)
            {
                int baseCost = 1;
    
                switch (Terrain)  // ✅ نتحقق من نوع التضاريس
                {
                    case TerrainType.Mountains:
                        // ⛰️ الجبال: الطيران فقط ممكن، الباقي مستحيل
                        return movementType == MovementType.Flying ? baseCost : 99;
            
                    case TerrainType.River:
                        // 🌊 النهر: السفن فقط سريعة، الباقي بطيء
                        return movementType == MovementType.Naval ? baseCost : baseCost * 3;
            
                    case TerrainType.Forest:
                        // 🌲 الغابة: المشاة سريعون، الباقي بطيء
                        return movementType == MovementType.Infantry ? baseCost : baseCost * 2;
            
                    case TerrainType.Desert:
                        // 🏜️ الصحراء: الخيالة بطيئون، الباقي متوسط
                        return movementType == MovementType.Cavalry ? baseCost * 2 : (int)(baseCost * 1.5);
            
                    case TerrainType.Swamp:
                    // 🐊 المستنقع: الطيران فقط سريع، الباقي بطيء جداً
                        return movementType == MovementType.Flying ? baseCost : baseCost * 4;
            
                    default:
                        // 🟦 باقي التضاريس: تكلفة عادية
                        return baseCost;
                }
            }
            public override string ToString()
            {
                string ownerName = Owner?.PlayerName ?? "Neutral";
                string armyInfo = OccupyingArmy?.ToString() ?? "No army";
                return $"{RegionName} ({X},{Y}) - {Terrain} - Owner: {ownerName} - {armyInfo}";
            }
        }
    }
