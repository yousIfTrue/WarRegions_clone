#!/bin/bash
echo "🔧 التنظيف النهائي لـ using statements..."

# دالة لتنظيف وإضافة using
fix_file_usings() {
    local file=$1
    local needed_using=$2
    
    if [ -f "$file" ]; then
        # إزالة التكرار أولاً
        awk '!seen[$0]++' "$file" > "${file}.tmp" && mv "${file}.tmp" "$file"
        
        # إضافة using إذا لم تكن موجودة
        if ! grep -q "$needed_using" "$file"; then
            # إضافة بعد آخر using
            if grep -q "using.*;" "$file"; then
                sed -i "/^using.*;/a $needed_using" "$file"
            else
                # إضافة بعد namespace إذا لم يكن هناك using
                sed -i "/^namespace/a $needed_using" "$file"
            fi
            echo "✅ تم إضافة $needed_using لـ $file"
        fi
        echo "✅ تم تنظيف $file"
    fi
}

# تنظيف وإضافة using statements للملفات المحددة
fix_file_usings "Core/Models/Army.cs" "using WarRegions.Core.Models.Units;"
fix_file_usings "Core/Interfaces/IViewManager.cs" "using WarRegions.Core.Models;"
fix_file_usings "Core/Controllers/AIController.cs" "using WarRegions.Core.Models;"
fix_file_usings "Core/Models/Development/DevConfig.cs" "using WarRegions.Core.Models;"

# الملفات التي تحتاج تنظيف التكرار فقط
awk '!seen[$0]++' Core/Controllers/GameManager.cs > Core/Controllers/GameManager.cs.tmp && mv Core/Controllers/GameManager.cs.tmp Core/Controllers/GameManager.cs
awk '!seen[$0]++' Core/Controllers/BattleCalculator.cs > Core/Controllers/BattleCalculator.cs.tmp && mv Core/Controllers/BattleCalculator.cs.tmp Core/Controllers/BattleCalculator.cs
awk '!seen[$0]++' Core/Models/Region.cs > Core/Models/Region.cs.tmp && mv Core/Models/Region.cs.tmp Core/Models/Region.cs
awk '!seen[$0]++' Core/Models/Player.cs > Core/Models/Player.cs.tmp && mv Core/Models/Player.cs.tmp Core/Models/Player.cs
awk '!seen[$0]++' Core/Models/Army.cs > Core/Models/Army.cs.tmp && mv Core/Models/Army.cs.tmp Core/Models/Army.cs

echo "🎉 تم التنظيف والإضافة النهائية!"
