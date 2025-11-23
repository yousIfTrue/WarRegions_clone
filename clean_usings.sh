#!/bin/bash
echo "🧹 تنظيف using statements المكررة..."

# دالة لتنظيف ملف
clean_file() {
    local file=$1
    if [ -f "$file" ]; then
        # إزالة الأسطر المكررة مع الحفاظ على الترتيب
        awk '!seen[$0]++' "$file" > "${file}.tmp" && mv "${file}.tmp" "$file"
        echo "✅ تم تنظيف: $file"
    fi
}

# تنظيف الملفات
clean_file "Core/Interfaces/IViewManager.cs"
clean_file "Core/Controllers/AIController.cs"

echo "🎉 تم التنظيف!"
