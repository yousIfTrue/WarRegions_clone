#!/bin/bash

# سكريبت إنشاء هيكل مشروع War Regions Clone
# الإصدار النهائي مع كل الأنظمة وهياكل الملفات
# لا يملأ الملفات بأي محتوى - هياكل فقط

set -e  # إيقاف عند أول خطأ

PROJECT_NAME="WarRegions_Clone"

echo "إنشاء الهيكل النهائي لمشروع $PROJECT_NAME..."

# تنظيف المشروع القديم
if [ -d "$PROJECT_NAME" ]; then
    echo "إزالة المشروع القديم..."
    rm -rf "$PROJECT_NAME"
fi

# إنشاء الهيكل الأساسي
echo "إنشاء المجلدات والملفات..."

# المجلد الجذري
mkdir -p "$PROJECT_NAME"

# المجلدات الأساسية
mkdir -p "$PROJECT_NAME/Levels"
mkdir -p "$PROJECT_NAME/Core/Models/Economy"
mkdir -p "$PROJECT_NAME/Core/Models/Level"
mkdir -p "$PROJECT_NAME/Core/Models/Units"
mkdir -p "$PROJECT_NAME/Core/Models/Terrain"
mkdir -p "$PROJECT_NAME/Core/Models/Development"
mkdir -p "$PROJECT_NAME/Core/Controllers/Pathfinding"
mkdir -p "$PROJECT_NAME/Core/Controllers/Economy"
mkdir -p "$PROJECT_NAME/Core/Interfaces"
mkdir -p "$PROJECT_NAME/Presentation/Interface2D/Scenes"
mkdir -p "$PROJECT_NAME/Presentation/Interface2D/Scripts"
mkdir -p "$PROJECT_NAME/Presentation/Interface2D/Resources/Sprites"
mkdir -p "$PROJECT_NAME/Presentation/Interface2D/Resources/Prefabs"
mkdir -p "$PROJECT_NAME/Presentation/Interface3D/Scenes"
mkdir -p "$PROJECT_NAME/Presentation/Interface3D/Scripts"
mkdir -p "$PROJECT_NAME/Presentation/Interface3D/Resources/Models"
mkdir -p "$PROJECT_NAME/Presentation/Interface3D/Resources/Materials"
mkdir -p "$PROJECT_NAME/Presentation/Interface3D/Resources/Prefabs"
mkdir -p "$PROJECT_NAME/Tools"
mkdir -p "$PROJECT_NAME/Settings"

# إنشاء ملفات المستويات
touch "$PROJECT_NAME/Levels/Level_01.json"
touch "$PROJECT_NAME/Levels/Level_02.json"
touch "$PROJECT_NAME/Levels/Template.json"

# إنشاء ملفات الـ Models
## Economy
touch "$PROJECT_NAME/Core/Models/Economy/Currency.cs"
touch "$PROJECT_NAME/Core/Models/Economy/ShopItem.cs"
touch "$PROJECT_NAME/Core/Models/Economy/Transaction.cs"
touch "$PROJECT_NAME/Core/Models/Economy/UpgradeCost.cs"

## Level
touch "$PROJECT_NAME/Core/Models/Level/LevelData.cs"
touch "$PROJECT_NAME/Core/Models/Level/LevelConfig.cs"
touch "$PROJECT_NAME/Core/Models/Level/SpawnPoint.cs"

## Units
touch "$PROJECT_NAME/Core/Models/Units/UnitCard.cs"
touch "$PROJECT_NAME/Core/Models/Units/UnitDeck.cs"
touch "$PROJECT_NAME/Core/Models/Units/UnitInventory.cs"
touch "$PROJECT_NAME/Core/Models/Units/UnitRarity.cs"
touch "$PROJECT_NAME/Core/Models/Units/UnitAttributes.cs"
touch "$PROJECT_NAME/Core/Models/Units/UnitUpgrade.cs"
touch "$PROJECT_NAME/Core/Models/Units/MovementType.cs"

## Terrain
touch "$PROJECT_NAME/Core/Models/Terrain/TerrainType.cs"
touch "$PROJECT_NAME/Core/Models/Terrain/TerrainTile.cs"
touch "$PROJECT_NAME/Core/Models/Terrain/TerrainEffect.cs"

## Development
touch "$PROJECT_NAME/Core/Models/Development/DevConfig.cs"
touch "$PROJECT_NAME/Core/Models/Development/DefaultUnits.cs"

## Core Models
touch "$PROJECT_NAME/Core/Models/Player.cs"
touch "$PROJECT_NAME/Core/Models/Region.cs"
touch "$PROJECT_NAME/Core/Models/Army.cs"
touch "$PROJECT_NAME/Core/Models/GameState.cs"

