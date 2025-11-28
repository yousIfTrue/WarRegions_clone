// Core/Models/Army.cs
namespace WarRegions.Core.Models
{
    public class Army
    {
        public string ArmyName { get; set; } = "Unnamed Army";
        public List<UnitCard> Units { get; set; } = new List<UnitCard>();
        public int Supply { get; set; } = 100;
        public int Morale { get; set; } = 100;
        public Player Owner { get; set; }
        public int Experience { get; set; } = 0;
        public Region CurrentRegion { get; set; }
        public bool IsDefeated => !Units.Any(u => u.IsAlive);
            // ✅ أضف هذه الخواص والداالة الجديدة
        public int MovementPoints { get; set; } = 10;
        public int MaxMovementPoints { get; set; } = 10;

        public void ResetMovementPoints()
        {
            MovementPoints = MaxMovementPoints;
        }
        public Army() { }

        public Army(string name, Player owner)
        {
            ArmyName = name;
            Owner = owner;
            MovementPoints = MaxMovementPoints; // ✅ تهيئة نقاط الحركة
        }

        // طرق أساسية
        public int GetAliveUnitCount() => Units.Count(u => u.IsAlive);
        public int GetTotalUnitCount() => Units.Count;
        public double GetStrength() => Units.Sum(u => u.GetCombatStrength());

        public void AddUnit(UnitCard unit)
        {
            Units.Add(unit);
        }

        public void RemoveUnit(UnitCard unit)
        {
            Units.Remove(unit);
        }

        public void HealUnits(int healAmount)
        {
            foreach (var unit in Units)
            {
                // تطبيق العلاج لاحقاً
            }
        }

        public void RestoreSupply()
        {
            Supply = 100;
        }

        public bool IsInRegion(Region region)
        {
            return CurrentRegion?.RegionId == region?.RegionId;
        }

        public override string ToString()
        {
            return $"{ArmyName} ({GetAliveUnitCount()}/{GetTotalUnitCount()} units)";
        }
                /// <summary>
        /// تحقق إذا كان الجيش يمكنه الحركة إلى موقع معين
        /// </summary>
        /// <param name="targetPosition">الموقع المستهدف</param>
        /// <param name="terrainManager">مدير التضاريس للتحقق من العوائق</param>
        /// <returns>true إذا كان الحركة ممكنة</returns>
        public bool CanMoveTo(Vector2 targetPosition, TerrainManager terrainManager = null)
        {
            // إذا لم يكن هناك نقاط حركة متبقية
            if (MovementPoints <= 0)
            {
                Console.WriteLine($"[ARMY] {ArmyName} cannot move - no movement points left");
                return false;
            }

            // إذا كان الجيش منهزم
            if (IsDefeated)
            {
                Console.WriteLine($"[ARMY] {ArmyName} cannot move - army is defeated");
                return false;
            }

            // إذا كان الموقع الحالي غير محدد
            if (CurrentRegion?.Position == null)
            {
                Console.WriteLine($"[ARMY] {ArmyName} cannot move - current position unknown");
                return false;
            }

            // حساب المسافة (استخدام Manhattan distance للسهولة)
            float distance = Math.Abs(targetPosition.X - CurrentRegion.Position.X) + Math.Abs(targetPosition.Y - CurrentRegion.Position.Y);
    
            // التحقق إذا كانت المسافة ممكنة بنقاط الحركة المتاحة
            bool canMove = distance <= MovementPoints;

            // التحقق من العوائق إذا وجد مدير التضاريس
            if (canMove && terrainManager != null)
            {
                canMove = terrainManager.IsPositionPassable(targetPosition);
            }

            Console.WriteLine($"[ARMY] {ArmyName} can move to ({targetPosition.X},{targetPosition.Y}): {canMove}");
            return canMove;
        }
        /// <summary>
/// حرك الجيش إلى موقع معين
/// </summary>
/// <param name="targetPosition">الموقع المستهدف</param>
/// <param name="terrainManager">مدير التضاريس</param>
/// <returns>true إذا تمت الحركة بنجاح</returns>
public bool MoveTo(Vector2 targetPosition, TerrainManager terrainManager = null)
{
    // التحقق أولاً إذا كانت الحركة ممكنة
    if (!CanMoveTo(targetPosition, terrainManager))
    {
        Console.WriteLine($"[ARMY] {ArmyName} movement failed - cannot move to target");
        return false;
    }

    // حساب المسافة لاستهلاك نقاط الحركة
    float distance = Math.Abs(targetPosition.X - CurrentRegion.Position.X) + 
                    Math.Abs(targetPosition.Y - CurrentRegion.Position.Y);
    
    // استهلاك نقاط الحركة
    MovementPoints -= (int)distance;
    
    // تحديث الموقع
    Vector2 oldPosition = CurrentRegion.Position;
    CurrentRegion.Position = targetPosition;

    Console.WriteLine($"[ARMY] {ArmyName} moved from ({oldPosition.X},{oldPosition.Y}) to ({targetPosition.X},{targetPosition.Y})");
    Console.WriteLine($"[ARMY] {ArmyName} movement points remaining: {MovementPoints}");

    // تطبيق تأثيرات التضاريس إذا وجدت
    if (terrainManager != null)
    {
        ApplyTerrainEffects(terrainManager.GetTerrainAt(targetPosition));
    }

    return true;
}
/// <summary>
/// تطبيق خسائر على الجيش
/// </summary>
/// <param name="casualties">عدد الخسائر</param>
/// <param name="isPercentage">إذا كان true الخسائر كنسبة مئوية</param>
public void TakeCasualties(int casualties, bool isPercentage = false)
{
    if (IsDefeated || casualties <= 0) return;

    int actualCasualties = casualties;

    if (isPercentage)
    {
        // حساب الخسائر كنسبة مئوية من إجمالي الوحدات
        actualCasualties = (int)Math.Ceiling(Units.Count * (casualties / 100.0));
    }

    // لا يمكن أن تتجاوز الخسائر عدد الوحدات الحية
    actualCasualties = Math.Min(actualCasualties, GetAliveUnitCount());

    // تطبيق الخسائر عشوائياً على الوحدات
    var aliveUnits = Units.Where(u => u.IsAlive).ToList();
    var random = new Random();

    for (int i = 0; i < actualCasualties && aliveUnits.Count > 0; i++)
    {
        int index = random.Next(aliveUnits.Count);
        var unit = aliveUnits[index];
        
        // في implementation حقيقي، هنا يتم تدمير الوحدة أو تقليل صحتها
        // unit.TakeDamage(unit.Health); // لتدمير الوحدة كلياً
        aliveUnits.RemoveAt(index);
    }

    // تحديث معنويات الجيش
    Morale = Math.Max(0, Morale - (actualCasualties * 2));

    Console.WriteLine($"[ARMY] {ArmyName} took {actualCasualties} casualties. Morale: {Morale}");
    
    // إذا لم يعد هناك وحدات حية، الجيش منهزم
    if (GetAliveUnitCount() == 0)
    {
        Console.WriteLine($"[ARMY] {ArmyName} has been defeated!");
    }
}
    }
}