#!/bin/bash
echo "🔧 توحيد جميع namespaces إلى WarRegions..."

# تغيير جميع WarRegionsClone إلى WarRegions
find . -name "*.cs" -exec sed -i 's/WarRegionsClone\./WarRegions./g' {} +
find . -name "*.cs" -exec sed -i 's/namespace WarRegionsClone/namespace WarRegions/g' {} +

echo "✅ تم توحيد namespaces!"
