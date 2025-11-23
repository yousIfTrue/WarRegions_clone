#!/bin/bash
echo "🛠️ إعادة بناء جميع الملفات التالفة..."

# 1. إصلاح DevConfig.cs
cat > Core/Models/Development/DevConfig.cs << 'DEVEOF'
using WarRegions.Core.Models;

namespace WarRegions.Core.Models.Development
{
    public static class DevConfig
    {
        public const bool EnableDebugMode = true;
        public const bool EnableCheats = false;
        public const int StartingSilver = 1000;
        public const int StartingGold = 100;
        
        // إعدادات التطوير ستضاف هنا لاحقاً
    }
}
DEVEOF

# 2. إصلاح AIController.cs نهائياً
cat > Core/Controllers/AIController.cs << 'AIEOF'
using System;
using WarRegions.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace WarRegions.Core.Controllers
{
    public class AIController
    {
        private GameState _gameState;
        private BattleCalculator _battleCalculator;
        private TerrainManager _terrainManager;
        private Random _random;

        public AIController(GameState gameState, BattleCalculator battleCalculator, TerrainManager terrainManager)
        {
            _gameState = gameState;
            _battleCalculator = battleCalculator;
            _terrainManager = terrainManager;
            _random = new Random();
        }

        public void MakeAIMove(Player aiPlayer)
        {
            // منطق AI سيتم إضافته هنا لاحقاً
        }

        private Army FindStrongestArmy(Player player)
        {
            return null; // مؤقت
        }

        private Region FindWeakestEnemyRegion(Player aiPlayer)
        {
            return null; // مؤقت
        }
    }
}
AIEOF

# 3. إصلاح Player.cs
cat > Core/Models/Player.cs << 'PLAYEREOF'
using System.Collections.Generic;
using WarRegions.Core.Models.Units;

namespace WarRegions.Core.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int SilverCoins { get; set; }
        public int GoldCoins { get; set; }
        public List<UnitCard> AvailableUnits { get; set; }
        public UnitDeck CurrentDeck { get; set; }

        public Player(string name)
        {
            Name = name;
            SilverCoins = 0;
            GoldCoins = 0;
            AvailableUnits = new List<UnitCard>();
            CurrentDeck = new UnitDeck();
        }
    }
}
PLAYEREOF

# 4. إصلاح Army.cs
cat > Core/Models/Army.cs << 'ARMYEOF'
using System.Collections.Generic;
using WarRegions.Core.Models.Units;

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
ARMYEOF

echo "✅ تم إعادة بناء جميع الملفات التالفة!"
