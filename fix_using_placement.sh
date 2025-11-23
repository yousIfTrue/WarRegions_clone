#!/bin/bash
echo "🔧 إصلاح مواقع using statements..."

# دالة لإصلاح ملف
fix_file() {
    local file=$1
    local using_stmt=$2
    
    if [ -f "$file" ]; then
        # حذف الـ using الخاطئة من نهاية الملف
        sed -i "/$using_stmt/d" "$file"
        
        # إضافة الـ using في بداية الملف بعد existing using statements
        if grep -q "using.*;" "$file"; then
            # إضافة بعد آخر using
            sed -i "/^using.*;/a $using_stmt" "$file"
        else
            # إضافة بعد namespace إذا لم يكن هناك using
            sed -i "/^namespace/a $using_stmt" "$file"
        fi
        
        echo "✅ تم إصلاح: $file"
    fi
}

# إصلاح الملفات المحددة
fix_file "Core/Models/Development/DefaultUnits.cs" "using WarRegions.Core.Models.Units;"
fix_file "Core/Interfaces/IViewManager.cs" "using WarRegions.Core.Models;"
fix_file "Core/Controllers/AIController.cs" "using WarRegions.Core.Models;"

echo "🎉 تم إصلاح جميع المواقع!"
