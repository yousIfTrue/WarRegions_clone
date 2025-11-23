#!/bin/bash
echo "🏗️ تنظيم جميع الملفات في namespaces صحيحة..."

# دالة لتنظيم ملف
organize_file() {
    local file=$1
    local namespace=$2
    
    echo "🔧 معالجة: $file"
    
    # إنشاء ملف مؤقت
    local temp_file=$(mktemp)
    
    # إضافة using statements شائعة
    cat > "$temp_file" << 'USINGS'
using System;
using System.Collections.Generic;
using System.Linq;

USINGS

    # إضافة namespace والمحتوى
    echo "namespace $namespace" >> "$temp_file"
    echo "{" >> "$temp_file"
    
    # إضافة المحتوى الأصلي مع مسافات
    cat "$file" | sed 's/^/    /' >> "$temp_file"
    
    echo "}" >> "$temp_file"
    
    # استبدال الملف الأصلي
    mv "$temp_file" "$file"
    echo "✅ تم تنظيم: $file → $namespace"
}

# تنظيم ملفات Core Models
organize_file "Core/Models/GameState.cs" "WarRegions.Core.Models"
organize_file "Core/Models/Player.cs" "WarRegions.Core.Models"
organize_file "Core/Models/Army.cs" "WarRegions.Core.Models"
organize_file "Core/Models/Region.cs" "WarRegions.Core.Models"

# تنظيم Models/Economy
organize_file "Core/Models/Economy/Currency.cs" "WarRegions.Core.Models.Economy"
organize_file "Core/Models/Economy/ShopItem.cs" "WarRegions.Core.Models.Economy"
organize_file "Core/Models/Economy/Transaction.cs" "WarRegions.Core.Models.Economy"
organize_file "Core/Models/Economy/UpgradeCost.cs" "WarRegions.Core.Models.Economy"

# تنظيم Models/Level
organize_file "Core/Models/Level/LevelData.cs" "WarRegions.Core.Models.Level"
organize_file "Core/Models/Level/LevelConfig.cs" "WarRegions.Core.Models.Level"
organize_file "Core/Models/Level/SpawnPoint.cs" "WarRegions.Core.Models.Level"

# تنظيم Models/Units
organize_file "Core/Models/Units/UnitCard.cs" "WarRegions.Core.Models.Units"
organize_file "Core/Models/Units/UnitDeck.cs" "WarRegions.Core.Models.Units"
organize_file "Core/Models/Units/UnitInventory.cs" "WarRegions.Core.Models.Units"
organize_file "Core/Models/Units/UnitRarity.cs" "WarRegions.Core.Models.Units"
organize_file "Core/Models/Units/UnitAttributes.cs" "WarRegions.Core.Models.Units"
organize_file "Core/Models/Units/UnitUpgrade.cs" "WarRegions.Core.Models.Units"
organize_file "Core/Models/Units/MovementType.cs" "WarRegions.Core.Models.Units"

# تنظيم Models/Terrain
organize_file "Core/Models/Terrain/TerrainType.cs" "WarRegions.Core.Models.Terrain"
organize_file "Core/Models/Terrain/TerrainTile.cs" "WarRegions.Core.Models.Terrain"

# تنظيم Controllers
organize_file "Core/Controllers/GameManager.cs" "WarRegions.Core.Controllers"
organize_file "Core/Controllers/AIController.cs" "WarRegions.Core.Controllers"
organize_file "Core/Controllers/BattleCalculator.cs" "WarRegions.Core.Controllers"
organize_file "Core/Controllers/DeckManager.cs" "WarRegions.Core.Controllers"
organize_file "Core/Controllers/LevelManager.cs" "WarRegions.Core.Controllers"
organize_file "Core/Controllers/TerrainManager.cs" "WarRegions.Core.Controllers"

# تنظيم Controllers/Economy
organize_file "Core/Controllers/Economy/EconomyManager.cs" "WarRegions.Core.Controllers.Economy"
organize_file "Core/Controllers/Economy/ShopManager.cs" "WarRegions.Core.Controllers.Economy"
organize_file "Core/Controllers/Economy/WorkshopManager.cs" "WarRegions.Core.Controllers.Economy"

# تنظيم Controllers/Pathfinding
organize_file "Core/Controllers/Pathfinding/IPathfinder.cs" "WarRegions.Core.Controllers.Pathfinding"
organize_file "Core/Controllers/Pathfinding/BlockBasedPathfinder.cs" "WarRegions.Core.Controllers.Pathfinding"
organize_file "Core/Controllers/Pathfinding/CentralUnitPathfinder.cs" "WarRegions.Core.Controllers.Pathfinding"
organize_file "Core/Controllers/Pathfinding/HybridPathfinder.cs" "WarRegions.Core.Controllers.Pathfinding"

# تنظيم Interfaces
organize_file "Core/Interfaces/IViewManager.cs" "WarRegions.Core.Interfaces"
organize_file "Core/Interfaces/IPathfinder.cs" "WarRegions.Core.Interfaces"

# تنظيم CLI
organize_file "CLI/Program.cs" "WarRegions.CLI"
organize_file "CLI/CLIViewManager.cs" "WarRegions.CLI"
organize_file "CLI/GameRenderer.cs" "WarRegions.CLI"
organize_file "CLI/InputHandler.cs" "WarRegions.CLI"

echo "🎉 تم تنظيم جميع الملفات!"