# إنشاء ملفات الـ Controllers
## Pathfinding
touch "$PROJECT_NAME/Core/Controllers/Pathfinding/IPathfinder.cs"
touch "$PROJECT_NAME/Core/Controllers/Pathfinding/CentralUnitPathfinder.cs"
touch "$PROJECT_NAME/Core/Controllers/Pathfinding/BlockBasedPathfinder.cs"
touch "$PROJECT_NAME/Core/Controllers/Pathfinding/HybridPathfinder.cs"

## Economy
touch "$PROJECT_NAME/Core/Controllers/Economy/ShopManager.cs"
touch "$PROJECT_NAME/Core/Controllers/Economy/WorkshopManager.cs"
touch "$PROJECT_NAME/Core/Controllers/Economy/EconomyManager.cs"

## Main Controllers
touch "$PROJECT_NAME/Core/Controllers/LevelManager.cs"
touch "$PROJECT_NAME/Core/Controllers/DeckManager.cs"
touch "$PROJECT_NAME/Core/Controllers/TerrainManager.cs"
touch "$PROJECT_NAME/Core/Controllers/GameManager.cs"
touch "$PROJECT_NAME/Core/Controllers/BattleCalculator.cs"
touch "$PROJECT_NAME/Core/Controllers/AIController.cs"

# إنشاء ملفات الـ Interfaces
touch "$PROJECT_NAME/Core/Interfaces/IViewManager.cs"
touch "$PROJECT_NAME/Core/Interfaces/IPathfinder.cs"

# إنشاء ملفات الواجهات
## 2D Interface
touch "$PROJECT_NAME/Presentation/Interface2D/Scripts/ViewManager2D.cs"
touch "$PROJECT_NAME/Presentation/Interface2D/Scripts/RegionView2D.cs"
touch "$PROJECT_NAME/Presentation/Interface2D/Scripts/ArmyView2D.cs"
touch "$PROJECT_NAME/Presentation/Interface2D/Scenes/MainScene2D.unity"

## 3D Interface
touch "$PROJECT_NAME/Presentation/Interface3D/Scripts/ViewManager3D.cs"
touch "$PROJECT_NAME/Presentation/Interface3D/Scripts/RegionView3D.cs"
touch "$PROJECT_NAME/Presentation/Interface3D/Scripts/ArmyView3D.cs"
touch "$PROJECT_NAME/Presentation/Interface3D/Scenes/MainScene3D.unity"

# إنشاء ملفات الإعدادات
touch "$PROJECT_NAME/Settings/GameSettings.asset"
touch "$PROJECT_NAME/Settings/GraphicsSettings.asset"

# نسخ السكريبت الحالي إلى Tools
cp "$0" "$PROJECT_NAME/Tools/create_project.sh"
chmod +x "$PROJECT_NAME/Tools/create_project.sh"

# إنشاء ملف README
cat > "$PROJECT_NAME/README.md" << 'EOF'
# War Regions Clone - الهيكل النهائي

## الهيكل التنظيمي الكامل

### الأنظمة الرئيسية:
- **Core/**: المنطق الأساسي للعبة
- **Levels/**: ملفات المستويات
- **Presentation/**: واجهات العرض (2D/3D)
- **Settings/**: إعدادات اللعبة

### مراحل التطوير:
1. المرحلة 1: المعارك والوحدات (الأولوية)
2. المرحلة 2: نظام البطاقات والمجموعات
3. المرحلة 3: النظام الاقتصادي والمتاجر

### ملاحظات:
- جميع الملفات فارغة وجاهزة للبرمجة
- نظام الاقتصاد والمتاجر غير مفعل حالياً
- التركيز على Core/Models/Units و Core/Controllers أولاً
EOF

# عرض الإحصائيات النهائية
echo "✅ تم إنشاء الهيكل بنجاح!"
echo ""
echo "📊 إحصائيات المشروع:"
echo "   📁 المجلدات: $(find "$PROJECT_NAME" -type d | wc -l)"
echo "   📄 الملفات: $(find "$PROJECT_NAME" -type f | wc -l)"
echo ""
echo "🎯 الأنظمة الجاهزة:"
echo "   • نظام الوحدات والبطاقات"
echo "   • نظام المستويات الديناميكي"
echo "   • نظام التضاريس والتتبع"
echo "   • النظام الاقتصادي (غير مفعل)"
echo "   • واجهات 2D/3D القابلة للتبديل"
echo ""
echo "🚀 يمكنك البدء بالبرمجة الآن!"
echo "   ركز على: Core/Models/Units/ و Core/Controllers/"
