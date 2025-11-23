#!/bin/bash
echo "🔧 إضافة using statements أساسية..."

# دالة لإضافة using لملف
add_using_to_file() {
    local file=$1
    local using_stmt=$2
    
    if [ -f "$file" ] && ! grep -q "$using_stmt" "$file"; then
        # إضافة بعد آخر using مباشرة
        sed -i "/^using.*;/a $using_stmt" "$file"
        echo "✅ $using_stmt → $file"
    fi
}

# CLI تحتاج Core
add_using_to_file "CLI/Program.cs" "using WarRegions.Core.Controllers;"
add_using_to_file "CLI/Program.cs" "using WarRegions.Core.Models;"
add_using_to_file "CLI/CLIViewManager.cs" "using WarRegions.Core.Interfaces;"
add_using_to_file "CLI/CLIViewManager.cs" "using WarRegions.Core.Models;"

# Controllers تحتاج Models
add_using_to_file "Core/Controllers/GameManager.cs" "using WarRegions.Core.Models;"
add_using_to_file "Core/Controllers/AIController.cs" "using WarRegions.Core.Models;"
add_using_to_file "Core/Controllers/BattleCalculator.cs" "using WarRegions.Core.Models;"

# Interfaces تحتاج Models  
add_using_to_file "Core/Interfaces/IViewManager.cs" "using WarRegions.Core.Models;"
add_using_to_file "Core/Interfaces/IPathfinder.cs" "using WarRegions.Core.Models;"

# Models تحتاج بعضها البعض
add_using_to_file "Core/Models/Army.cs" "using WarRegions.Core.Models.Units;"
add_using_to_file "Core/Models/Player.cs" "using WarRegions.Core.Models.Units;"
add_using_to_file "Core/Models/Region.cs" "using WarRegions.Core.Models.Terrain;"

echo "🎉 تم إضافة using statements!"
