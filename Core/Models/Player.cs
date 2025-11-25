
namespace WarRegions.Core.Models
{
    public class Player
    {
        // ✅ الخصائص الأصلية - محفوظة
        public string Name { get; set; }
        public int SilverCoins { get; set; }
        public int GoldCoins { get; set; }
        public List<UnitCard> AvailableUnits { get; set; }
        public UnitDeck CurrentDeck { get; set; }
        
        // ✅ التأكد من وجود LevelProgress
        public int LevelProgress { get; set; } = 0;
        
        // ✅ إضافة طريقة لحساب التقدم
        public double GetLevelProgressPercentage()
        {
            int expForNextLevel = Level * 100;
            return expForNextLevel > 0 ? (double)LevelProgress / expForNextLevel * 100 : 0;
        }
        // ✅ الخصائص الجديدة المطلوبة فقط
        public string PlayerId { get; set; } = Guid.NewGuid().ToString();
        public string PlayerName 
        { 
            get => Name; 
            set => Name = value; 
        }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int Silver 
        { 
            get => SilverCoins; 
            set => SilverCoins = value; 
        }
        public int Gold 
        { 
            get => GoldCoins; 
            set => GoldCoins = value; 
        }
        public int Morale { get; set; } = 100;
        
        public List<Army> Armies { get; set; } = new List<Army>();
        public UnitInventory UnitInventory { get; set; } = new UnitInventory();
        public UnitDeck ActiveDeck 
        { 
            get => CurrentDeck; 
            set => CurrentDeck = value; 
        }
        
        public int BattlesWon { get; set; } = 0;
        public int BattlesLost { get; set; } = 0;
        public int TotalUnitsRecruited { get; set; } = 0;
        
        public List<string> UnlockedUnits { get; set; } = new List<string>();
        public List<string> CompletedLevels { get; set; } = new List<string>();

        public Player(string name)
        {
            Name = name;
            SilverCoins = 0;
            GoldCoins = 0;
            AvailableUnits = new List<UnitCard>();
            CurrentDeck = new UnitDeck();
            
            // ✅ تهيئة الخصائص الجديدة
            UnitInventory = new UnitInventory();
            Armies = new List<Army>();
        }

        // ✅ Constructor إضافي للتوافق
        public Player() : this("Player")
        {
        }

        // ✅ الطرق الجديدة المطلوبة فقط
        public bool CanAfford(int silverCost, int goldCost = 0)
        {
            return Silver >= silverCost && Gold >= goldCost;
        }
        
        public void SpendResources(int silver, int gold = 0)
        {
            Silver -= silver;
            Gold -= gold;
        }
        
        public void GainResources(int silver, int gold = 0)
        {
            Silver += silver;
            Gold += gold;
        }
        
        public void AddExperience(int exp)
        {
            Experience += exp;
        }
        
        public bool HasUnlockedUnit(string unitType)
        {
            return UnlockedUnits.Contains(unitType);
        }
        
        public void UnlockUnit(string unitType)
        {
            if (!UnlockedUnits.Contains(unitType))
            {
                UnlockedUnits.Add(unitType);
            }
        }
        
        public override string ToString()
        {
            return $"{PlayerName} (Lvl {Level}) - Silver: {Silver}, Gold: {Gold}";
        }
    }
}