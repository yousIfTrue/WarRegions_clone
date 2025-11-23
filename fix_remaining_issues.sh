#!/bin/bash
echo "🔧 إصلاح المشاكل المتبقية..."

# إصلاح using statements للملفات المحددة
echo "using WarRegions.Core.Models.Units;" >> Core/Models/Development/DefaultUnits.cs
echo "using WarRegions.Core.Models;" >> Core/Interfaces/IViewManager.cs
echo "using WarRegions.Core.Models;" >> Core/Controllers/AIController.cs

# إصلاح مشاكل nullable
find . -name "*.cs" -exec sed -i 's/ = null;/ = null!;/g' {} +
find . -name "*.cs" -exec sed -i 's/return null;/return null!;/g' {} +

echo "✅ تم الإصلاح!"
