global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;

// Core Models - بناءً على الهيكل الفعلي
global using WarRegions.Core.Models;
global using WarRegions.Core.Models.Terrain;
global using WarRegions.Core.Models.Units;
global using WarRegions.Core.Models.Level;
global using WarRegions.Core.Models.Economy;
global using WarRegions.Core.Models.Development;

// Interfaces
global using WarRegions.Core.Interfaces;

// Controllers
global using WarRegions.Core.Controllers;
global using WarRegions.Core.Controllers.Pathfinding;
global using WarRegions.Core.Controllers.Economy;

// أنواع شائعة الاستخدام
global using TerrainTile = WarRegions.Core.Models.Terrain.TerrainTile;
global using MovementType = WarRegions.Core.Models.Units.MovementType;
global using UnitCard = WarRegions.Core.Models.Units.UnitCard;
global using UnitRarity = WarRegions.Core.Models.Units.UnitRarity;
global using Player = WarRegions.Core.Models.Player;
global using ShopItem = WarRegions.Core.Models.Economy.ShopItem;
global using Transaction = WarRegions.Core.Models.Economy.Transaction;
global using SpawnPoint = WarRegions.Core.Models.Level.SpawnPoint;