// CLI/GlobalUsings.cs
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;

// استيراد جميع الـ namespaces من Core مع جميع الاختلافات المحتملة
global using WarRegions.Core.Models;
global using WarRegions.Core.Models.Terrain;
global using WarRegions.Core.Models.Units;
global using WarRegions.Core.Models.Level;
global using WarRegions.Core.Models.Economy;
global using WarRegions.Models;
global using WarRegions.Models.Terrain;
global using WarRegions.Models.Units;
global using WarRegions.Models.Level;
global using WarRegions.Models.Economy;

global using WarRegions.Core.Interfaces;
global using WarRegions.Interfaces;

global using WarRegions.Core.Controllers;
global using WarRegions.Core.Controllers.Pathfinding;
global using WarRegions.Core.Controllers.Economy;
global using WarRegions.Controllers;
global using WarRegions.Controllers.Pathfinding;
global using WarRegions.Controllers.Economy;

// أنواع محددة
global using MovementType = WarRegions.Core.Models.Units.MovementType;
global using TerrainTile = WarRegions.Core.Models.Terrain.TerrainTile;
global using UnitCard = WarRegions.Core.Models.Units.UnitCard;