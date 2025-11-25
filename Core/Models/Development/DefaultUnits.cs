
namespace WarRegions.Core.Models.Development
{
    public static class DefaultUnits
    {
        public static List<UnitCard> CreateStarterUnits()
        {
            return new List<UnitCard>
            {
                // سيتم إضافة الوحدات الأساسية هنا لاحقاً
            };
        }
                // ✅ أضف هذه الدالة الجديدة
        public static void AddDefaultUnitsToPlayer(Player player)
        {
            if (player == null) return;
            
            var starterUnits = CreateStarterUnits();
            foreach (var unit in starterUnits)
            {
                // أضف الوحدة للاعب
                // هذا يعتمد على كيفية إضافة الوحدات للاعب في نظامك
                player.AvailableUnits.Add(unit);// أو أي طريقة أخرى مناسبة
            }
            
            Console.WriteLine($"[DEFAULT UNITS] Added {starterUnits.Count} default units to player {player.PlayerName}");
        }
        public static List<UnitCard> CreateEnemyUnits()
        {
            return new List<UnitCard>
            {
                new UnitCard { UnitName = "Infantry", Attack = 10, Defense = 15, Health = 100 },
                new UnitCard { UnitName = "Archer", Attack = 15, Defense = 5, Health = 80 },
                new UnitCard { UnitName = "Cavalry", Attack = 20, Defense = 10, Health = 120 }
            };
        }
    }
}
